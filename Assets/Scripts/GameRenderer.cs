using UnityEngine;

public class GameRenderer : MonoBehaviour
{
    public GameObject square;
    public Game game;

    private GameSquare[,] gameStateMatrix;

    public Sprite blank;

    public Sprite redPillNone;
    public Sprite bluePillNone;
    public Sprite yellowPillNone;

    public Sprite redPillJoined;
    public Sprite bluePillJoined;
    public Sprite yellowPillJoined;

    public Sprite redVirus;
    public Sprite blueVirus;
    public Sprite yellowVirus;

    private int width;
    private int height;

    private SpriteRenderer[,] tiles;


    private void OnEnable()
    {
        game.OnGameReset += Reset;
    }

    private void OnDisable()
    {
        game.OnGameReset -= Reset;
    }

    private void Reset()
    {
        gameStateMatrix = game.GetStateMatrix();
        if(tiles == null)
        {
            width = game.GetWidth();
            height = game.GetHeight();
            transform.position = new Vector3(-width / 2.0f + 0.5f, -height / 2.0f + 0.5f, 0);
            gameStateMatrix = game.GetStateMatrix();
            tiles = new SpriteRenderer[height, width];
            for (int row = height - 1; row >= 0; --row)
            {
                for (int col = 0; col < width; ++col)
                {
                    tiles[row, col] = Instantiate(square, Vector3.zero, Quaternion.identity).GetComponent<SpriteRenderer>();
                    tiles[row, col].transform.parent = transform;
                    tiles[row, col].transform.localPosition = new Vector3(col, row, 0);
                }
            }
        }
        
    }

    void Update()
    {
        for (int row = height - 1; row >= 0; --row)
        {
            for (int col = 0; col < width; ++col)
            {
                tiles[row, col].sprite = GetSprite(gameStateMatrix[row, col]);
                tiles[row, col].transform.eulerAngles = GetEulerRotation(gameStateMatrix[row, col]);
            }
        }
    }

    Sprite GetSprite(GameSquare square)
    {
        if (square == null)
        {
            return blank;
        }
        if(square.type == Type.VIRUS)
        {
            switch (square.color)
            {
                case Color.RED:
                    return redVirus;
                case Color.BLUE:
                    return blueVirus;
                case Color.YELLOW:
                    return yellowVirus;
            }
        } else if(square.joinedAt == JoinDirection.NONE)
        {
            switch (square.color)
            {
                case Color.RED:
                    return redPillNone;
                case Color.BLUE:
                    return bluePillNone;
                case Color.YELLOW:
                    return yellowPillNone;
            }
        } else
        {
            switch (square.color)
            {
                case Color.RED:
                    return redPillJoined;
                case Color.BLUE:
                    return bluePillJoined;
                case Color.YELLOW:
                    return yellowPillJoined;
            }
        }
        return blank;
    }

    Vector3 GetEulerRotation(GameSquare square)
    {
        if (square == null || square.type == Type.VIRUS || (square.type == Type.PILL && square.joinedAt == JoinDirection.NONE))
        {
            return Vector3.zero;
        }
        float z = 0;
        switch (square.joinedAt)
        {
            case JoinDirection.UP:
                z = 90;
                break;
            case JoinDirection.LEFT:
                z = 180;
                break;
            case JoinDirection.DOWN:
                z = 270;
                break;
        }
        return new Vector3(0, 0, z);
    }
}

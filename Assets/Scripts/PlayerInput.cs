using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [SerializeField]
    private string left = "left";

    [SerializeField]
    private string right = "right";

    [SerializeField]
    private string down = "down";

    [SerializeField]
    private string rotateLeft = "z";

    [SerializeField]
    private string rotateRight = "x";

    [SerializeField]
    private string wait = "c";

    [SerializeField]
    private string reset = "r";

    [SerializeField]
    private GameObject gameManager;

    private GameAPI game;

    private void Start()
    {
        foreach(GameAPI api in gameManager.GetComponents<GameAPI>())
        {
            if(api.enabled)
            {
                game = api;
                break;
            }
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(left))
        {
            game.MoveLeft();
        }

        if (Input.GetKeyDown(right))
        {
            game.MoveRight();
        }

        if (Input.GetKeyDown(down))
        {
            game.MoveDown();
        }

        if (Input.GetKeyDown(rotateLeft))
        {
            game.RotateLeft();
        }

        if (Input.GetKeyDown(rotateRight))
        {
            game.RotateRight();
        }

        if (Input.GetKeyDown(wait))
        {
            game.Wait();
        }

        if (Input.GetKeyDown(reset))
        {
            game.ResetGame(Config.width, Config.height, Config.virusHeight, Config.virusCount);
        }
    }
}

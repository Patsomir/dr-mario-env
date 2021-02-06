using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

public class Game : MonoBehaviour
{
    private int width;
    private int height;
    private int virusHeight;
    private int virusCount;

    public bool singleColorPillsOnly = false;


    private GameState gameState;

    private int spawnRow;
    private int spawnCol;

    private int currentPillRow;
    private int currentPillCol;

    private bool playerCanPlay = true;

    public UnityAction OnGameEnd;
    public UnityAction OnGameReset;

    enum TerminationState { WIN, LOSS, ONGOING };
    private TerminationState terminationState = TerminationState.ONGOING;

    public void ResetGame(int width, int height, int virusHeight, int virusCount)
    {
        this.width = width;
        this.height = height;
        this.virusCount = virusCount;
        this.virusHeight = virusHeight;

        gameState = new GameState(width, height);
        spawnRow = height - 1;
        spawnCol = width / 2 - 1;
        terminationState = TerminationState.ONGOING;

        SpawnViruses(virusCount, virusHeight);
        StartTurn();

        OnGameReset?.Invoke();
    }

    public bool PlayerCanPlay()
    {
        return playerCanPlay;
    }

    public bool PlayerHasWon()
    {
        return terminationState == TerminationState.WIN;
    }

    public bool PlayerHasLost()
    {
        return terminationState == TerminationState.LOSS;
    }

    public bool GameHasEnded()
    {
        return terminationState != TerminationState.ONGOING;
    }

    public void SpawnViruses(int virusCount, int virusHeight)
    {
        int viruses = 0;
        while(viruses < virusCount)
        {
            int row = Random.Range(0, virusHeight);
            int col = Random.Range(0, width);
            if(gameState.IsOccupied(row, col))
            {
                continue;
            }
            gameState.SpawnVirus(row, col, (Color) Random.Range(0, 3));
            ++viruses;
        }
    }

    void StartTurn()
    {
        playerCanPlay = true;
        if(gameState.IsOccupied(spawnRow, spawnCol) || gameState.IsOccupied(spawnRow, spawnCol + 1))
        {
            terminationState = TerminationState.LOSS;
            playerCanPlay = false;
            OnGameEnd?.Invoke();
            return;
        }
        Color primaryColor = (Color)Random.Range(0, 3);
        Color secondaryColor = (Color)Random.Range(0, 3);
        if(singleColorPillsOnly)
        {
            secondaryColor = primaryColor;
        }
        gameState.SpawnPill(spawnRow, spawnCol, primaryColor, secondaryColor);
        currentPillRow = spawnRow;
        currentPillCol = spawnCol;
    }

    void EndTurn()
    {
        playerCanPlay = false;
    }

    public bool MoveLeft()
    {
        if (playerCanPlay && gameState.CanMoveLeft(currentPillRow, currentPillCol))
        {
            gameState.MoveLeft(currentPillRow, currentPillCol);
            --currentPillCol;
            return true;
        }
        return false;
    }

    public bool MoveRight()
    {
        if (playerCanPlay && gameState.CanMoveRight(currentPillRow, currentPillCol))
        {
            gameState.MoveRight(currentPillRow, currentPillCol);
            ++currentPillCol;
            return true;
        }
        return false;
    }

    public bool MoveDown()
    {
        if (playerCanPlay && gameState.CanMoveDown(currentPillRow, currentPillCol))
        {
            gameState.MoveDown(currentPillRow, currentPillCol);
            --currentPillRow;
            return true;
        }
        return false;
    }

    public bool RotateLeft()
    {
        if (playerCanPlay && gameState.CanRotate(currentPillRow, currentPillCol))
        {
            gameState.RotateLeft(currentPillRow, currentPillCol);
            return true;
        }
        return false;
    }

    public bool RotateRight()
    {
        if (playerCanPlay && gameState.CanRotate(currentPillRow, currentPillCol))
        {
            gameState.RotateRight(currentPillRow, currentPillCol);
            return true;
        }
        return false;
    }

    public void Wait()
    {
        if (playerCanPlay && gameState.CanMoveDown(currentPillRow, currentPillCol))
        {
            --currentPillRow;
        }
        else
        {
            EndTurn();
        }
        if (!gameState.UpdateState())
        {
            if (gameState.GetVirusCount() == 0)
            {
                terminationState = TerminationState.WIN;
                OnGameEnd?.Invoke();
                return;
            }
            StartTurn();
        }
    }

    public GameSquare[,] GetStateMatrix()
    {
        return gameState.GetStateMatrix();
    }

    public int GetWidth()
    {
        return gameState.GetWidth();
    }

    public int GetHeight()
    {
        return gameState.GetHeight();
    }

    public (int, int) GetPlayerPosition()
    {
        return (currentPillRow, currentPillCol);
    }
}

public enum Type { PILL, VIRUS };
public enum JoinDirection { UP, DOWN, LEFT, RIGHT, NONE };
public enum Color { RED, BLUE, YELLOW };

public class GameSquare
{
    public Type type;
    public JoinDirection joinedAt;
    public Color color;
    public bool isFalling;
};

class GameState
{
    private GameSquare[,] squares;
    private int width;
    private int height;

    private List<(int, int)> destroyBuffer;

    public GameState(int width, int height)
    {
        this.width = width;
        this.height = height;
        squares = new GameSquare[height, width];

        destroyBuffer = new List<(int, int)>();
    }

    public void SpawnPill(int row, int col, Color core, Color secondary)
    {
        squares[row, col] = new GameSquare
        {
            type = Type.PILL,
            joinedAt = JoinDirection.RIGHT,
            color = core,
            isFalling = true,
        };
        squares[row, col + 1] = new GameSquare
        {
            type = Type.PILL,
            joinedAt = JoinDirection.LEFT,
            color = secondary,
            isFalling = true,
        };
    }

    public void SpawnVirus(int row, int col, Color color)
    {
        squares[row, col] = new GameSquare
        {
            type = Type.VIRUS,
            joinedAt = JoinDirection.NONE,
            color = color,
            isFalling = false,
        };
    }

    public bool UpdateState()
    {
        bool stateHasChanged = false;
        for(int row = 0; row < height; ++row)
        {
            for (int col = 0; col < width; ++col)
            {
                if(UpdateSquare(row, col))
                {
                    stateHasChanged = true;
                }
            }
        }
        if(destroyBuffer.Count > 0)
        {
            ExecuteDestroyBuffer();
            EvaluateFalling();
        }
        return stateHasChanged;
    }

    public bool IsValidSquare(int row, int col)
    {
        return row >= 0 && row < height && col >= 0 && col < width;
    }

    public bool IsOccupied(int row, int col)
    {
        return squares[row, col] != null;
    }

    public bool IsPillCore(int row, int col)
    {
        return squares[row, col] != null
            && squares[row, col].type == Type.PILL
            && (squares[row, col].joinedAt == JoinDirection.NONE
            || squares[row, col].joinedAt == JoinDirection.UP
            || squares[row, col].joinedAt == JoinDirection.RIGHT);
    }

    public bool IsFalling(int row, int col)
    {
        return squares[row, col].isFalling;
    }

    public bool IsVirus(int row, int col)
    {
        return squares[row, col] != null && squares[row, col].type == Type.VIRUS;
    }

    public int GetVirusCount()
    {
        int viruses = 0;
        for (int row = 0; row < height; ++row)
        {
            for (int col = 0; col < width; ++col)
            {
                if (IsVirus(row, col))
                {
                    ++viruses;
                }
            }
        }
        return viruses;
    }

    public bool UpdateSquare(int row, int col)
    {
        if (!IsPillCore(row, col) || !squares[row, col].isFalling)
        {
            return false;
        }

        if (CanMoveDown(row, col))
        {
            MoveDown(row, col);
            return true;
        }

        Place(row, col);
        return destroyBuffer.Count > 0;
    }

    // assumes falling pill core
    public bool CanMoveDown(int row, int col)
    {
        if (row == 0 || (squares[row - 1, col] != null && !squares[row - 1, col].isFalling))
        {
            return false;
        }
        if (squares[row, col].joinedAt == JoinDirection.RIGHT)
        {
            return squares[row - 1, col + 1] == null || squares[row - 1, col + 1].isFalling;
        }
        return true;
    }

    // assumes pill core that can move down
    public void MoveDown(int row, int col)
    {
        squares[row - 1, col] = squares[row, col];
        squares[row, col] = null;

        switch (squares[row - 1, col].joinedAt)
        {
            case JoinDirection.UP:
                squares[row, col] = squares[row + 1, col];
                squares[row + 1, col] = null;
                break;
            case JoinDirection.RIGHT:
                squares[row - 1, col + 1] = squares[row, col + 1];
                squares[row, col + 1] = null;
                break;
        }
    }

    // assumes pill core
    public bool CanMoveLeft(int row, int col)
    {
        if (col == 0 || squares[row , col - 1] != null)
        {
            return false;
        }
        if (squares[row, col].joinedAt == JoinDirection.UP)
        {
            return squares[row + 1, col - 1] == null;
        }
        return true;
    }

    // assumes pill core that can move left
    public void MoveLeft(int row, int col)
    {
        squares[row, col - 1] = squares[row, col];
        squares[row, col] = null;

        switch (squares[row, col - 1].joinedAt)
        {
            case JoinDirection.UP:
                squares[row + 1, col - 1] = squares[row + 1, col];
                squares[row + 1, col] = null;
                break;
            case JoinDirection.RIGHT:
                squares[row, col] = squares[row, col + 1];
                squares[row, col + 1] = null;
                break;
        }
    }

    // assumes pill core
    public bool CanMoveRight(int row, int col)
    {
        if (squares[row, col].joinedAt == JoinDirection.UP)
        {
            return col <= width - 2 && squares[row, col + 1] == null && squares[row + 1, col + 1] == null;
        }
        if (squares[row, col].joinedAt == JoinDirection.RIGHT)
        {
            return col <= width - 3 && squares[row, col + 2] == null;
        }
        return col <= width - 2 && squares[row, col + 1] == null;
    }

    // assumes pill core that can move right
    public void MoveRight(int row, int col)
    {
        switch (squares[row, col].joinedAt)
        {
            case JoinDirection.UP:
                squares[row, col + 1] = squares[row, col];
                squares[row, col] = null;
                squares[row + 1, col + 1] = squares[row + 1, col];
                squares[row + 1, col] = null;
                break;
            case JoinDirection.RIGHT:
                squares[row, col + 2] = squares[row, col + 1];
                squares[row, col + 1] = null;
                squares[row, col + 1] = squares[row, col];
                squares[row, col] = null;
                break;
        }
    }

    // assumes pill core
    public bool CanRotate(int row, int col)
    {
        switch (squares[row, col].joinedAt)
        {
            case JoinDirection.UP:
                return col < width - 1 && squares[row, col + 1] == null;
            case JoinDirection.RIGHT:
                return row < height - 1 && squares[row + 1, col] == null;
        }
        return true;
    }

    // assumes pill core that can rotate
    public void RotateLeft(int row, int col)
    {
        switch (squares[row, col].joinedAt)
        {
            case JoinDirection.UP:
                squares[row, col + 1] = squares[row, col];
                squares[row, col + 1].joinedAt = JoinDirection.LEFT;
                squares[row, col] = squares[row + 1, col];
                squares[row, col].joinedAt = JoinDirection.RIGHT;
                squares[row + 1, col] = null;
                break;
            case JoinDirection.RIGHT:
                squares[row + 1, col] = squares[row, col + 1];
                squares[row + 1, col].joinedAt = JoinDirection.DOWN;
                squares[row, col].joinedAt = JoinDirection.UP;
                squares[row, col + 1] = null;
                break;
        }
    }

    // assumes pill core that can rotate
    public void RotateRight(int row, int col)
    {
        switch (squares[row, col].joinedAt)
        {
            case JoinDirection.UP:
                squares[row, col + 1] = squares[row + 1, col];
                squares[row, col + 1].joinedAt = JoinDirection.LEFT;
                squares[row, col].joinedAt = JoinDirection.RIGHT;
                squares[row + 1, col] = null;
                break;
            case JoinDirection.RIGHT:
                squares[row + 1, col] = squares[row, col];
                squares[row + 1, col].joinedAt = JoinDirection.DOWN;
                squares[row, col] = squares[row, col + 1];
                squares[row, col].joinedAt = JoinDirection.UP;
                squares[row, col + 1] = null;
                break;
        }
    }

    public void PlaceHorizontal(int row, int col)
    {
        Color targetColor = squares[row, col].color;
        int min = col - 1;
        int max = col + 1;
        while (max < width && squares[row, max] != null && !squares[row, max].isFalling && squares[row, max].color == targetColor)
        {
            ++max;
        }
        while (min >= 0 && squares[row, min] != null && !squares[row, min].isFalling && squares[row, min].color == targetColor)
        {
            --min;
        }
        if(max - min > 4)
        {
            for(int i = min + 1; i < max; ++i)
            {
                destroyBuffer.Add((row, i));
            }
        }
    }

    public void PlaceVertical(int row, int col)
    {
        Color targetColor = squares[row, col].color;
        int min = row - 1;
        int max = row + 1;
        while (max < height && squares[max, col] != null && !squares[max, col].isFalling && squares[max, col].color == targetColor)
        {
            ++max;
        }
        while (min >= 0 && squares[min, col] != null && !squares[min, col].isFalling && squares[min, col].color == targetColor)
        {
            --min;
        }
        if (max - min > 4)
        {
            for (int i = min + 1; i < max; ++i)
            {
                destroyBuffer.Add((i, col));
            }
        }
    }

    public void Place(int row, int col)
    {
        squares[row, col].isFalling = false;

        switch (squares[row, col].joinedAt)
        {
            case JoinDirection.UP:
                squares[row + 1, col].isFalling = false;
                PlaceHorizontal(row + 1, col);
                PlaceVertical(row + 1, col);
                break;
            case JoinDirection.RIGHT:
                squares[row, col + 1].isFalling = false;
                PlaceHorizontal(row, col + 1);
                PlaceVertical(row, col + 1);
                break;
        }
        PlaceHorizontal(row, col);
        PlaceVertical(row, col);
    }

    public void ExecuteDestroyBuffer()
    {
        destroyBuffer.ForEach(DestorySquare);
        destroyBuffer.Clear();
    }

    public void DestorySquare((int, int) square)
    {
        int row = square.Item1;
        int col = square.Item2;
        if(squares[row, col] == null)
        {
            return;
        }
        switch (squares[row, col].joinedAt)
        {
            case JoinDirection.UP:
                squares[row + 1, col].joinedAt = JoinDirection.NONE;
                break;
            case JoinDirection.RIGHT:
                squares[row, col + 1].joinedAt = JoinDirection.NONE;
                break;
            case JoinDirection.DOWN:
                squares[row - 1, col].joinedAt = JoinDirection.NONE;
                break;
            case JoinDirection.LEFT:
                squares[row, col - 1].joinedAt = JoinDirection.NONE;
                break;
        }
        squares[row, col] = null;
    }



    public void EvaluateFalling()
    {
        for (int row = 0; row < height; ++row)
        {
            for (int col = 0; col < width; ++col)
            {
                if(IsPillCore(row, col) && CanMoveDown(row, col))
                {
                    squares[row, col].isFalling = true;
                    switch (squares[row, col].joinedAt)
                    {
                        case JoinDirection.UP:
                            squares[row + 1, col].isFalling = true;
                            break;
                        case JoinDirection.RIGHT:
                            squares[row, col + 1].isFalling = true;
                            break;
                    }
                }
            }
        }
    }

    public override string ToString()
    {
        StringBuilder result = new StringBuilder();
        for (int row = height - 1; row >= 0; --row)
        {
            for (int col = 0; col < width; ++col)
            {
                if(squares[row, col] == null)
                {
                    result.Append(' ');
                } else
                {
                    switch(squares[row, col].color)
                    {
                        case Color.RED:
                            result.Append('R');
                            break;
                        case Color.BLUE:
                            result.Append('B');
                            break;
                        case Color.YELLOW:
                            result.Append('Y');
                            break;
                    }
                }
            }
            result.Append('\n');
        }
        return result.ToString();
    }

    public GameSquare[,] GetStateMatrix()
    {
        return squares;
    }

    public int GetWidth()
    {
        return width;
    }
    public int GetHeight()
    {
        return height;
    }
};

public abstract class GameAPI : MonoBehaviour
{
    abstract public void MoveLeft();
    abstract public void MoveRight();
    abstract public void MoveDown();
    abstract public void RotateLeft();
    abstract public void RotateRight();
    abstract public void Wait();
    abstract public void ResetGame(int width, int height, int virusHeight, int virusCount);
}
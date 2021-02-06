using System.Text;
using UnityEngine;

public class GameMonitorController : MonoBehaviour, Controller, Monitor
{
    [SerializeField]
    private Game gameManager = null;
    private GameAPI api = null;
    private Game game = null;

    [SerializeField]
    private RemoteInput remote = null;

    public void Execute(string action)
    {
        if (action.Equals("QT"))
        {
            remote.Disconnect();
        }
        else if (action.Equals("RS"))
        {
            api.ResetGame(Config.width, Config.height, Config.virusHeight, Config.virusCount);
        }
        else if (action[0] == 'M')
        {
            switch (action[1])
            {
                case 'L':
                    game.MoveLeft();
                    break;
                case 'R':
                    game.MoveRight();
                    break;
                case 'D':
                    game.MoveDown();
                    break;
            }
        }
        else if (action[0] == 'R')
        {
            switch (action[1])
            {
                case 'L':
                    api.RotateLeft();
                    break;
                case 'R':
                    api.RotateRight();
                    break;
            }
        }
        else if (action.Equals("WT"))
        {
            if(Config.stepMode)
            {
                do
                {
                    api.Wait();
                } while (!game.PlayerCanPlay() && !game.GameHasEnded());
            } else
            {
                api.Wait();
            }
        }
    }

    public int GetCommandSize()
    {
        return 2;
    }

    public string GetState()
    {
        return GetStateJSON();
    }

    string GetStateJSON()
    {
        StringBuilder result = new StringBuilder();
        int width = game.GetWidth();
        int height = game.GetHeight();
        (int, int) playerPosition = game.GetPlayerPosition();

        result.Append("{ ");
        result.Append("\"board\": [");

        GameSquare[,] squares = game.GetStateMatrix();
        for (int row = 0; row < height; ++row)
        {
            result.Append('[');
            for (int col = 0; col < width; ++col)
            {
                result.Append('\"');
                if (squares[row, col] == null)
                {
                    result.Append('E');
                }
                else
                {
                    switch (squares[row, col].color)
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
                    switch (squares[row, col].type)
                    {
                        case Type.PILL:
                            result.Append('P');
                            switch (squares[row, col].joinedAt)
                            {
                                case JoinDirection.NONE:
                                    result.Append('N');
                                    break;
                                case JoinDirection.LEFT:
                                    result.Append('L');
                                    break;
                                case JoinDirection.RIGHT:
                                    result.Append('R');
                                    break;
                                case JoinDirection.UP:
                                    result.Append('U');
                                    break;
                                case JoinDirection.DOWN:
                                    result.Append('D');
                                    break;
                            }
                            break;
                        case Type.VIRUS:
                            result.Append('V');
                            break;
                    }

                }
                result.Append('\"');
                if (col < width - 1)
                {
                    result.Append(", ");
                }
            }
            result.Append(']');
            if (row < height - 1)
            {
                result.Append(", ");
            }
        }
        result.Append("], ");

        result.Append(string.Format("\"position\": {{ \"row\": {0}, \"col\": {1} }}, ", playerPosition.Item1, playerPosition.Item2));
        result.Append("\"condition\": \"");
        if(!game.GameHasEnded())
        {
            result.Append('O');
        } else if (game.PlayerHasWon())
        {
            result.Append('W');
        } else
        {
            result.Append('L');
        }
        result.Append("\" }");

        return result.ToString();
    }

    void Start()
    {
        foreach (GameAPI api in gameManager.GetComponents<GameAPI>())
        {
            if (api.enabled)
            {
                this.api = api;
                break;
            }
        }
        game = gameManager.GetComponent<Game>();
    }
}

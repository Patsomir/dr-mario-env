using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    [SerializeField]
    private Game game;

    [SerializeField]
    private TextMeshProUGUI message;

    [SerializeField]
    private Image panel;

    [SerializeField]
    private string winMessage = "You Win!";

    [SerializeField]
    private string lossMessage = "You Lose!";

    private void OnEnable()
    {
        game.OnGameEnd += EndGame;
        game.OnGameReset += ResetGame;
    }

    private void OnDisable()
    {
        game.OnGameEnd -= EndGame;
        game.OnGameReset -= ResetGame;
    }

    void EndGame()
    {
        panel.enabled = true;
        if (game.PlayerHasWon())
        {
            message.text = winMessage;
        }
        if (game.PlayerHasLost())
        {
            message.text = lossMessage;
        }
    }

    void ResetGame()
    {
        panel.enabled = false;
        message.text = "";
    }
}

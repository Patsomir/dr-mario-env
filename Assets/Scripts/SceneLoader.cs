using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField]
    private Game game = null;

    [SerializeField]
    private NormalGame normalGame = null;

    [SerializeField]
    private StepGame stepGame = null;

    [SerializeField]
    private PlayerInput manualInput = null;

    [SerializeField]
    private RemoteInput remoteInput = null;

    [SerializeField]
    private TextMeshProUGUI configParamsMessage = null;

    void OnEnable()
    {
        SceneManager.sceneLoaded += LoadScene;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= LoadScene;
    }

    void LoadScene(Scene scene, LoadSceneMode mode)
    {
        game.singleColorPillsOnly = Config.onlySingleColorPills;
        if (Config.stepMode)
        {
            stepGame.enabled = true;
            normalGame.enabled = false;
            stepGame.ResetGame(Config.width, Config.height, Config.virusHeight, Config.virusCount);
        } else
        {
            stepGame.enabled = false;
            normalGame.enabled = true;
            normalGame.cycleDurationSlow = Config.cycleDuration;
            normalGame.cycleDurationFast = Config.fastCycleDuration;
            normalGame.ResetGame(Config.width, Config.height, Config.virusHeight, Config.virusCount);
        }
        manualInput.enabled = Config.enableManualPlay;
        remoteInput.ip = Config.ip;
        remoteInput.port = Config.port;
        SetConfigParamsMessage();
    }

    void SetConfigParamsMessage()
    {
        configParamsMessage.text = string.Format(
            "{0}x{1} board with {2} virus{3} on {4} row{5}\nIP: {6} Port: {7}\nMode: {8}\n{9}\n{10}",
            Config.width,
            Config.height,
            Config.virusCount,
            Config.virusCount == 1 ? "" : "es",
            Config.virusHeight,
            Config.virusHeight == 1 ? "" : "s",
            Config.ip,
            Config.port,
            Config.stepMode ? "Steps" : string.Format("Real Time ({0}, {1})", Config.cycleDuration, Config.fastCycleDuration),
            Config.enableManualPlay ? "Manual play enabled" : "Manual play disabled",
            Config.onlySingleColorPills ? "With only single color pills\n" : ""
        );
    }
}

using UnityEngine;

public class ApplicationControls : MonoBehaviour
{
    [SerializeField]
    private RemoteInput remote;

    [SerializeField]
    private string disconnect = "q";

    [SerializeField]
    private string connect = "l";

    [SerializeField]
    private string quit = "escape";

    void Update()
    {
        if (Input.GetKey(quit))
        {
            Application.Quit();
        }

        if (Input.GetKey(disconnect))
        {
            remote.Disconnect();
        }

        if (Input.GetKeyDown(connect))
        {
            remote.Listen();
        }
    }
}

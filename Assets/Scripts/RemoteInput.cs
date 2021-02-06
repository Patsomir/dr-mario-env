using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class RemoteInput : MonoBehaviour
{
    public Int32 port = 42001;
    public string ip = "0.0.0.0";


    [SerializeField]
    private GameObject gameManager = null;
    private Game monitor = null;
    private GameAPI controller = null;

    private TcpListener server = null;
    private TcpClient client = null;
    NetworkStream stream = null;

    Queue<string> actionQueue = null;

    void AcceptClientCallback(IAsyncResult ar)
    {
        TcpListener listener = (TcpListener)ar.AsyncState;
        client = listener.EndAcceptTcpClient(ar);
        stream = client.GetStream();
        print("Client connected");
    }

    public void Listen()
    {
        if(server != null)
        {
            server.Stop();
        }
        try
        {
            server = new TcpListener(IPAddress.Parse(ip), port);
            server.Start();
            server.BeginAcceptTcpClient(new AsyncCallback(AcceptClientCallback), server);
            print(string.Format("Started listening on port {0}", port));
        }
        catch (SocketException e)
        {
            print(string.Format("SocketException: {0}", e));
        }
    }

    private Byte[] readBuffer = new Byte[16];
    private string data = "";
    void Read()
    {
        Task<Int32> res = stream.ReadAsync(readBuffer, 0, readBuffer.Length);
        
        data += Encoding.ASCII.GetString(readBuffer, 0, res.Result);
        for(int i = 0; i < data.Length - 1; i+=2)
        {
            actionQueue.Enqueue(data.Substring(i, 2).ToUpper());
        }

        if(data.Length % 2 == 1)
        {
            data = data.Substring(data.Length - 1, 1);
        } else
        {
            data = "";
        }
    }

    void ExecuteNextAction()
    {
        if(actionQueue.Count == 0)
        {
            return;
        }

        string action = actionQueue.Dequeue();
        if(action.Equals("QT"))
        {
            Disconnect();
        } else if(action.Equals("RS"))
        {
            controller.ResetGame(Config.width, Config.height, Config.virusHeight, Config.virusCount);
            SendCurrentState();
        } else if(action[0] == 'M')
        {
            switch(action[1])
            {
                case 'L':
                    controller.MoveLeft();
                    break;
                case 'R':
                    controller.MoveRight();
                    break;
                case 'D':
                    controller.MoveDown();
                    break;
            }
            SendCurrentState();
        } else if(action[0] == 'R')
        {
            switch (action[1])
            {
                case 'L':
                    controller.RotateLeft();
                    break;
                case 'R':
                    controller.RotateRight();
                    break;
            }
            SendCurrentState();
        } else if(action.Equals("WT"))
        {
            do
            {
                controller.Wait();
            } while (!monitor.PlayerCanPlay() && !monitor.GameHasEnded());
            
            SendCurrentState();
        }

        print(string.Format("Executed action \"{0}\"", action));
    }

    void SendCurrentState()
    {
        byte[] state = Encoding.ASCII.GetBytes(monitor.GetStateJSON());
        stream.Write(state, 0, state.Length);
    }

    public void Disconnect()
    {
        if(client != null)
        {
            stream.Close();
            client.Close();
            actionQueue.Clear();
        }
    }

    void Start()
    {
        foreach (GameAPI api in gameManager.GetComponents<GameAPI>())
        {
            if (api.enabled)
            {
                controller = api;
                break;
            }
        }
        monitor = gameManager.GetComponent<Game>();
        actionQueue = new Queue<string>();
    }

    private void Update()
    {
        if(client != null && client.Connected)
        {
            try
            {
                Read();
                ExecuteNextAction();
            } catch(Exception e)
            {
                print(string.Format("Exception thrown: {0}", e));
                Disconnect();
            }
        }
    }
}
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
    private int readBufferSize = 16;

    [SerializeField]
    private Component monitorComponent = null;

    [SerializeField]
    private Component controllerComponent = null;

    private Monitor monitor = null;
    private Controller controller = null;

    private TcpListener server = null;
    private TcpClient client = null;
    NetworkStream stream = null;

    Queue<string> actionQueue = null;
    private int commandSize;

    private void Start()
    {
        monitor = (Monitor)monitorComponent;
        controller = (Controller)controllerComponent;
        actionQueue = new Queue<string>();
        readBuffer = new Byte[readBufferSize];
        commandSize = controller.GetCommandSize();
    }

    void AcceptClientCallback(IAsyncResult ar)
    {
        TcpListener listener = (TcpListener)ar.AsyncState;
        client = listener.EndAcceptTcpClient(ar);
        stream = client.GetStream();
        print("Client connected");
    }

    public void Listen()
    {
        if (server != null)
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

    private Byte[] readBuffer;
    private string data = "";
    void Read()
    {
        Task<Int32> res = stream.ReadAsync(readBuffer, 0, readBuffer.Length);

        data += Encoding.ASCII.GetString(readBuffer, 0, res.Result);
        for (int i = 0; i <= data.Length - commandSize; i += commandSize)
        {
            actionQueue.Enqueue(data.Substring(i, commandSize).ToUpper());
        }

        int left = data.Length % commandSize;
        if (left == 0)
        {
            data = "";
        }
        else
        {
            data = data.Substring(data.Length - left, left);
        }
    }

    void ExecuteNextAction()
    {
        if (actionQueue.Count == 0)
        {
            return;
        }

        string action = actionQueue.Dequeue();
        controller.Execute(action);
        SendCurrentState();
        print(string.Format("Executed action \"{0}\"", action));
    }

    void SendCurrentState()
    {
        if (client.Connected)
        {
            byte[] state = Encoding.ASCII.GetBytes(monitor.GetState());
            stream.Write(state, 0, state.Length);
        }
    }

    public void Disconnect()
    {
        if (client != null)
        {
            stream.Close();
            client.Close();
            actionQueue.Clear();
        }
    }

    private void Update()
    {
        if (client != null && client.Connected)
        {
            try
            {
                Read();
                ExecuteNextAction();
            }
            catch (Exception e)
            {
                print(string.Format("Exception thrown: {0}", e));
                Disconnect();
            }
        }
    }
}

public interface Monitor
{
    string GetState();
}

public interface Controller
{
    int GetCommandSize();
    void Execute(string command);
}
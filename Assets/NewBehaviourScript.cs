using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

public class NewBehaviourScript : MonoBehaviour
{

    private int frameCounter = 0;
    private const int frameInterval = 300;
    private WebCamTexture webCamTexture;

    private Socket serverSocket;
    private Socket clientSocket;
    private int serverPort = 8000;
    // private int clientMax = 1;
    // private bool useNAT = false;
    private bool clientConnected = false;
    private Thread acceptClientThread;
    private Thread communicateThread;

    private byte[] snapshotBytes;
    private string friendName = "";

    // Use this for initialization
    void Start()
    {
        // start camera streaming
        webCamTexture = new WebCamTexture();
        webCamTexture.Play();

        // instantiate network server to communicate with client phone
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress serverIP = Dns.GetHostAddresses(Dns.GetHostName())[0];
        IPEndPoint serverIPEndPoint = new IPEndPoint(serverIP, serverPort);
        serverSocket.Bind(serverIPEndPoint);
        serverSocket.Listen(1);
        // Network.InitializeServer(clientMax, serverPort, useNAT);

        acceptClientThread = new Thread(t =>
        {
            clientSocket = serverSocket.Accept();
            clientConnected = true;
        });
        communicateThread = new Thread(t =>
        {
            clientSocket.Send(snapshotBytes, snapshotBytes.Length, SocketFlags.None);
            byte[] nameBytes = new byte[100];
            int nameBytesLen = clientSocket.Receive(nameBytes);
            name = Encoding.ASCII.GetString(nameBytes, 0, nameBytesLen);
        });
        acceptClientThread.Start();
    }
    /*
    private void OnPlayerDisconnected(NetworkPlayer player)
    {
        clientConnected = false;
        acceptClientThread.Start();
    }
    */
    // Update is called once per frame
    void Update()
    {
        if (frameCounter == 0)
        {
            Texture2D snapshot = new Texture2D(webCamTexture.width, webCamTexture.height);
            snapshot.SetPixels(webCamTexture.GetPixels());
            snapshot.Apply();
            snapshotBytes = snapshot.EncodeToJPG();
            if (clientConnected)
            {
                communicateThread.Start();
            }
        }
        frameCounter = (frameCounter + 1) % frameInterval;
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 1000, 400), friendName);
    }
}

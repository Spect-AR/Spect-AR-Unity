using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
#if !UNITY_EDITOR
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#endif

public class BluetoothCommunication : MonoBehaviour
{
    private int frameCounter = 0;
    private const int frameInterval = 300;
    private WebCamTexture webCamTexture;

    private Socket clientSocket;
    private StreamSocket clientStreamSocket;
    private IPAddress serverIP;
    private int serverPort = 1802;
    private Thread communicateThread;

    private byte[] snapshotBytes;
    private buffer snapshotBuffer;
    private string friendName = "";

    // Use this for initialization
    void Start()
    {
        // start camera streaming
        webCamTexture = new WebCamTexture();
        webCamTexture.Play();

        // instantiate network server to communicate with client phone
#if UNITY_EDITOR
        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        serverIP = IPAddress.Parse("104.154.205.34");
        IPEndPoint serverIPEndPoint = new IPEndPoint(serverIP, serverPort);
        clientSocket.Connect(serverIPEndPoint);
#else
        clientStreamSocket = new StreamSocket();
        HostName wserverIP = new HostName("104.154.205.34");
        string wserverPort = "1802";
        // HostName wclientIP = new HostName("127.0.0.1");
        // string wclientPort = "1802";
        // EndPointPair endPointPair = new EndPoingPair(wclientIP, wclientPort, wserverIP, wserverPort);
        clientStreamSocket.ConnectAsync(wserverIP, wserverPort);
#endif

        communicateThread = new Thread(t =>
        {
            clientSocket.Send(snapshotBytes, snapshotBytes.Length, SocketFlags.None);
            byte[] nameBytes = new byte[64];
            int nameBytesLen = clientSocket.Receive(nameBytes);
            string name = Encoding.ASCII.GetString(nameBytes, 0, nameBytesLen);
            Debug.Log(name);
            friendName = name;
        });
    }
    
    // Update is called once per frame
    void Update()
    {
        if (frameCounter == 0)
        {
            Texture2D snapshot = new Texture2D(webCamTexture.width, webCamTexture.height);
            snapshot.SetPixels(webCamTexture.GetPixels());
            snapshot.Apply();
            snapshotBytes = snapshot.EncodeToJPG();
            communicateThread.Start();
        }
        frameCounter = (frameCounter + 1) % frameInterval;
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 1000, 400), friendName);
        Debug.Log(friendName);
    }
}

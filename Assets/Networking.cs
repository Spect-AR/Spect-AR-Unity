using UnityEngine;
using System.Text;
using Windows.Networking.Sockets;
using Windows.Networking.HostName;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using Assets;

public class Networking : MonoBehaviour
{
    private int frameCounter = 0;
    private const int frameInterval = 300;
    private WebCamTexture webCamTexture;

    private StreamSocket clientSocket;
    private HostName serverIP;
    private string serverPort = "1802";
    // private Thread communicateThread;

    private Buffer snapshotBytes;
    private Buffer friendNameBytes;

    // Use this for initialization
    void Start()
    {
        IFlyer x = new Superman();
        x.doWork();

        // start camera streaming
        webCamTexture = new WebCamTexture();
        webCamTexture.Play();

        // instantiate network server to communicate with client phone
        clientSocket = new StreamSocket();
        serverIP = new HostName("104.154.205.34");
        clientSocket.ConnectAsync(serverIP, serverPort);

        snapshotBytes = new Buffer(4 * 1024 * 1024);
        friendNameBytes = new Buffer(64);

        /*
        communicateThread = new Thread(t =>
        {
            clientSocket.OutputStream.WriteAsync(snapshotBytes);
            // clientSocket.Send(snapshotBytes, snapshotBytes.Length, SocketFlags.None);
            // byte[] nameBytes = new byte[64];
            // int nameBytesLen = clientSocket.Receive(nameBytes);
            // string name = Encoding.ASCII.GetString(nameBytes, 0, nameBytesLen);
            // Debug.Log(name);
            // friendName = name;
            clientSocket.InputStream.ReadAsync(friendNameBytes, 64, InputStreamOptions.None);
        });
        */
    }

    // Update is called once per frame
    void Update()
    {
        if (frameCounter == 0)
        {
            Texture2D snapshot = new Texture2D(webCamTexture.width, webCamTexture.height);
            snapshot.SetPixels(webCamTexture.GetPixels());
            snapshot.Apply();
            snapshotBytes = AsBuffer(snapshot.EncodeToJPG());
            // communicateThread.Start();
            clientSocket.OutputStream.WriteAsync(snapshotBytes);
            clientSocket.InputStream.ReadAsync(friendNameBytes, 64, InputStreamOptions.None);
        }
        frameCounter = (frameCounter + 1) % frameInterval;
    }

    private void OnGUI()
    {
        byte[] bytes = ToArray(friendNameBytes);
        string friendName = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
        GUI.Label(new Rect(10, 10, 1000, 400), friendName);
        Debug.Log(friendName);
    }
}

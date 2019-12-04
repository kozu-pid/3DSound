using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;

public class G711Receiver : MonoBehaviour
{
    private int recvPort;
    public int RecvPort
    {
        get { return recvPort; }
        set { recvPort = value; }
    }

    private string ipAddress;
    public string IPAddress
    {
        get { return ipAddress; }
        set { ipAddress = value; }
    }

    private G711RX udpClient;
    // Start is called before the first frame update
    void Start()
    {

        udpClient = new G711RX();
        /*
        if (!udpClient.StartServer(ipAddress, RecvPort))
        {
            Debug.LogError("Socket作成ができませんでした");
        }
        */

    }
}

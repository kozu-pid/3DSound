using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using System.Net.Sockets;
using System.Threading;

/// <summary>
/// サブスレッドにてソケット通信を行うクラス
/// サブスレッドとメインスレッドでの情報の受け渡しにQueueを使う
/// </summary>
public class G711RX
{
    /*
    private Socket socket = null;
    private PacketQueue sendQueue;
    private PacketQueue recvQueue;

    // Used by Communication Thread
    protected Thread communicationThread;
    private bool isThreadLoop;

    private static int mtu = 1400;

    #region Audio config

    private int hdlen = 12;
    private int pllen = 160;

    private int lfreq = 44100;
    private int lqbit = 16;
    private int lchnl = 2;

    private int ufreq = 44100;
    private int uchnl = 2;
    private int ubpf = 2;
    private int uframe = 44100;

    public void SetFrequency(int freq)
    {
        this.lfreq = freq;
        this.ufreq = freq;
        this.uframe = freq;
    }

    private long receivedpackets = 0;
    private long receivedsize = 0;
    private long packetloss = 0;

    private int i = 0, k;
    private short num, temp, seq = 0;

    private AudioFileWriter afw;

    private WaveFormat linerPCMFormat;
    private WaveFormat muLawFormat;

    #endregion Audio config

    // constructor
    public G711RX()
    {
        sendQueue = new PacketQueue();
        recvQueue = new PacketQueue();
    }

    // constructor
    public G711RX(Socket socket)
    {
        this.socket = socket;
        sendQueue = new PacketQueue();
        recvQueue = new PacketQueue();
    }

    #region startserver method
    /// <summary>
    /// ポート番号を指定する待ち受け処理
    /// </summary>
    /// <param name="port">ポート番号</param>
    public void StartServer(int port)
    {
        try
        {
            // Initialize socket
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            this.socket.Bind(new IPEndPoint(IPAddress.Any, port));
        }
        catch
        {
            this.socket = null;
        }
    }

    /// <summary>
    /// ポート番号とIPアドレスを指定する待ち受け処理
    /// </summary>
    /// <param name="iPAddress">元のIPAddress</param>
    /// <param name="port">ポート番号</param>
    public bool StartServer(string iPAddress, int port)
    {
        try
        {
            // Initialize socket
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            this.socket.Bind(new IPEndPoint(IPAddress.Parse(iPAddress), port));
        }
        catch
        {
            this.socket = null;
        }

        return launchThread();
    }
    #endregion startserver method

    #region Thread
    /// <summary>
    /// Start Thread Method
    /// </summary>
    /// <returns></returns>
    private bool launchThread()
    {
        try
        {
            isThreadLoop = true;
            communicationThread = new Thread(new ThreadStart(Dispatch));
            communicationThread.Start();
        }
        catch
        {
            return false;
        }

        return true;
    }
    /// <summary>
    /// use for sub thread
    /// </summary>
    public void Dispatch()
    {

        byte[] buffer = new byte[hdlen + pllen];
        // buffer length must be alloced twice as pllen.
        byte[] linearBuffer = new byte[2 * pllen];
        byte[] tempBuffer = new byte[2 * pllen];
        byte[] seqNum = new byte[2];

        for (k = 0; k < tempBuffer.Length; k++)
        {
            tempBuffer[k] = 0;
        }

        muLawFormat = WaveFormat.CreateMuLawFormat(ufreq, uchnl);
        linerPCMFormat = new WaveFormat(lfreq, lqbit, lchnl);
    }

    // スレッド側の受信処理.
    void DispatchReceive()
    {
        // 受信処理.
        try
        {
            while (socket.Poll(0, SelectMode.SelectRead))
            {
                byte[] buffer = new byte[mtu];

                int recvSize = socket.Receive(buffer, buffer.Length, SocketFlags.None);
                // 通信相手と切断したことにReceive関数の関数値は0が返されます.
                if (recvSize == 0)
                {
                    // 切断.
                    // Disconnect();
                }
                else if (recvSize > 0)
                {
                    Debug.Log(buffer.ToString());
                    // ゲームスレッド側に受信したデータを渡すために受信データをキューに追加します.
                    recvQueue.Enqueue(buffer, recvSize);
                }
            }
        }
        catch
        {
            return;
        }
    }
    #endregion Thread
    */
}
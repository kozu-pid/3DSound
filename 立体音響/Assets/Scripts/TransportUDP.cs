// UDP通信を行う通信モジュール

using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;


public class TransportUDP : MonoBehaviour {

	//
	// ソケットによる送受信関連変数.
	//

	// クライアントとの送受信用ソケット.
	private Socket			m_socket = null;

	// 送信バッファ.
	// private PacketQueue		m_sendQueue;
	
	// 受信バッファ.
	private PacketQueue		m_recvQueue;

	//
	// スレッド関連のメンバ変数.
	//

	// スレッド実行フラグ.
	protected bool			m_threadLoop = false;
    public bool ThreadLoop
    {
        get { return m_threadLoop; }
    }
	
	protected Thread		m_thread = null;

	// バッファのサイズはMTUの設定によって決まります.(MTU:1回に送信できる最大のデータサイズ)
	// イーサネットの最大MTUは1500bytesです.
	// この値はOSや端末などで異なるものですのでバッファのサイズは
	// 動作させる環境のMTUを調べて設定しましょう.
	private static int 		s_mtu = 1400;

    private int dataIndex;
    private int recvIndex;
    private int dataPacketsParFile;
    public int DataPacketsParFile
    {
        set { dataPacketsParFile = value; }
        get { return dataPacketsParFile; }
    }
    private int bytesParPacket;
    public int BytesParPacket
    {
        set { bytesParPacket = value; }
        get { return bytesParPacket; }
    }

    public TransportUDP()
    {
        m_recvQueue = new PacketQueue();
        m_recvQueue.Clear();
        dataIndex = 0;
        recvIndex = 0;
    }

    public TransportUDP(int dataPacketsParFile, int bytesParPacket)
    {
        m_recvQueue = new PacketQueue();
        dataIndex = 0;
        recvIndex = 0;
        this.dataPacketsParFile = dataPacketsParFile;
        this.bytesParPacket = bytesParPacket;
    }


    // 待ち受け開始.
    public bool StartListening(int port)
	{
        Debug.Log("StartListening called.!");

        // 送受信用ソケットを生成します.
        try {
			// ソケットを生成します.
			m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			// 使用するポート番号を割り当てます.
			m_socket.Bind(new IPEndPoint(IPAddress.Any, port));
        }
        catch {
			Debug.Log("StartListening fail");
            return false;
        }
        return LaunchThread();
    }

    public bool StartListening(string ipAddress, int port)
    {
        Debug.Log("StartListening called.!");

        // 送受信用ソケットを生成します.
        try
        {
            // ソケットを生成します.
            m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPAddress srcIpAddress = IPAddress.Parse(ipAddress);
            // 使用するポート番号を割り当てます.
            m_socket.Bind(new IPEndPoint(srcIpAddress, port));
        }
        catch
        {
            Debug.Log("StartListening fail");
            return false;
        }
        return LaunchThread();
    }

    // 待ち受け終了.
    public void StopListening()
    {
		m_threadLoop = false;
        if (m_thread != null) {
            m_thread.Join();
            m_thread = null;
        }

		if (m_socket != null) {
			m_socket.Close();
			m_socket = null;
        }

        Debug.Log("Listening stopped.");
    }

    // 受信処理.
    public int Receive(ref byte[] buffer, int size)
	{
		if (m_recvQueue.Length <= dataPacketsParFile) {
            Debug.Log("Receive is not yet");
			return 0;
		}
        Debug.Log("Receive is ready");
        // Dequeue
		return m_recvQueue.Dequeue(ref buffer, size, dataPacketsParFile);
    }

	// スレッド起動関数.
	bool LaunchThread()
	{
		try {
			// Dispatch用のスレッド起動.
			m_threadLoop = true;
			m_thread = new Thread(new ThreadStart(Dispatch));
			m_thread.Start();
		}
		catch {
			Debug.Log("Cannot launch thread.");
			return false;
		}
		
		return true;
	}

	// 通信スレッド側の送受信処理.
    public void Dispatch()
	{
		Debug.Log("Dispatch thread started.");
        while (m_threadLoop) {
            // クライアントからの接続を待ちます.
            // AcceptClient();
			// クライアントとの送受信を処理します.
			if (m_socket != null) {
	            // 受信処理.
	            DispatchReceive();
	        }

			Thread.Sleep(5);
		}

		Debug.Log("Dispatch thread ended.");
    }

	// スレッド側の受信処理.
    void DispatchReceive()
	{
        // 受信処理.
        try {
            while (m_socket.Poll(0, SelectMode.SelectRead)) {
				byte[] buffer = new byte[s_mtu];
                int recvSize = m_socket.Receive(buffer, buffer.Length, SocketFlags.None);
                // 通信相手と切断したことにReceive関数の関数値は0が返されます.
                if (recvSize == 0) {
                    // 切断.
                    Debug.Log("Disconnect recv from client.");
                }
                else if (recvSize > 0) {
                    // ゲームスレッド側に受信したデータを渡すために受信データをキューに追加します.
                    // インデックスの切り離し
                    recvIndex = getRecvIndex(buffer);
                    // waveData is OK
                    // Debug.Log("dataIndex : " + dataIndex + ", recvIndex : " + recvIndex);
                    if (dataIndex <= recvIndex)
                    {
                        // 0埋め行う
                        setZero(dataIndex, recvSize, bytesParPacket);
                        m_recvQueue.Enqueue(buffer, 4, buffer.Length - 4, recvIndex);
                        Debug.Log("Enqueue is called");
                        dataIndex = recvIndex + 1;
                    }
                    // Dequeueされたあとに格納したくない
                    else if (m_recvQueue.LeadIndex > recvIndex)
                    {
                        m_recvQueue.Insertqueue(buffer, 4, buffer.Length - 4, recvIndex);
                    }
                }
            }
        }
        catch {
            return;
        }
    }

    private int getRecvIndex(byte[] buffer)
    {
        byte[] header = new byte[4];
        // ヘッダの取得
        for (int i = 0; i < header.Length; i++)
        {
            header[i] = buffer[i];
        }
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(header);
        }
        return BitConverter.ToInt32(header, 0);
    }

    private void setZero(int srcIndex, int dstIndex, int unitLength)
    {
        if (srcIndex >= dstIndex)
        {
            return;
        }
        byte[] zeros = new byte[unitLength];
        for (int i = 0; i < zeros.Length; i++)
        {
            zeros[i] = (byte)0;
        }
        for (int i = srcIndex; i < dstIndex; i++)
        {
            m_recvQueue.Enqueue(zeros, 0, zeros.Length, i);
        }
    }
}
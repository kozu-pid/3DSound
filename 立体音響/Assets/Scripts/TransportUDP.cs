// UDP通信を行う通信モジュール

using UnityEngine;
using System.Collections;
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

    public TransportUDP()
    {
        m_recvQueue = new PacketQueue();
        m_recvQueue.Clear();
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

    /*
    // 送信処理.
    public int Send(byte[] data, int size)
	{
		if (m_sendQueue == null) {
			return 0;
		}

		// 送信データは一旦キューにバッファリングするだけで送信はしていません.
		// 実際の送信は通信スレッド側(DispatchSend() 関数)で行います.
		// ゲームスレッド側の処理をできるだけ軽くするために直接 Send() 関数で送信していません.
		return m_sendQueue.Enqueue(data, size);
    }
    */

    // 受信処理.
    public int Receive(ref byte[] buffer, int size)
	{
        Debug.Log("Receive is called");
		if (m_recvQueue.Length <= 0) {
            Debug.Log("Receice queue is null");
			return 0;
		}
        Debug.Log("Recive queue is not null");
        // 実際の受信は通信スレッド側(DispatchReceive() 関数)で行います.
        // ゲームスレッド側の処理をできるだけ軽くするために直接　Receive() 関数で受信していません.
        
		return m_recvQueue.Dequeue(ref buffer, size);
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

	            // 送信処理.
	            // DispatchSend();

	            // 受信処理.
	            DispatchReceive();
	        }

			Thread.Sleep(5);
		}

		Debug.Log("Dispatch thread ended.");
    }

    /*
	// スレッド側の送信処理.
    void DispatchSend()
	{
        try {
            // 送信処理.
            if (m_socket.Poll(0, SelectMode.SelectWrite)) {
				byte[] buffer = new byte[s_mtu];

				// Send関数でバッファリングされたデータを取り出して送信を行います.
                int sendSize = m_sendQueue.Dequeue(ref buffer, buffer.Length);
                // 送信データがなくなるまで送信を続けます.
                while (sendSize > 0) {
                    m_socket.Send(buffer, sendSize, SocketFlags.None);
                    sendSize = m_sendQueue.Dequeue(ref buffer, buffer.Length);
                }
            }
        }
        catch {
            return;
        }
    }
    */

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
                    m_recvQueue.Enqueue(buffer, recvSize);
                }
            }
        }
        catch {
            return;
        }
    }
}
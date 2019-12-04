using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ClientTestManager : MonoBehaviour
{
    #region SerializeField Define

    // TODO : Array
    [SerializeField] private AudioSource speakerAudio;
    // mtuの仮置き，10ms分のデータ（441byte）とヘッダ（4byte）
    [SerializeField] private int mtu = 445;
    [SerializeField] private Text debugText;

    #endregion SerializeField Define

    #region private Define

    private TransportUDP clientSocket;
    private string ipAddressText;
    private int port;
    private int dataIndex;
    private bool isConnected = false;

    private byte[] waveData;

    #endregion

    // イベントハンドラー.
    public void OnEventHandling(NetEventState state)
    {
        switch (state.type)
        {
            case NetEventType.Connect:
                isConnected = true;
                Debug.Log("[NetworkController] Connected.");
                break;

            case NetEventType.Disconnect:
                isConnected = false;
                Debug.Log("[NetworkController] Disconnected.");
                break;
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        // 通信関連
        clientSocket = new TransportUDP();
        ipAddressText = DataManager.Instance.IpAddressText;
        port = DataManager.Instance.PortNum;
        // connectionNumはUDPでは使用しない
        // 本番コード
        clientSocket.StartServer(ipAddressText, port, 1);
        // テストコード
        // clientSocket.StartServer(port, -1);
        clientSocket.RegisterEventHandler(OnEventHandling);
        dataIndex = 0;
        // waveのデータを0.1sで保存する配列
        waveData = new byte[4410];
    }

    private void Update()
    {
        if (Input.anyKeyDown)
        {
            clientSocket.Disconnect();
            clientSocket.StopServer();
        }
    }
    private void FixedUpdate()
    {
        recieveWaveBytes();
    }

    private void recieveWaveBytes()
    {
        byte[] buffer = new byte[mtu];

        int recvSize = clientSocket.Receive(ref buffer, buffer.Length);
        if (recvSize > 0)
        {
            /*
            for (int i = 0; i < recvSize; i++)
            {
                Debug.Log("受信したデータ：" + System.Text.Encoding.ASCII.GetString(buffer) + "\n");
            }
            */
            Debug.Log("受信したデータ：" + System.Text.Encoding.ASCII.GetString(buffer) + "\n");
            debugText.text = "受信したデータ：" + System.Text.Encoding.ASCII.GetString(buffer) + "\n";
        }
    }

    private float[] ConvertByteToFloat(byte[] array)
    {
        float[] floatArr = new float[array.Length / 4];
        for (int i = 0; i < floatArr.Length; i++)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(array, i * 4, 4);
            floatArr[i] = BitConverter.ToSingle(array, i * 4) / 0x1000;
        }
        return floatArr;
    }

    private AudioClip createAudioClip(byte[] array)
    {
        float[] waveFloat = ConvertByteToFloat(array);
        AudioClip audioClip = AudioClip.Create("testSound", waveFloat.Length, 2, 44100, false, false);
        audioClip.SetData(waveFloat, 0);
        return audioClip;
    }

}

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

    private byte[] waveData;

    #endregion

    // Start is called before the first frame update
    private void Start()
    {
        // 通信関連
        clientSocket = new TransportUDP();
        ipAddressText = DataManager.Instance.IpAddressText;
        port = DataManager.Instance.PortNum;
        // 本番コード
        clientSocket.StartListening(ipAddressText, port);
        dataIndex = 0;
        // waveのデータを0.1sで保存する配列
        waveData = new byte[4410];
    }

    private void Update()
    {
        if (Input.anyKey)
        {
            clientSocket.StopListening();
        }
    }

    private void FixedUpdate()
    {
        if (clientSocket.ThreadLoop)
        {
            recieveWaveBytes();
        }
    }

    private void recieveWaveBytes()
    {
        byte[] buffer = new byte[mtu];

        int recvSize = clientSocket.Receive(ref buffer, buffer.Length);
        if (recvSize > 0)
        {
            byte[] header = new byte[4];
            // ヘッダの取得
            for (int i = 0; i < header.Length; i++)
            {
                header[i] = buffer[i];
            }
            if (BitConverter.IsLittleEndian)
                Array.Reverse(header);
            Debug.Log("header index : " + BitConverter.ToInt32(header, 0));
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

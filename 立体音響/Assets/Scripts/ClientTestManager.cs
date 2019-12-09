using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;

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
    // 送られてきたデータのindexと比較するためのindex
    private int dataIndex;
    // 一つのパケットに含まれるwaveデータの長さ
    private int waveDataBytesParPacketLength;
    // 一つのwaveファイルに含まれるwaveデータのパケット数
    private int waveDataPacketParFile;

    private byte[] waveData;

    private Queue<AudioClip> audioClips;

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
        waveDataBytesParPacketLength = mtu - 4;
        waveDataPacketParFile = 100;
        // waveのデータを0.1sで保存する配列
        // waveファイルに書き出す配列
        waveData = new byte[waveDataBytesParPacketLength * waveDataPacketParFile];

        audioClips = new Queue<AudioClip>();

        // If clientSocket.ThreadLoop is true, recieveWaveBytes is called
        var recieveStream = this.UpdateAsObservable().Where(_ => clientSocket.ThreadLoop);
        recieveStream.Subscribe(_ => {
            recieveWaveBytes();
        });

        
        var stopWaveStream = this.UpdateAsObservable().Where(_ => !speakerAudio.isPlaying);
        stopWaveStream.Subscribe(_ =>
        {
            playWaveFile();
        });
    }

    private void Update()
    {
        if (Input.anyKey)
        {
            clientSocket.StopListening();
        }
    }

    private void recieveWaveBytes()
    {
        byte[] buffer = new byte[mtu];

        int recvSize = clientSocket.Receive(ref buffer, buffer.Length);
        if (recvSize <= 0)
        {
            return;
        }
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
        int nowIndex = BitConverter.ToInt32(header, 0);

        // 期待するindexが来たとき
        if (nowIndex == dataIndex)
        {
            setWaveData(buffer, nowIndex);
            dataIndex++;
            // 期待するindexが10の倍数であればwaveに格納する
            if ((dataIndex % waveDataPacketParFile) == 0)
            {
                setAudioClip(createAudioClip(waveData));
            }
        }
        #region 期待するindexより小さい値が来たとき

        // waveに書き出す前であれば格納する
        else if (nowIndex < dataIndex && judgeSameCircle(nowIndex))
        {
            setWaveData(buffer, nowIndex);
        }
        // 書き出したあとのため破棄
        else if (nowIndex < dataIndex)
        {
            // 破棄すべきデータ
            return;
        }

        #endregion

        #region 期待するindexより大きい値が来た時

        // waveに書き出す前であれば格納し，空き箇所に0を埋める
        else if (nowIndex > dataIndex && judgeSameCircle(nowIndex))
        {
            setWaveData(buffer, nowIndex);
            // 空き箇所に０詰める
            setZero(nowIndex);
            dataIndex = nowIndex + 1;
            // 期待するindexが10の倍数であればwaveに格納する
            if ((dataIndex % waveDataPacketParFile) == 0)
            {
                setAudioClip(createAudioClip(waveData));
            }
        }

        // waveに書き出してから格納したい
        else if (nowIndex > dataIndex)
        {
            // 損失箇所を0埋めする
            setZero(nowIndex);
            // waveに書き出す
            setAudioClip(createAudioClip(waveData));
            // 格納する
            setWaveData(buffer, nowIndex);

            dataIndex = nowIndex + 1;
        }

        #endregion

    }

    private float[] ConvertByteToFloat(byte[] array)
    {
        float[] floatArr = new float[array.Length / 4];
        for (int i = 0; i < floatArr.Length; i++)
        {
            /*
            if (BitConverter.IsLittleEndian)
                Array.Reverse(array, i * 4, 4);
            */
            floatArr[i] = BitConverter.ToSingle(array, i * 4) / 0x80000000;
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

    private void setWaveData(byte[] buffer, int nowIndex)
    {
        int CopyDestinationIndex = waveDataBytesParPacketLength * (nowIndex % waveDataPacketParFile);
        Array.Copy(buffer, 4, waveData, CopyDestinationIndex, waveDataBytesParPacketLength);
    }

    private void setZero(int nowIndex)
    {
        // nowIndexがdataIndex以下のとき処理を行わない
        if (nowIndex <= dataIndex)
        {
            return;
        }
        int dstIndex = nowIndex;
        if (judgeSameCircle(dstIndex))
        {
            dstIndex = 10;
        }

        for (int i = dataIndex * waveDataBytesParPacketLength; i < dstIndex * waveDataBytesParPacketLength; i++)
        {
            // TODO: IndexOutOfRangeException
            waveData[i] = 0;
        }
    }

    private bool judgeSameCircle(int nowIndex)
    {
        return (nowIndex / waveDataPacketParFile) == (dataIndex / waveDataPacketParFile);
    }

    private void setAudioClip(AudioClip audioClip)
    {
        audioClips.Enqueue(audioClip);
    }

    private void playWaveFile()
    {
        if (audioClips.Count <= 0)
        {
            return;
        }
        speakerAudio.clip = audioClips.Dequeue();
        Debug.Log("Dequeue is called");
        speakerAudio.Play();
    }
}

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using WWUtils.Audio;

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
        waveDataPacketParFile = 1000;
        // waveのデータを0.1sで保存する配列
        // waveファイルに書き出す配列
        waveData = new byte[waveDataBytesParPacketLength * waveDataPacketParFile + 46];
        setWaveHeader(waveData);

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
            if (BitConverter.IsLittleEndian)
                Array.Reverse(array, i * 4, 4);
            floatArr[i] = BitConverter.ToInt16(array, i * 4) / 0x8000;
        }
        return floatArr;
    }

    private AudioClip createAudioClip(byte[] array)
    {
        /*
        AudioClipMaker acm = new AudioClipMaker();
        return acm.Create("testSound", array, 44, 16, array.Length / 4, 2, 44100, false);
        */

        WAV wav = new WAV(array);
        Debug.Log(wav);
        AudioClip audioClip = AudioClip.Create("testSound", wav.SampleCount, 1, wav.Frequency, false);
        audioClip.SetData(wav.LeftChannel, 0);
        return audioClip;
    }

    private void setWaveData(byte[] buffer, int nowIndex)
    {
        int CopyDestinationIndex = waveDataBytesParPacketLength * (nowIndex % waveDataPacketParFile) + 46;
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

    #region About header

    private int setWaveHeader(byte[] waveBytes)
    {
        if (waveBytes.Length < 44)
        {
            return -1;
        }
        // "RIFF" : 4bytes
        byte[] RIFF_ASCII = System.Text.Encoding.ASCII.GetBytes("RIFF");
        Debug.Log("RIFF bytes : " + BitConverter.ToString(RIFF_ASCII));
        Array.Copy(RIFF_ASCII, 0, waveBytes, 0, RIFF_ASCII.Length);

        // チャンクサイズ : 4bytes
        // ファイル全体のサイズ - 8　（"RIFF"と"WAVE"の分）
        // 可変であるべき
        int size = waveBytes.Length;
        // int -> byte[4]
        byte[] chunk_size = BitConverter.GetBytes(size - 8);
        Debug.Log("chunk_size : " + BitConverter.ToString(chunk_size));
        Array.Copy(chunk_size, 0, waveBytes, 4, chunk_size.Length);
        chunk_size = null;

        // "WAVE" : 4bytes
        byte[] WAVE_ASCII = System.Text.Encoding.ASCII.GetBytes("WAVE");
        Debug.Log("WAVE bytes : " + BitConverter.ToString(WAVE_ASCII));
        Array.Copy(WAVE_ASCII, 0, waveBytes, 8, WAVE_ASCII.Length);

        // "fmt " : 4bytes
        byte[] fmt_ASCII = System.Text.Encoding.ASCII.GetBytes("fmt ");
        Debug.Log("fmt bytes : " + BitConverter.ToString(fmt_ASCII));
        Array.Copy(fmt_ASCII, 0, waveBytes, 12, fmt_ASCII.Length);

        // fmtチャンクのバイト数　リニアPCMなら16 : 4bytes
        // 今回のWAVファイルは18であったが，16にしてみる
        byte[] fmt_size = BitConverter.GetBytes(18);
        Debug.Log("fmt_size : " + BitConverter.ToString(fmt_size));
        Array.Copy(fmt_size, 0, waveBytes, 16, fmt_size.Length);
        fmt_size = null;

        // 音声フォーマット : 2bytes
        // リニアPCM = 1
        waveBytes[20] = (byte)1;
        waveBytes[21] = (byte)0;

        // チャンネル数 : 2bytes
        // ステレオ = 2
        waveBytes[22] = (byte)2;
        waveBytes[23] = (byte)0;

        // サンプリング周波数 : 4bytes
        // 44100Hz
        byte[] sampleFreq = BitConverter.GetBytes(44100);
        Debug.Log("sampleFreq : " + BitConverter.ToString(sampleFreq));
        Array.Copy(sampleFreq, 0, waveBytes, 24, sampleFreq.Length);
        sampleFreq = null;

        // 1秒あたりバイト数の平均 : 4bytes
        byte[] aveParSec = BitConverter.GetBytes(44100 * 4);
        Debug.Log("aveParSec : " + BitConverter.ToString(aveParSec));
        Array.Copy(aveParSec, 0, waveBytes, 28, aveParSec.Length);
        aveParSec = null;

        // ブロックサイズ : 2bytes
        waveBytes[32] = (byte)4;
        waveBytes[33] = (byte)0;

        // ビット／サンプル : 2bytes
        waveBytes[34] = (byte)10;
        waveBytes[35] = (byte)0;

        // 拡張パラメータののサイズ
        waveBytes[36] = (byte)0;
        waveBytes[37] = (byte)0;

        // "data" : 4bytes
        byte[] data_ASCII = System.Text.Encoding.ASCII.GetBytes("data");
        Debug.Log("data bytes : " + BitConverter.ToString(data_ASCII));
        Array.Copy(data_ASCII, 0, waveBytes, 38, data_ASCII.Length);

        // 波形データのバイト数 : 4bytes
        // ここが分からないため先生の助言どうり-46で試してみる
        byte[] waveDataBytes = BitConverter.GetBytes(size - 46);
        Debug.Log("waveDataBytes : " + BitConverter.ToString(waveDataBytes));
        Array.Copy(waveDataBytes, 0, waveBytes, 42, waveDataBytes.Length);
        waveDataBytes = null;

        return 46;
    }

    #endregion
}

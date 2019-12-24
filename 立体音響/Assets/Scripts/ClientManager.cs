using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Video;
using UniRx;
using UniRx.Triggers;
using WWUtils.Audio;

public class ClientManager : MonoBehaviour
{
    #region SerializeField Define

    [SerializeField] private AudioSource[] speakerAudio;
    [SerializeField] private VideoPlayer screen;

    #endregion SerializeField Define

    #region private Define

    private TransportUDP clientSocket;
    private string ipAddressText;
    private int port;
    // 一つのパケットに含まれるwaveデータの長さ
    private int waveDataBytesParPacketLength;
    // 一つのwaveファイルに含まれるwaveデータのパケット数
    private int waveDataPacketParFile;

    private byte[] waveData;

    private Queue<AudioClip> audioClips;

    #endregion

    private void Start()
    {
        // 通信関連
        waveDataBytesParPacketLength = 441;
        waveDataPacketParFile = 1000;
        clientSocket =
            new TransportUDP(waveDataPacketParFile, waveDataBytesParPacketLength);
        ipAddressText = DataManager.Instance.IpAddressText;
        port = DataManager.Instance.PortNum;
        // 本番コード
        clientSocket.StartListening(ipAddressText, port);
        // waveのデータを0.1sで保存する配列
        // waveファイルに書き出す配列
        waveData = new byte[waveDataBytesParPacketLength * waveDataPacketParFile + 44];
        setWaveHeader(waveData);

        audioClips = new Queue<AudioClip>();

        // If clientSocket.ThreadLoop is true, recieveWaveBytes is called
        var recieveStream =
            this.UpdateAsObservable().Where(_ => clientSocket.ThreadLoop);
        recieveStream.Subscribe(_ => {
            recieveWaveBytes();
        });

        var stopWaveStream =
            this.UpdateAsObservable().Where(_ =>
                (!speakerAudio[0].isPlaying || speakerAudio[0].clip == null));
        stopWaveStream.Subscribe(_ =>
        {
            playWaveFile();
        });
    }

    private void recieveWaveBytes()
    {
        byte[] buffer = new byte[waveDataBytesParPacketLength * waveDataPacketParFile];
        int recvSize = clientSocket.Receive(ref buffer, buffer.Length);
        if (recvSize <= 0)
        {
            buffer = null;
            return;
        }
        Array.Copy(buffer, 0, waveData, 44, buffer.Length);
        setAudioClip(createAudioClip(waveData));
    }

    private AudioClip createAudioClip(byte[] array)
    {
        WAV wav = new WAV(array);
        Debug.Log(wav);
        AudioClip audioClip =
            AudioClip.Create("testSound", wav.SampleCount, 1, wav.Frequency, false);
        audioClip.SetData(wav.LeftChannel, 0);
        return audioClip;
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
        AudioClip clip = audioClips.Dequeue();
        for (int i = 0; i < speakerAudio.Length; i++)
        {
            speakerAudio[i].clip = clip;
        }
        for (int i = 0; i < speakerAudio.Length; i++)
        {
            speakerAudio[i].Play();
        }
        if (!screen.isPlaying)
        {
            screen.Play();
        }
    }

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
        byte[] fmt_size = BitConverter.GetBytes(16);
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

        /*
        // 拡張パラメータののサイズ
        waveBytes[36] = (byte)0;
        waveBytes[37] = (byte)0;
        */

        // "data" : 4bytes
        byte[] data_ASCII = System.Text.Encoding.ASCII.GetBytes("data");
        Debug.Log("data bytes : " + BitConverter.ToString(data_ASCII));
        Array.Copy(data_ASCII, 0, waveBytes, 36, data_ASCII.Length);

        // 波形データのバイト数 : 4bytes
        // ここが分からないため先生の助言どうり-46で試してみる
        byte[] waveDataBytes = BitConverter.GetBytes(size - 46);
        Debug.Log("waveDataBytes : " + BitConverter.ToString(waveDataBytes));
        Array.Copy(waveDataBytes, 0, waveBytes, 40, waveDataBytes.Length);
        waveDataBytes = null;

        return 44;
    }

    public void ClickResetButton()
    {
        clientSocket.StopListening();
        audioClips.Clear();
        for (int i = 0; i < speakerAudio.Length; i++)
        {
            speakerAudio[i].Stop();
            speakerAudio[i].clip = null;
        }
        screen.Stop();
        Start();
    }
}
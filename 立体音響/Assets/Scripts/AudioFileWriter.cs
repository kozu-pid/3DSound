using System.Collections;
using System.Collections.Generic;
using System.IO;

public class AudioFileWriter
{
    /*
    private WaveFileWriter waveFileWriter;
    private List<byte> audioDataList;
    private WaveFormat waveFormat;

    #region constructor
    /// <summary>
    /// ファイル名とwaveのフォーマットを引数としたコンストラクタ
    /// </summary>
    /// <param name="fileName">ファイル名</param>
    /// <param name="waveFormat">wavefileのフォーマット</param>
    public AudioFileWriter(string fileName, WaveFormat waveFormat)
    {
        this.waveFileWriter = new WaveFileWriter(fileName, waveFormat);
        audioDataList = new List<byte>();
    }

    /// <summary>
    /// ファイル名とwaveのフォーマットの中身を引数としたコンストラクタ
    /// </summary>
    /// <param name="filename">ファイル名</param>
    /// <param name="sampleFreq">サンプリング周波数</param>
    /// <param name="bit">量子化ビット数</param>
    /// <param name="channel">チャネル数（２＝ステレオ）</param>
    public AudioFileWriter(string filename, int sampleFreq, int bit, int channel)
    {
        this.waveFileWriter = new WaveFileWriter(filename, new WaveFormat(sampleFreq, bit, channel));
        audioDataList = new List<byte>();
    }

    public AudioFileWriter(string fileName)
    {
        this.waveFileWriter = new WaveFileWriter(fileName, this.waveFormat);
    }
    #endregion constructor

    #region Add method
    public void Add(byte[] byteArrey)
    {
        for (int i = 0; i < byteArrey.Length; i++)
        {
            this.audioDataList.Add(byteArrey[i]);
        }
    }

    public void Add(List<byte> byteList)
    {
        this.audioDataList = byteList;
    }
    #endregion Add method

    #region Write method
    public void Write()
    {
        byte[] audioDataByte = new byte[audioDataList.Count];
        for (int i = 0; i < audioDataByte.Length; i++)
        {
            audioDataByte[i] = audioDataList[i];
        }

        waveFileWriter.Write(audioDataByte, 0, audioDataByte.Length);
    }

    public void Write(byte[] b)
    {
        waveFileWriter.Write(b, 0, b.Length);
    }

    /// <summary>
    /// AudioDataを途中から追加する際に使うメソッド
    /// </summary>
    /// <param name="b">the buffer containing the wave data</param>
    /// <param name="offset">the offset from which to start writing</param>
    public void Write(byte[] b,  int offset)
    {
        waveFileWriter.Write(b, offset, b.Length);
    }
    #endregion Write method

    public void Close()
    {
        audioDataList.Clear();
        waveFileWriter.Close();
    }

    public void SetWaveFormat(WaveFormat waveFormat)
    {
        this.waveFormat = waveFormat;
    }

    public void SetWaveFormat(int sampleFreq, int bit, int channel)
    {
        this.waveFormat = new WaveFormat(sampleFreq, bit, channel);
    }

    public WaveFormat GetWaveFormat()
    {
        return this.waveFormat;
    }
    */
}

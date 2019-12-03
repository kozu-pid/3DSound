import java.io.*;
import java.util.*;
import java.nio.ByteBuffer;


public class WaveFileReader
{
    // ヘッダを作成するメソッド
    private int setWaveHeader(byte[] waveBytes)
    {
        if(waveBytes.length < 44){
            return -1;
        }
        // "RIFF" : 4bytes
        for(int i = 0; i < 4; i++){
            waveBytes[i] = "RIFF".getBytes()[i];
        }

        // チャンクサイズ : 4bytes
        // ファイル全体のサイズ - 8　（"RIFF"と"WAVE"の分）
        int size = waveBytes.length;
        // int -> byte[4]
        byte[] chunk_size = ByteBuffer.allocate(4).putInt(size - 8).array();
        setLittleEndian(chunk_size);
        for(int i = 0; i < 4; i++){
            waveBytes[i + 4] = chunk_size[i];
        }
        chunk_size = null;

        // "WAVE" : 4bytes
        for(int i = 0; i < 4; i++){
            waveBytes[i + 8] = "WAVE".getBytes()[i];
        }

        // "fmt " : 4bytes
        for(int i = 0; i < 4; i++){
            waveBytes[i + 12] = "fmt ".getBytes()[i];
        }

        // fmtチャンクのバイト数　リニアPCMなら16 : 4bytes
        // 今回のWAVファイルは18であったが，16にしてみる
        byte[] fmt_size = ByteBuffer.allocate(4).putInt(16).array();
        setLittleEndian(fmt_size);
        for(int i = 0; i < 4; i++){
            waveBytes[i + 16] = fmt_size[i];
        }
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
        byte[] sampleFreq = ByteBuffer.allocate(4).putInt(44100).array();
        setLittleEndian(sampleFreq);
        for(int i = 0; i < 4; i++){
            waveBytes[i + 24] = sampleFreq[i];
        }
        sampleFreq = null;

        // 1秒あたりバイト数の平均 : 4bytes
        byte[] aveParSec = ByteBuffer.allocate(4).putInt(44100 * 4).array();
        setLittleEndian(aveParSec);
        for(int i = 0; i < 4; i++){
            waveBytes[i + 28] = aveParSec[i];
        }
        aveParSec = null;

        // ブロックサイズ : 2bytes
        waveBytes[32] = (byte)4;
        waveBytes[33] = (byte)0;

        // ビット／サンプル : 2bytes
        waveBytes[34] = (byte)10;
        waveBytes[35] = (byte)0;

        // "data" : 4bytes
        for(int i = 0; i < 4; i++){
            waveBytes[i + 36] = "data".getBytes()[i];
        }

        // 波形データのバイト数 : 4bytes
        // ここが分からないため先生の助言どうり-44で試してみる
        byte[] waveDataBytes = ByteBuffer.allocate(4).putInt(size - 44).array();
        setLittleEndian(waveDataBytes);
        for(int i = 0; i < 4; i++){
            waveBytes[i + 40] = waveDataBytes[i];
        }
        waveDataBytes = null;

        return 44;
    }

    // リトルエンディアンに変更するためのスワップメソッド
    private void swap(byte[] arr, int a, int b){
        byte temp = arr[a];
        arr[a] = arr[b];
        arr[b] = temp;
    }

    // リトルエンディアンに変更するメソッド
    private void setLittleEndian(byte[] arr){
        int i = 0;
        while(i <= ((arr.length - 1) - i)){
            swap(arr, i, (arr.length - 1) - i);
            i++;
        }
    }

    public static void main(String[] args)
    {
        WaveFileReader wfr = new WaveFileReader();
        byte[] a = new byte[44];
        wfr.setWaveHeader(a);
        // ヘッダのデバッグ
        for(int i = 0; i < a.length; i++){
            System.out.print(String.format("%02x", a[i]) + " ");
        }

        try
        {
            File inputFile = new File("data/" + args[0]);

            if(inputFile.exists())
            {
                BufferedInputStream reader = new BufferedInputStream(new FileInputStream(inputFile));
                // ヘッダの読み飛ばし(God knowsはヘッダが46あるため：通常は44)
                int data;
                for(int i = 0; i < 46; i++){
                    data = reader.read();
                }
                int outputIndex = 0;
                while ((data = reader.read()) != -1) {
                    byte[] waveSplitedBits = new byte[80000];
                    int headerSize = wfr.setWaveHeader(waveSplitedBits);
                    waveSplitedBits[headerSize] = (byte)data;
                    // 読み込み動作，実際はI・Oを分けるため今回も分ける
                    for(int i = headerSize + 1; i < waveSplitedBits.length; i++){
                        if((data = reader.read()) != -1){
                            waveSplitedBits[i] = (byte)data;
                        }
                    }
                    File outputFile = new File("data/test" + outputIndex + ".wav");
                    outputIndex++;
                    BufferedOutputStream writer = new BufferedOutputStream(new FileOutputStream(outputFile));
                    writer.write(waveSplitedBits, 0, waveSplitedBits.length);
                    writer.close();
                }
                //ファイルクローズ
                reader.close();
            }
            else
            {
                System.out.print("ファイルは存在しません");
            }
        }
        catch(IOException e){
            e.printStackTrace();
        }
        
    }
}

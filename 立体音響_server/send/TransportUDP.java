import java.io.*;
import java.net.*;
import java.util.*;
import java.nio.ByteBuffer;

public class TransportUDP
{
  	private DatagramSocket socket;             // Socket
  	private String destIP;                     // 宛先IPアドレス
	private String destPort;                   // 宛先UDPポート番号
  	private TransmitThread transmitThread;     // 送信スレッド
  	private String filename;                   // 送信ファイル名
	private int sendUnit;

  	public TransportUDP(DatagramSocket socket,String destIP,String destPort,String filename, int sendUnit)
  	{
		this.socket = socket;
		this.destIP = destIP;
		this.destPort = destPort;
		this.filename = filename;
		this.sendUnit = sendUnit;
  	}
	
  	public void start()
  	{
		this.transmitThread = new TransmitThread(this.socket,this.destIP,this.destPort,this.filename, this.sendUnit);
		this.transmitThread.start();
  	}
	
  	public void stop()
  	{
		this.transmitThread.transmitStop();
  	}
}

class TransmitThread extends Thread
{
 	private String destIP;
  	private String destPort;
	private String filename;
	private int freq;
	private int sendUnit;
	private int sleepSec;
	private int mtu;
	
  	private DatagramSocket socket;
  	private boolean isStop;
	
  	// コンストラクタ
  	public TransmitThread(DatagramSocket socket, String destIP, String destPort, String filename, int sendUnit)
  	{
		this.socket = socket;
		this.destIP = destIP;
		this.destPort = destPort;
		this.filename = filename;
		this.isStop = false;
		this.freq = 44100;
		this.sendUnit = sendUnit;
		// sleepする時間　送信した分だけsleepしたい
		// this.sleepSec = 1000 * sendUnit / (this.freq * 4);
		this.sleepSec = 1;
		// 送信単位＋シーケンスヘッダ：4bytes
		this.mtu = sendUnit + 4;
	}
	  
  	long sentSize = 0;
  	int sentPacket = 0;
    
	// スレッド開始
  	public void run()
	{
		try
	  	{
			//本番コード
			InetSocketAddress address = new InetSocketAddress(this.destIP,Integer.parseInt(this.destPort));

			//ローカルテスト用
			// InetSocketAddress address = new InetSocketAddress("localhost",10005);
			
			DatagramPacket packet = null;

			// オーディオ入力ストリームを取得する
			File inputFile = new File(filename);

			if(!inputFile.exists()){
				System.out.println("ファイルは存在しません");
				return;
			}
			else{
				BufferedInputStream reader = new BufferedInputStream(new FileInputStream(inputFile));
				// ヘッダの読み飛ばし(God knowsはヘッダが46あるため：通常は44)
                int data;
                for(int i = 0; i < 46; i++){
                    data = reader.read();
				}
				
				int logPoint = 0;
				while(((data = reader.read())!= -1) && !isStop){
					// プログラムが正常に動いているかどうかのログ
					if(logPoint%100==0)
						System.out.print(".");
					logPoint++;
					// 音データを入れる送信用のbyte配列
					byte[] waveSplitedBytes = new byte[mtu];
					// 送信パケットの先頭にseq : 4byteつける
					setSeqHeader(waveSplitedBytes, sentPacket);
					// whileで読み込んでしまった分の回収
					waveSplitedBytes[4] = (byte)data;
					for(int i = 5; i < waveSplitedBytes.length; i++){
						if((data = reader.read()) != -1){
							waveSplitedBytes[i] = (byte)data;
                        }
					}
					// 送信処理
					packet = new DatagramPacket(waveSplitedBytes, waveSplitedBytes.length,address);
					this.socket.send(packet);
					this.sentSize += (waveSplitedBytes.length - 4);
					this.sentPacket++;

					// スレッドの停止
					try{
						// System.out.println("Thread is sleep : " + sleepSec + "(msec)");
						Thread.sleep(sleepSec);
					}
					catch(InterruptedException ie){
						ie.printStackTrace();
					}
				}
				socket.close();
				reader.close();
			}	
	  	}catch(Exception e)
	  	{
			e.printStackTrace();
	  	}
	}

  	// 送信　スレッド停止
  	public void transmitStop()
  	{
		System.out.println("Sent Data = "+sentSize);
		System.out.println("Sent Packets = "+sentPacket);
		this.isStop = true;
  	}

	//　送信用データにseqのヘッダ4bytesをつけるメソッド
	private void setSeqHeader(byte[] waveData, int seq){
		byte[] seqBytes = ByteBuffer.allocate(4).putInt(seq).array();
        for(int i = 0; i < 4; i++){
        	waveData[i] = seqBytes[i];
		}
	}
}


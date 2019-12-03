import java.io.IOException;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetSocketAddress;
 
public class Client {
 
	public static void main(String[] args) throws IOException {
		String sendData = "UDPてすとですよ";//送信データ
		byte[] data = sendData.getBytes("UTF-8");//UTF-8バイト配列の作成
		DatagramSocket sock = new DatagramSocket();//UDP送信用ソケットの構築
		DatagramPacket packet = new DatagramPacket(data, data.length,new InetSocketAddress("localhost",10005));//指定アドレス、ポートへ送信するパケットを構築
		sock.send(packet);//パケットの送信
		System.out.println("UDP送信:"+sendData);//送信データにの表示
		sock.close();//ソケットのクローズ
	}
}
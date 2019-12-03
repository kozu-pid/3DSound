import java.io.*;
import java.util.*;
import java.net.*;

public class SoundSender
{
    public static void main(String[] args)
    {
		try 
		{
			
	    	/*
	     	 * usage:
	     	 * $ java SoundSender <dstIPaddr> <srcIPaddr> <dstport> <srcport> <filename> <sendUnit>
			 * 
			 * <dstIPaddr>    送信先端末のIPアドレスを指定する．１０進表記．(e.g. 192.168.0.1)
	     	 * <srcIPaddr>    送信元端末のIPアドレスを指定する．１０進表記．(e.g. 192.168.0.1)
	     	 * <dstport>   送信先端末のデータを受信するポート番号を任意の整数で指定する．
	     	 * <srcport>   データを送信するポート番号． 任意の整数． （自端末のポート番号を指定）
			 * <filename>  送信するファイル名．
			 * <sendUnit>　送信するパケットのbyte単位 
	     	 */

	    	int recvPort = Integer.parseInt(args[3]);
 	    	String filename = "data/"+args[4];
			
	    	// 通信用ソケット
			// DatagramSocket socket = new DatagramSocket(recvPort,InetAddress.getLocalHost());
			DatagramSocket socket = new DatagramSocket(recvPort,InetAddress.getByName(args[1]));
	    	System.out.println("Transmit IP:Port = " 
			    + socket.getLocalAddress().getHostAddress()
			    + ":" + socket.getLocalPort());
			
	    	// 送信
	    	TransportUDP transmitter = new TransportUDP(socket,args[0],args[2],filename, Integer.parseInt(args[5]));
	    	transmitter.start();
	    	BufferedReader reader = new BufferedReader(new InputStreamReader(System.in));
			
	    	System.out.println("Type <ENTER> to exit this program.\n");
			while(true)
			{
				if(reader.readLine().equals("") == true)
		    	break;
	    	}
			
	    	transmitter.stop();			// 送信　停止
	    	System.out.println("Connection Closed.");
			
		} catch(Exception e) 
		{
	    	e.printStackTrace();
		}
    }
}

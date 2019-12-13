// パケットデータをスレッド間で共有するためのバッファ
//
// ■プログラムの説明
// MemoryStreamを使用してパケットのキューイングを行うモジュールです.
// MemoryStreamはデータを1次元のストリームで管理するクラスです.
// 送受信するパケットはデータサイズがデータの種類により不定になるため効率よくバッファリングするMemoryStreamを使用するとよいでしょう.
// データサイズが不定なためデータの先頭位置とサイズをパケットごとに保存してキューイングします.
// ゲームプログラムの Send() 関数ではキューの最後尾に送信するためのデータを追加します.実際のソケットによる送信時にキューの先頭から取り出して送信します.
// ゲームプログラムの Recieve() 関数でキューに溜まっているデータを先頭から取り出します.実際のソケットによる受信時にキューの最後尾に追加します.
// 

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

public class PacketQueue_pre
{
	// パケット格納情報.
	struct PacketInfo
	{
		public int	offset;
		public int 	size;
        public int index;
	};
	
	//
	private MemoryStream 		m_streamBuffer;
	
	private List<PacketInfo>	m_offsetList;
	
	private int					m_offset = 0;

    private int leadIndex;
    public int LeadIndex
    {
        get { return leadIndex; }
    }
    public int Length
    {
        get { return m_offsetList.Count; }
    }

    // private System.Object lockObj = new System.Object();

    //  コンストラクタ(ここで初期化を行います).
    public PacketQueue_pre()
	{
		m_streamBuffer = new MemoryStream();
		m_offsetList = new List<PacketInfo>();
        leadIndex = 0;
	}
	
	// キューを追加.
	public int Enqueue(byte[] data, int srcOffset, int size, int index)
	{
		PacketInfo	info = new PacketInfo();
	    info.offset = m_offset;
		info.size = size;
        info.index = index;
		// パケット格納情報を保存します.
		m_offsetList.Add(info);
		// パケットデータを保存します.
		m_streamBuffer.Position = m_offset;
        m_streamBuffer.Write(data, srcOffset, size);
        m_streamBuffer.Flush();
		m_offset += size;
		
		return size;
	}

    // キューを指定箇所に追加
    public int Insertqueue(byte[] data, int srcOffset, int size, int index)
    {
        // infoはすでにあるため格納しない
        m_streamBuffer.Position = index * size;
        m_streamBuffer.Write(data, srcOffset, size);
        m_streamBuffer.Flush();
        return size;
    }
	
	// キューの取り出し.
	public int Dequeue(ref byte[] buffer, int size) {
		if (m_offsetList.Count <= 0) {
			return -1;
		}

		int recvSize = 0;
        PacketInfo info = m_offsetList[0];
		// バッファから該当するパケットデータを取得します.
		int dataSize = Math.Min(size, info.size);
		m_streamBuffer.Position = info.offset;
		recvSize = m_streamBuffer.Read(buffer, 0, dataSize);
        // キューデータを取り出したので先頭要素を削除します.
		if (recvSize > 0) {
			m_offsetList.RemoveAt(0);
		}

		// すべてのキューデータを取り出したときはストリームをクリアしてメモリを節約します.
		if (m_offsetList.Count == 0) {
			Clear();
			m_offset = 0;
		}
				
		return recvSize;
	}

    // 複数キューの取り出し.
    public int Dequeue(ref byte[] buffer, int size, int dataPacketsParFile)
    {
        if (m_offsetList.Count <= dataPacketsParFile)
        {
            return -1;
        }
        int recvSize = 0;
        PacketInfo info = m_offsetList[0];
        // バッファから該当するパケットデータを取得
        int dataSize = size;
        m_streamBuffer.Position = info.offset;
        recvSize = m_streamBuffer.Read(buffer, 0, dataSize);
        // キューデータを取り出したので先頭要素を削除します.
        for (int i = 0; i < dataPacketsParFile; i++)
        {
            if (m_offsetList.Count > 0)
            {
                m_offsetList.RemoveAt(0);
                leadIndex = m_offsetList[0].index;
            }
        }
        leadIndex = m_offsetList[0].index;
        // すべてのキューデータを取り出したときはストリームをクリアしてメモリを節約します.
        if (m_offsetList.Count == 0)
        {
            Clear();
            m_offset = 0;
        }

        return recvSize;
    }

    // キューをクリア.	
    public void Clear()
	{
		byte[] buffer = m_streamBuffer.GetBuffer();
		Array.Clear(buffer, 0, buffer.Length);
		
		m_streamBuffer.Position = 0;
		m_streamBuffer.SetLength(0);
	}

}


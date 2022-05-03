using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using UnityEngine.UI;
using System;

public class Echo : MonoBehaviour {

	//定义套接字
	Socket socket;
	//UGUI
	public InputField InputFeld;
	public Text text;
	byte[] readBuff = new byte[1024];
	string recvStr = "";
	
	void Update()
	{
		if (socket == null)
			return;
		if (socket.Poll(0, SelectMode.SelectRead))
		{
			byte[] readBuff = new byte[1024];
			int count = socket.Receive(readBuff);
			string s = System.Text.Encoding.UTF8.GetString(readBuff, 0, count);
			recvStr = s + "\n" + recvStr;
			text.text = recvStr;
		}
		
	}

	//点击连接按钮
	public void Connetion()
	{
		//Socket
		socket = new Socket(AddressFamily.InterNetwork,
			SocketType.Stream, ProtocolType.Tcp);
		//Connect
		socket.BeginConnect("127.0.0.1", 8888, ConnectCallBack, socket);
	}

	void ConnectCallBack(IAsyncResult ar)
	{
		try
		{
			Socket socketAsync = (Socket)ar.AsyncState;
			socketAsync.EndConnect(ar);
			Debug.Log("Socket Connect Succ");

		}catch(Exception e)
		{
			Debug.Log("Socket Connect Fail: " + e.ToString());
		}
		
	}

	void ReceiveCallBack(IAsyncResult ar)
	{
		try
		{
			Socket socketAsync = (Socket)ar.AsyncState;
			int count = socketAsync.EndReceive(ar);
			string s = System.Text.Encoding.UTF8.GetString(readBuff, 0, count);
			recvStr = s + "\n" + recvStr;
			socketAsync.BeginReceive(readBuff, 0, 1024, 0, ReceiveCallBack, socketAsync);
		} 
		catch(Exception e)
		{
			Debug.Log("Receive Fail: " + e.ToString());
		}

	}

	//点击发送按钮
	public void Send()
	{
		//Send
		string sendStr = InputFeld.text;
		byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
		socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallBack, socket);
		
	}

	void SendCallBack(IAsyncResult ar)
	{
		try
		{
			Socket socketAsync = (Socket)ar.AsyncState;
			int count = socketAsync.EndSend(ar);
			Debug.Log("Send Succ: bytes " + count);

		} catch (Exception e)
		{
			Debug.Log("Send Fail: " + e.ToString());
		}
	}
}

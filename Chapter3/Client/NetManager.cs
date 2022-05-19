using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System;
using UnityEngine;

public static class NetManager
{

	//定义套接字
	static Socket socket;
	// 接收缓冲区
	static byte[] readBuff = new byte[1024];
	// 消息队列
	static List<string> msgList = new List<string>();
	// 委托类型
	public delegate void MsgListener(string str);
	// 监听列表
	private static Dictionary<string, MsgListener> listeners = new Dictionary<string, MsgListener>();

	// 获取描述
	public static string GetDesc()
	{
		if (socket == null)
			return "";
		if (!socket.Connected)
			return "";
		return socket.LocalEndPoint.ToString();
	}

	//点击连接按钮
	public static void Connect(string ip, int port)
	{
		//Socket
		socket = new Socket(AddressFamily.InterNetwork,
			SocketType.Stream, ProtocolType.Tcp);
		//Connect
		socket.BeginConnect(ip, port, ConnectCallBack, socket);
	}

	static void ConnectCallBack(IAsyncResult ar)
	{
		try
		{
			Socket socketAsync = (Socket)ar.AsyncState;
			socketAsync.EndConnect(ar);
			socket.BeginReceive(readBuff, 0, 1024, 0, ReceiveCallBack, socket);
		}
		catch (Exception e)
		{
			Debug.LogError("Socket Connect Fail: " + e.ToString());
		}

	}

	static void ReceiveCallBack(IAsyncResult ar)
	{
		try
		{
			Socket socketAsync = (Socket)ar.AsyncState;
			int count = socketAsync.EndReceive(ar);
			string message = System.Text.Encoding.UTF8.GetString(readBuff, 0, count);
			msgList.Add(message);
			socketAsync.BeginReceive(readBuff, 0, 1024, 0, ReceiveCallBack, socketAsync);
		}
		catch (Exception e)
		{
			Debug.Log("Receive Fail: " + e.ToString());
		}

	}

	//点击发送按钮
	public static void Send(string sendStr)
	{
		//Send
		byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
		socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallBack, socket);
	}

	static void SendCallBack(IAsyncResult ar)
	{
		try
		{
			Socket socketAsync = (Socket)ar.AsyncState;
			int count = socketAsync.EndSend(ar);
			Debug.Log("Send Succ: bytes " + count);

		}
		catch (Exception e)
		{
			Debug.Log("Send Fail: " + e.ToString());
		}
	}
	
	public static void AddListener(string msgName, MsgListener listener)
	{
		listeners[msgName] = listener;
	}
	

	public static void Update()
	{
		if (msgList.Count == 0)
			return;
		string message = msgList[0];
		msgList.RemoveAt(0);
		string[] msgSplit = message.Split('|');
		string msgName = msgSplit[0];
		string msgArgs = msgSplit[1];
		// 监听回调
		if (listeners.ContainsKey(msgName))
		{
			listeners[msgName](msgArgs);
		}

	}
}

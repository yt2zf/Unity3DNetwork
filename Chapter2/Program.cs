using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace EchoServer
{	
	class ClientState
	{
		public Socket socket;
		public byte[] readBuff = new byte[1024];
	}
	class MainClass
	{
		// clients
		static Dictionary<Socket, ClientState> clientStates = new Dictionary<Socket, ClientState>();
		public static void Main (string[] args)
		{
			Console.WriteLine ("Hello World!");
		
			//Socket
			Socket listenfd = new Socket(AddressFamily.InterNetwork,
				SocketType.Stream, ProtocolType.Tcp);
			//Bind
			IPAddress ipAdr = IPAddress.Parse("127.0.0.1");
			IPEndPoint ipEp = new IPEndPoint(ipAdr, 8888);
			listenfd.Bind(ipEp);
			//Listen
			listenfd.Listen(0);
			Console.WriteLine("[服务器]启动成功");

			// 异步Accpet
			listenfd.BeginAccept(AcceptCallBack, listenfd);
			// 等待
			Console.ReadLine();
		}

		public static void AcceptCallBack(IAsyncResult ar)
		{
			try
			{
				Socket listenfd = (Socket)ar.AsyncState;
				Socket connfd = (Socket)listenfd.EndAccept(ar);
				ClientState cs = new ClientState { socket = connfd };
				clientStates[connfd] = cs;
				connfd.BeginReceive(cs.readBuff, 0, 1024, 0, ReceiveCallBack, cs);
				listenfd.BeginAccept(AcceptCallBack, listenfd);
			} catch(SocketException e)
			{
				Console.WriteLine("Socket Accept Fail: " + e.ToString());
			}
		}

		public static void ReceiveCallBack(IAsyncResult ar)
		{
			try
			{
				ClientState cs = (ClientState)ar.AsyncState;
				Socket clientfd = cs.socket;
				int count = clientfd.EndReceive(ar);
				if (count == 0)
				{
					clientfd.Close();
					clientStates.Remove(clientfd);
					Console.WriteLine("Socket Close");
					return;
				}
				string recvStr = System.Text.Encoding.UTF8.GetString(cs.readBuff, 0, count);
				Console.WriteLine("[服务器接收]" + recvStr);
				//Send, 不异步
				string sendStr = "Echo: " + recvStr;
				byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
				clientfd.Send(sendBytes);
				clientfd.BeginReceive(cs.readBuff, 0, 1024, 0, ReceiveCallBack, cs);
			}
			catch (SocketException e)
			{
				Console.WriteLine("Socket Receive Fail: " + e.ToString());
			}
		}
	}
}

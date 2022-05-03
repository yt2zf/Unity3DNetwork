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
		// clients 初始化clients列表
		static Dictionary<Socket, ClientState> clientStates = new Dictionary<Socket, ClientState>();
		
		public static void Main (string[] args)
		{
			Console.WriteLine ("Hello World!");
		
			//Socket
			Socket listenfd = new Socket(AddressFamily.InterNetwork,
				SocketType.Stream, ProtocolType.Tcp);
			// check read
			List<Socket> checkRead = new List<Socket>();
			//Bind
			IPAddress ipAdr = IPAddress.Parse("127.0.0.1");
			IPEndPoint ipEp = new IPEndPoint(ipAdr, 8888);
			listenfd.Bind(ipEp);
			//Listen
			listenfd.Listen(0);
			Console.WriteLine("[服务器]启动成功");
			while (true)
			{
				checkRead.Clear();
				checkRead.Add(listenfd);
				foreach (ClientState cs in clientStates.Values)
				{
					checkRead.Add(cs.socket);
				}
				// 多路复用
				Socket.Select(checkRead, null, null, 1000);
				foreach(Socket s in checkRead)
				{
					if (s == listenfd)
					{
						ReadListenfd(s);
					}
					else
					{
						ReadClientfd(s);
					}
				}
			}
		}

		public static bool ReadListenfd(Socket lfd)
		{
			try
			{
				Socket cfd = lfd.Accept();
				ClientState cs = new ClientState();
				cs.socket = cfd;
				clientStates[cfd] = cs;
				return true;
			} catch (SocketException e)
			{
				Console.WriteLine("Socket Accept Fail: " + e.ToString());
				return false;
			}
		}

		public static bool ReadClientfd(Socket cfd)
		{
			ClientState cliState = clientStates[cfd];
			int count = 0;
			try
			{
				count = cfd.Receive(cliState.readBuff);
			} catch(SocketException e)
			{
				cfd.Close();
				clientStates.Remove(cfd);
				Console.WriteLine("Socket Receive Fail: " + e.ToString());
				return false;
			}
			if (count == 0)
			{
				cfd.Close();
				clientStates.Remove(cfd);
				Console.WriteLine("Socket Close");
				return false;
			}
			string recvStr = System.Text.Encoding.UTF8.GetString(cliState.readBuff, 0, count);
			Console.WriteLine("[服务器接收]" + recvStr);
			string sendStr = cfd.RemoteEndPoint.ToString() + ": " + recvStr;
			byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
			//广播
			foreach(ClientState cs in clientStates.Values)
			{
				cs.socket.Send(sendBytes);
			}

			return true;
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
				string sendStr = clientfd.RemoteEndPoint.ToString() + ": " + recvStr;
				byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
				foreach (ClientState cliState in clientStates.Values)
				{
					cliState.socket.Send(sendBytes);
				}
				clientfd.BeginReceive(cs.readBuff, 0, 1024, 0, ReceiveCallBack, cs);
			}
			catch (SocketException e)
			{
				ClientState cs = (ClientState)ar.AsyncState;
				Socket clientfd = cs.socket;
				clientfd.Close();
				clientStates.Remove(clientfd);
				Console.WriteLine("Socket Receive Fail: " + e.ToString());
			}
		}
	}
}

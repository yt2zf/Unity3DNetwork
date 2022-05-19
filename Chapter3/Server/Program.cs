using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace EchoServer
{	
	class ClientState
	{
		public Socket socket;
		public byte[] readBuff = new byte[1024];
		public int hp = -100;
		public float x = 0;
		public float y = 0;
		public float z = 0;
		public float eulY = 0;
	}
	class MainClass
	{
		// clients 初始化clients列表
		public static Dictionary<Socket, ClientState> clientStates = new Dictionary<Socket, ClientState>();
		
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
				MethodInfo connectEvent = typeof(EventHandler).GetMethod("OnConnect");
				Object[] evArgs = { cs };
				connectEvent.Invoke(null, evArgs);
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
				MethodInfo disconnectEvent = typeof(EventHandler).GetMethod("OnDisconnect");
				Object[] evArgs = { cliState };
				disconnectEvent.Invoke(null, evArgs);

				cfd.Close();
				clientStates.Remove(cfd);
				Console.WriteLine("Socket Receive Fail: " + e.ToString());
				return false;
			}
			if (count <= 0)
			{
				MethodInfo disconnectEvent = typeof(EventHandler).GetMethod("OnDisconnect");
				Object[] evArgs = { cliState };
				disconnectEvent.Invoke(null, evArgs);

				cfd.Close();
				clientStates.Remove(cfd);
				Console.WriteLine("Socket Close");
				return false;
			}
			string recvStr = System.Text.Encoding.UTF8.GetString(cliState.readBuff, 0, count);
			Console.WriteLine("[服务器接收]" + recvStr);
			string[] msgSplit = recvStr.Split('|');
			string msgName = msgSplit[0];
			string msgArgs = msgSplit[1];
			string funcName = "Msg" + msgName;
			MethodInfo mi = typeof(MsgHandler).GetMethod(funcName);
			Object[] args = { cliState, msgArgs };
			mi.Invoke(null, args);
			return true;
		}

		public static void Send(ClientState cs, string sendStr)
		{
			byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
			// cs.socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallBack, cs.socket);
			cs.socket.Send(sendBytes);
		}

		private static void SendCallBack(IAsyncResult ar)
		{
			try
			{
				Socket socketAsync = (Socket)ar.AsyncState;
				socketAsync.EndSend(ar);
			} catch(SocketException e)
			{
				Console.WriteLine("Socket Send Fail: " + e.ToString());
			}
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

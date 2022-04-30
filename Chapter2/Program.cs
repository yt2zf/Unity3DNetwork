using System;
using System.Net;
using System.Net.Sockets;

namespace EchoServer
{
	class MainClass
	{
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
			
			while (true) {
				//Accept
				Socket connfd = listenfd.Accept();
				Console.WriteLine("[服务器]Accept");
				//Receive
				byte[] readBuff = new byte[1024];
				int count = connfd.Receive (readBuff);
				string readStr = System.Text.Encoding.UTF8.GetString (readBuff, 0, count);
				Console.WriteLine ("[服务器接收]" + readStr);
				//Send
				string sendStr = System.DateTime.Now.ToString();
				byte[] sendBytes = System.Text.Encoding.Default.GetBytes (sendStr);
				connfd.Send (sendBytes);
			}
		}
	}
}

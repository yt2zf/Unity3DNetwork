using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EchoServer
{
    class EventHandler
    {
        public static void OnConnect(ClientState cs)
        {
            Console.WriteLine("OnConnect: " + cs.socket.RemoteEndPoint.ToString());
        }

        public static void OnDisconnect(ClientState cs)
        {
            Console.WriteLine("OnDisconnect: " + cs.socket.RemoteEndPoint.ToString());
            string sendStr = "Leave|" + cs.socket.RemoteEndPoint.ToString() + ",";

            foreach (var cliState in MainClass.clientStates.Values)
            {
                MainClass.Send(cliState, sendStr);
            }
        }
    }
}

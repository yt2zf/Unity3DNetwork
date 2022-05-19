using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EchoServer
{
    class MsgHandler
    {
        public static void MsgEnter(ClientState cs, string msgArgs)
        {
            Console.WriteLine("MsgEnter " + msgArgs);
            string[] msgSplit = msgArgs.Split(',');
            cs.x = float.Parse(msgSplit[1]);
            cs.y = float.Parse(msgSplit[2]);
            cs.z = float.Parse(msgSplit[3]);
            cs.eulY = float.Parse(msgSplit[4]);
            cs.hp = int.Parse(msgSplit[5]);

            string senderCharInfo = "";
            senderCharInfo += cs.socket.RemoteEndPoint.ToString() + ",";
            senderCharInfo += cs.x.ToString() + ",";
            senderCharInfo += cs.y.ToString() + ",";
            senderCharInfo += cs.z.ToString() + ",";
            senderCharInfo += cs.eulY.ToString() + ",";
            senderCharInfo += cs.hp.ToString() + ",";


            string allCharInfo = "";
            foreach(var cliState in MainClass.clientStates.Values)
            {
                allCharInfo += cliState.socket.RemoteEndPoint.ToString() + ",";
                allCharInfo += cliState.x.ToString() + ",";
                allCharInfo += cliState.y.ToString() + ",";
                allCharInfo += cliState.z.ToString() + ",";
                allCharInfo += cliState.eulY.ToString() + ",";
                allCharInfo += cliState.hp.ToString() + ",";
                if (cliState != cs)
                {
                    MainClass.Send(cliState, "Enter|" + senderCharInfo);
                }
            }
            MainClass.Send(cs, "Enter|" + allCharInfo);
        }

        public static void MsgMove(ClientState cs, string msgArgs)
        {
            Console.WriteLine("MsgMove: " + msgArgs);
            string[] msgSplit = msgArgs.Split(',');
            float x = float.Parse(msgSplit[1]);
            float y = float.Parse(msgSplit[2]);
            float z = float.Parse(msgSplit[3]);
            cs.x = x;
            cs.y = y;
            cs.z = z;

            string sendStr = "Move|" + msgArgs;
            foreach(var cliState in MainClass.clientStates.Values)
            {
                MainClass.Send(cliState, sendStr);
            }
        }

        public static void MsgAttack(ClientState cs, string msgArgs)
        {
            Console.WriteLine("MsgAttack: " + msgArgs);
            string[] msgSplit = msgArgs.Split(',');
            float eulY = float.Parse(msgSplit[1]);
            cs.eulY = eulY;

            string sendStr = "Attack|" + msgArgs;
            foreach(var cliState in MainClass.clientStates.Values)
            {
                MainClass.Send(cliState, sendStr);
            }
        }

        public static void MsgHit(ClientState cs, string msgArgs)
        {
            Console.WriteLine("MsgHit: " + msgArgs);
            string[] msgSplit = msgArgs.Split(',');
            string hitDesc = msgSplit[0];

            ClientState hitCS = null;
            foreach(var cliState in MainClass.clientStates.Values)
            {
                if (cliState.socket.RemoteEndPoint.ToString() == hitDesc)
                {
                    hitCS = cliState;
                    break;
                }
            }

            if (hitCS == null)
                return;

            hitCS.hp -= 25;
            if (hitCS.hp <= 0)
            {
                string sendStr = "Die|" + hitDesc + ",";
                foreach(var cliState in MainClass.clientStates.Values)
                {
                    MainClass.Send(cliState, sendStr);
                }
            }
        }
    }
}

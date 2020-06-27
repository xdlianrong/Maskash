using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace Client
{
    class SocketClass
    {
        private static Socket ClientSocket;
        public static string Send(string MessageStr)
        {
            String IP = "106.15.191.148";
            int port = 10086;

            IPAddress ip = IPAddress.Parse(IP);
            ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint endPoint = new IPEndPoint(ip, port);
            ClientSocket.Connect(endPoint);

            byte[] message = Encoding.UTF8.GetBytes(MessageStr);
            ClientSocket.Send(message);

            byte[] receive = new byte[1024];
            ClientSocket.Receive(receive);
            string output = Encoding.UTF8.GetString(receive);

            ClientSocket.Close();
            return output;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using ServiceStack.Redis;

namespace ProxyServerTCP
{
    class Program
    {
        static void Main(string[] args)
        {
            CashController.Flush("localhost:6379");

            var loadbalancer = LoadBalancer.Instance;
            loadbalancer.addServer(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 54523));
            loadbalancer.addServer(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 54539));

            TcpListener myTCP = new TcpListener(IPAddress.Parse("127.0.0.1"), 8888);

            myTCP.Start();
            Console.WriteLine("Proxy Server started...");

            while (true)
            {
                if (myTCP.Pending())
                {
                    Thread t = new Thread(ExecuteRequest);
                    t.IsBackground = true;
                    t.Start(myTCP.AcceptSocket());
                }
            }

            myTCP.Stop();
        }

        private static void ExecuteRequest(object arg)
        {

            Socket myClient = (Socket)arg;
            if (myClient.Connected)
            {
                byte[] httpRequest = ReadToEnd(myClient);
                Regex myReg = new Regex(@"^(?<method>[A-Z]*) (?<path>\/.*) .*\s+", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                Match ReqMatch = myReg.Match(System.Text.Encoding.ASCII.GetString(httpRequest));

                //string host = ReqMatch.Groups["host"].Value;
                int port = 0;
                if (!int.TryParse(ReqMatch.Groups["port"].Value, out port)) { port = 80; }

                string path = ReqMatch.Groups["path"].Value;
                string method = ReqMatch.Groups["method"].Value;
                Console.WriteLine("Request to {0}, method:{1}", path, method);

                string cash = null;

                if(method == "GET")
                {
                    cash = CashController.Get("localhost:6379", path);
                    if (cash != null)
                    {
                        byte[] _buffer = Encoding.ASCII.GetBytes(cash);
                        if (_buffer != null && _buffer.Length > 0)
                        {
                            myClient.Send(_buffer, _buffer.Length, SocketFlags.None);
                        }
                    }
                    else
                        method = "";
                }

                //IPHostEntry myIPHostEntry = Dns.GetHostEntry(host);
                //IPEndPoint myIPEndPoint = new IPEndPoint(myIPHostEntry.AddressList[0], port);

                if (method != "GET")
                {
                    var loadb = LoadBalancer.Instance;
                    IPEndPoint myIPEndPoint = loadb.getServerPoint();

                    byte[] httpResponse = null;
                    using (Socket myRerouting = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                    {

                        myRerouting.Connect(myIPEndPoint);
                        if (myRerouting.Send(httpRequest, httpRequest.Length, SocketFlags.None) != httpRequest.Length)
                        {
                            Console.WriteLine("Error when send data to remote server!");
                        }
                        else
                        {
                            httpResponse = ReadToEnd(myRerouting);
                            if (httpResponse != null && httpResponse.Length > 0)
                            {
                                myClient.Send(httpResponse, httpResponse.Length, SocketFlags.None);
                            }
                        }

                        String msgString = Encoding.ASCII.GetString(httpResponse);
                        if(!CashController.Save("localhost:6379", path, msgString))
                        {
                            Console.WriteLine("Error! Can not cash data");
                        }
                    }

                    loadb.remuveConnection(myIPEndPoint);
                    myClient.Close();
                }
            }
        }

        private static byte[] ReadToEnd(Socket mySocket)
        {
            byte[] b = new byte[mySocket.ReceiveBufferSize];
            int len = 0;
            using (MemoryStream m = new MemoryStream())
            {
                while (mySocket.Poll(1000000, SelectMode.SelectRead) && (len = mySocket.Receive(b, mySocket.ReceiveBufferSize, SocketFlags.None)) > 0)
                {
                    m.Write(b, 0, len);
                }
                return m.ToArray();
            }
        }

    }
}
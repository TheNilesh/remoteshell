using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;

namespace RemoteShell
{
    class RelayConn
    {
        private Socket relay;

        //configuration
        IPAddress relayIp;
        int relayPort;
        string ID;

        public RelayConn()
        {
            relayPort = 2026;
            ID = "FOO";

            IPHostEntry ipHostInfo = Dns.GetHostEntry("127.0.0.1");
            foreach( IPAddress ip in ipHostInfo.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork) //only if its IPv4 Address
                {
                    relayIp = ip;
                }
            }
            if (relayIp == null)
            {
                Console.WriteLine("No IPv4 Address found");
                return;
            }

            while(true)
            {
                bool success = conn(relayIp, relayPort);
                if (success)
                {
                    Thread listener = new Thread(new ThreadStart(communicate)); //listen to stream infinitely
                    listener.Start();
                    break;//out of loop
                }
                else
                {
                    Console.WriteLine("Connection failed. Retrying after a minute");
                    Thread.Sleep(60000);    //wait a minute
                }
            }//while
        }

        private void communicate()
        {
            StreamReader inStream=new StreamReader(new NetworkStream(relay));   //Stream creation
            StreamWriter outStream= new StreamWriter(new NetworkStream(relay));   //Stream creation

            string msg = "";
            outStream.WriteLine(ID); //identity
            outStream.Flush();
            
          try
          {
                while (true)    //listen from relay
                {
                        msg = inStream.ReadLine();
                        Console.WriteLine("R:" + msg);
                        if (msg.StartsWith("CONNECT_TO"))
                        {
                            string[] arg = msg.Split(' ');
                            int cPort=int.Parse(arg[1]);
                            //create new Thread for client, connect to ip:cPort
                            new Skeleton(relayIp, cPort);
                        }
               
                }
        }catch (IOException)
            {
                Console.WriteLine("relay disconnected");
            }
        }

        private bool conn(IPAddress ipAddress, int port)
        {
            try
            {
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
                // Create a TCP/IP  socket.
                relay = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    relay.Connect(remoteEP);
                    Console.WriteLine("Socket connected to {0}", relay.RemoteEndPoint.ToString());
                    return true;
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return false;
        }//conn
    }
}

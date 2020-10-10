using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
/// <summary>
/// Keegan Carlo Falcao 5F 29/09/2020
/// </summary>

namespace ConsoleAppClientS
{
    class Program
    {
        static Socket sender;
        static private void RicezionePacchettoDaServer(byte[] dataBytes, bool exit)
        {
            lock (new object())
            {
                string textReceived = "";
                do
                {
                    // Receive the response from the remote device.  
                    int bytesRec = sender.Receive(dataBytes);
                    textReceived = Encoding.ASCII.GetString(dataBytes, 0, bytesRec);
                    Console.WriteLine("<Server>Messaggio Ricevuto: {0}",
                        textReceived);

                } while (textReceived != "ciao" && sender.Connected);

                if (textReceived == "ciao")
                    CloseConnection(sender, ref exit);
                return;
            }
        }

        static private void CloseConnection(Socket sender, ref bool exit)
        {
            Console.WriteLine($"<Client>Termina la connessione con il Server[IPv6] {sender.RemoteEndPoint}");
            exit = true;
            // Release the socket.  
            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }
         
        static void Main(string[] args)
        {
            Console.WriteLine("Keegan Carlo Falcao 5F Autore: Keegan Carlo \n CLIENT***********");
            // Data buffer for incoming data.  
            byte[] dataBytes = new byte[1024];
            string testo = "";

            // Connect to a remote device.  
            try
            {
                // Establish the remote endpoint for the socket.  
                // This example uses port 11000 on the local computer.  
                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddress = ipHostInfo.AddressList[2];
                IPEndPoint destIP = new IPEndPoint(0, 32000);

                Console.WriteLine($"<Client>Indirizzo host: {ipAddress}");

                // Create a TCP/IP  socket.  
                sender = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);
                Thread threadClient = null;

                // Connect the socket to the remote endpoint. Catch any errors.  
                
                bool exit = false;
                sender.Connect(destIP);

                do
                {
                       
                    Console.WriteLine("<Client>Inserisci un testo");
                    testo = Console.ReadLine();
                    // Encode the data string into a byte array.  
                    byte[] msg = Encoding.ASCII.GetBytes(testo);

                    // Send the data through the socket.  
                    int bytesSent = sender.Send(msg);
                    if (testo != "ciao")
                    {
                        threadClient = new Thread(() => RicezionePacchettoDaServer(dataBytes, exit));
                        threadClient.Start();
                    }

                    if (testo == "ciao")
                    {                        
                        if (threadClient.IsAlive)
                        {
                            threadClient.Abort();
                            threadClient.Join();
                        }
                        CloseConnection(sender, ref exit);
                        break;
                    }

                }
                while (!exit);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            //Terminazione 
            Console.ReadKey();
        }
    }
}

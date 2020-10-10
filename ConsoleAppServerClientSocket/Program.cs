using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
/// <summary>
/// Keegan Carlo Falcao 5F 29/09/2020
/// </summary>

namespace ConsoleAppServerClientSocket
{
    class Program
    {
       
        static void Main(string[] args)
        {
            //Prefazione
            Console.WriteLine("Keegan Carlo Falcao 5F Autore: Keegan Carlo" +
                "\n SERVER***********");
            //stringa di dati da inviare
            string data = null;
            string readMessage = "";
            byte[] buffer = new byte[1024];
            //Il dns ottiene il nome dell'host della macchine in locale
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[2];
            //ottiene l'indirizzo ip della macchina e la sua relativa porta
            IPEndPoint pEndPoint = new IPEndPoint(0, 32000);
            //stampa dell'indirizzo locale
            Console.WriteLine($"Indirizzo host: {ipAddress}");

            //creazione socket TCP
            Socket ascoltatore = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
         
            try
            {
                //Associazione dell'endpoint al socket
                ascoltatore.Bind(pEndPoint);
                //Massimo 10 client in ascolto
                ascoltatore.Listen(10);
                //Permette l'Uscita da un ciclo (teoricamente)sempre true
                bool exit = false;
                bool superExit = false;

                while (!superExit)
                {
                    //Il socket si mette in ascolto
                    Console.WriteLine("<<Richesta Conessione>>");
                    Socket handler = ascoltatore.Accept();
                    Thread threadInvioDaServer = null;
                    //Sospensione del programma in attesa di richieste
                    while (!exit)
                    {
                        if (threadInvioDaServer != null)
                        {
                            threadInvioDaServer.Join();
                            if (readMessage == "ciao")
                                break;
                        }
                        //Riceve il messaggio con la lunghezza del dato
                        int nByteRec = handler.Receive(buffer);
                        data = Encoding.ASCII.GetString(buffer, 0, nByteRec);
                        //Stampa del messaggio ricevuto dal client
                        Console.WriteLine($"<Client> Messaggio Ricevuto: {data}");                
                        //Termina la connessione de riceve la stringa ciao
                        if (data != "ciao")
                        {
                            //Thread riguardo all'invio dei messaggi dal server
                            threadInvioDaServer = new Thread(() => PacchettoDaServer(out readMessage, handler, exit));
                            threadInvioDaServer.Start();
                        }
                        if (data == "ciao")
                        {
                            if (threadInvioDaServer != null)
                            {
                                threadInvioDaServer.Abort();
                                threadInvioDaServer.Join();
                            }
                            ServerCloseConnection(handler, ref exit);

                            break;//esce dal ciclo
                        }
                        //stringa di conferma messaggio
                        string ack = "<Server> Conferma ACK";
                        //Conferma del messaggio Ricevuto
                        byte[] sendBytes = Encoding.ASCII.GetBytes(ack);
                        //Invio Messaggio da parte del server
                        handler.Send(sendBytes);
                    }

                }

            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            //Terminazione Programma
            Console.ReadKey();


        }

        /// <summary>
        /// Invio dei messaggi da parte del server
        /// </summary>
        /// <param name="msgServ">messaggio da inviare al Client</param>
        /// <param name="handler">ottiene il socket di cui è stato precedentemente associato</param>
        /// <param name="exit">permette l'uscita dal ciclo</param>
        private static void PacchettoDaServer(out string msgServ, Socket handler, bool exit)
        {
            //legge una stringa di dati dal Server
            msgServ = Console.ReadLine();
            //Invia il messaggio al Client
            handler.Send(Encoding.ASCII.GetBytes(msgServ));
            if (msgServ == "ciao")
                ServerCloseConnection(handler, ref exit);
        }
        /// <summary>
        /// Chiude la connessione con il socket del client
        /// </summary>
        /// <param name="handler">ottiene il socket di cui è stato precedentemente associat</param>
        /// <param name="exit">permette l'uscita dal ciclo</param>
        static void ServerCloseConnection(Socket handler, ref bool exit)
        {           
            Console.WriteLine($"<Server>Termina la conessione con il Client[IPv6] {handler.RemoteEndPoint}");
            //Non permette più l'invio e la ricezione di messaggi
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
            //permette l'uscita del ciclo degli invii e delle ricezioni
            exit = true;
        }
    }
}

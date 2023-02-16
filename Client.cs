using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatApp;

public class Client
{
    private static Socket server;
    private static int bufferSize = 1024;
    private static byte[] buffer = new byte[bufferSize];
    public static void Connect()
    {
        server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        server.Connect(new IPEndPoint(IPAddress.Loopback, 1212));
        HandleFirstConnection();

        server.BeginReceive(
            buffer, 0, bufferSize, SocketFlags.None,
            new AsyncCallback(MessageReceiver), server);
        
        LoopMessageSender();
    }

    private static void HandleFirstConnection()
    {
        bool didVerified = false;
        
        while (didVerified == false)
        {
            Console.Write("[>] Digite seu nome de usuário: ");
            string username = Console.ReadLine();
            server.Send(Encoding.ASCII.GetBytes(username));
            server.Receive(buffer, 0, bufferSize, SocketFlags.None);
            
            bool validUsername = BitConverter.ToBoolean(buffer, 0);

            if (validUsername)
            {
                didVerified = true;
                Console.Title = String.Format("Cliente: {0}", username);
                Console.WriteLine("[!] Bem-vindo, {0}!", username);
            }

            else
            {
                Console.WriteLine("[!] O servidor invalidou o seu nome.");
            }
        }
    }

    private static void LoopMessageSender()
    {
        while (true)
        {
            byte[] bytesToSend = Encoding.ASCII.GetBytes(Console.ReadLine());
            server.BeginSend(bytesToSend, 0, bytesToSend.Length, SocketFlags.None, new AsyncCallback(SendCallback), server);
        }
    }

    private static void SendCallback(IAsyncResult ar)
    {
        Socket server = (Socket)ar.AsyncState;
        server.EndSend(ar);
    }

    private static void MessageReceiver(IAsyncResult ar)
    {
        int bytesRead = server.EndReceive(ar);
        Console.WriteLine(Encoding.ASCII.GetString(buffer, 0, bytesRead));
        
        server.BeginReceive(
            buffer, 0, bufferSize, SocketFlags.None,
            new AsyncCallback(MessageReceiver), server);
    }
}
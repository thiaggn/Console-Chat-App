using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatApp;

public class Server
{
    private static IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, 1212);
    private static List<ClientState> serverConnections = new();

    public static void Start()
    {
        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        serverSocket.Bind(endpoint);
        serverSocket.Listen();
        serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), serverSocket);
        
        Console.WriteLine("[!] Aguardando por conexões");
    }

    private static void AcceptCallback(IAsyncResult ar)
    {
        Socket serverSocket = (Socket)ar.AsyncState;
        Socket newConnection = serverSocket.EndAccept(ar);
        ClientState client = new ClientState(newConnection);
        serverConnections.Add(client);

        client.socket.BeginReceive(
            client.buffer, 0, ClientState.bufferSize, SocketFlags.None, 
            new AsyncCallback(HandleNewConnection), client);
        
        serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), serverSocket);
        
        Console.WriteLine("[!] Solicitação de conexão recebida");
    }
    
    private static void HandleNewConnection(IAsyncResult ar)
    {
        ClientState client = (ClientState)ar.AsyncState;
        int usernameLength = client.socket.EndReceive(ar);

        if (usernameLength > 2 && usernameLength < 16)
        {
            string username = Encoding.ASCII.GetString(client.buffer, 0, usernameLength);
            client.username = username;

            client.socket.BeginSend(
                BitConverter.GetBytes(true),0, 1, 
                SocketFlags.None, new AsyncCallback(SendCallback), client);
            
            client.socket.BeginReceive(
                client.buffer, 0, ClientState.bufferSize, SocketFlags.None, 
                new AsyncCallback(HandleIncomingMessages), client);
            
            Console.WriteLine("[!] {0} conectou-se ao servidor", username);
        }
        
        else
        {
            client.socket.BeginSend(
                BitConverter.GetBytes(false),0, 1, 
                SocketFlags.None, new AsyncCallback(SendCallback), client);
            
            client.socket.BeginReceive(
                client.buffer, 0, ClientState.bufferSize, SocketFlags.None, 
                new AsyncCallback(HandleNewConnection), client);
        }
    }

    private static void HandleIncomingMessages(IAsyncResult ar)
    {
        ClientState currentClient = (ClientState)ar.AsyncState;
        int messageLength = currentClient.socket.EndReceive(ar);
        string message = "[" + currentClient.username + "]: " +
                         Encoding.ASCII.GetString(currentClient.buffer, 0, messageLength);
        byte[] messageBytes = Encoding.ASCII.GetBytes(message);
        
        foreach (ClientState connection in serverConnections)
        {
            if (!connection.Equals(currentClient))
            {
                connection.socket.BeginSend(
                    messageBytes, 0, messageBytes.Length, 
                    SocketFlags.None, new AsyncCallback(SendCallback), currentClient);
            };
        }
        
        currentClient.socket.BeginReceive(
            currentClient.buffer, 0, ClientState.bufferSize, SocketFlags.None, 
            new AsyncCallback(HandleIncomingMessages), currentClient);
    }
    
    private static void SendCallback(IAsyncResult ar)
    {
        ClientState client = (ClientState) ar.AsyncState;
        client.socket.EndSend(ar);
    }
}
public class ClientState
{
    public Socket socket;
    public const int bufferSize = 1024;
    public byte[] buffer = new byte[bufferSize];
    public string username;

    public ClientState(Socket connection)
    {
        this.socket = connection;
    }
}


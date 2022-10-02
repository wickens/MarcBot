using System.Text;
using WatsonWebsocket;

namespace MarcBot;



class Program
{
    private static WatsonWsServer? server;
    private static Dictionary<int, string> clients;

    static void Main(string[] args)
    {

        Console.WriteLine("Welcome to MarcBot Server");
        int port = 8001;
        server = new WatsonWsServer("localhost", port, false);
        server.ClientConnected += ClientConnected;
        server.ClientDisconnected += ClientDisconnected;
        server.MessageReceived += MessageReceived;

        while (true)
        {

            StartServer(port);
            string? userInput = Console.ReadLine()?.Trim();
            if (userInput == null) { userInput = String.Empty; };


            if (userInput.StartsWith("/"))
            {
                SendMessage(userInput);
            }

            //server.SendAsync(clients[1], "this is a message I love messages!");
        }

    }

    private static async void SendMessage(string userInput)
    {
        int clientId = -1;
        string message = String.Empty;

        // Extract the message and client
        string[] parts = userInput.Split(' ');
        if (parts.Length > 1)
        {
            clientId = Convert.ToInt32(parts[0].TrimStart('/'));
            string toTrim = String.Format("/{0} ", clientId);
            message = userInput.Substring(toTrim.Length, userInput.Length - toTrim.Length);
        }

        if(!clients.ContainsKey(clientId))
        {
            Console.WriteLine("Cannot find client with ID '{0}'", clientId);
        }

        if (String.IsNullOrEmpty(message))
        {
            Console.WriteLine("Message cannot be empty");
        }


        await server.SendAsync(clients[clientId], message);


    }

    private static void StartServer(int port)
    {
        try
        {
            if (!server.IsListening)
            {
                server.Start();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("*** Server Started on port {0} ***", port);
                Console.ResetColor();
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("Error: ", ex.Message);
            // Quit the app
            Environment.Exit(-1);
        }
    }

    private static void MessageReceived(object? sender, MessageReceivedEventArgs e)
    {
        string message = Encoding.UTF8.GetString(e.Data);
        Console.WriteLine("*** Message received *** : {0}", message);
    }

    private static void ClientDisconnected(object? sender, ClientDisconnectedEventArgs e)
    {
        int clientId = RemoveClient(e.IpPort);
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("*** Client Disconnected ***: {0}", clientId);
        Console.ResetColor();
    }

    private static void ClientConnected(object? sender, ClientConnectedEventArgs e)
    {
        int clientId = AddClient(e.IpPort);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("*** Client Connected ***: {0}", clientId);
        Console.ResetColor();
    }


    private static int AddClient(string IpPort)
    {
        if (clients == null)
        {
            clients = new Dictionary<int, string>();
        }

        int clientId = clients.Count + 1;
        if (clients.ContainsKey(clientId))
        {
            clients.Remove(clientId);
        }
        clients.Add(clientId, IpPort);
        return clientId;
    }

    private static int RemoveClient(string ip)
    {
        int idToRemove = -1;

        foreach (KeyValuePair<int, string> kvp in clients)
        {
            if (kvp.Value.Equals(ip))
            {
                idToRemove = kvp.Key;
                break;
            }
        }

        if (idToRemove >= 0)
        {
            clients.Remove(idToRemove);
        }


        return idToRemove;


    }

    private static int GetClientIdFromIp(string ip)
    {
        int result = -1;

        foreach (KeyValuePair<int, string> kvp in clients)
        {
            if (kvp.Value.Equals(ip))
            {
                result = kvp.Key;
                break;
            }
        }


        return result;
    }


}


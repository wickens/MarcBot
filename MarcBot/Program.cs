using System.Globalization;
using System.Text;
using System.Text.Json;
using CommandLine;
using CommandLine.Text;
using WatsonWebsocket;

namespace MarcBot;



internal class Program
{
    public class Options
    {
        [Option('b', "binding", Required = false, HelpText = "The server address (can be a IP or DNS name). Defualt is localhost.")]
        public string? ServerAddress { get; set; }

        [Option('p', "port", Required = false, HelpText = "The server port. Default 8001")]
        public string? ServerPort { get; set; }


    }

    private static WatsonWsServer? server;
    private static Dictionary<int, string> clients;


    static void Main(string[] args)
    {
        int port = 8001;
        string binding = "localhost";

        // parse the commandline args
        Parser.Default.ParseArguments<Options>(args)
                 .WithParsed<Options>(o =>
                 {
                    if(!String.IsNullOrEmpty(o.ServerAddress))
                     {
                         binding = o.ServerAddress;
                     }

                     if (o.ServerPort != null)
                     {
                         port = Convert.ToInt32(o.ServerPort);
                     }
                    


                 });

        Console.WriteLine("Welcome to MarcBot Server");
        Console.WriteLine();
        Console.WriteLine(@"Usage: Type /[ID] [MSG] to send a message. (where '[ID]' is the numeric ID of the client computer, and '[MSG]' is text you want to send. You can include HTML.");
        Console.WriteLine();
        Console.WriteLine("To set the typing indicator, send a message that says 'typing'.");


        server = new WatsonWsServer(binding, port, false);
        server.ClientConnected += ClientConnected;
        server.ClientDisconnected += ClientDisconnected;
        server.MessageReceived += MessageReceived;

        while (true)
        {

            StartServer(binding, port);
            string? userInput = Console.ReadLine()?.Trim();
            if (userInput == null) { userInput = String.Empty; };


            if (userInput.StartsWith("/"))
            {
                SendMessage(userInput);
            }

 
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
            try
            {
                clientId = Convert.ToInt32(parts[0].TrimStart('/'));
                string toTrim = String.Format("/{0} ", clientId);
                message = userInput.Substring(toTrim.Length, userInput.Length - toTrim.Length);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Invalid command: " + ex.Message);
            }
        }

        if(!clients.ContainsKey(clientId))
        {
            Console.WriteLine("Cannot find client with ID '{0}'", clientId);
            return;
        }

        if (String.IsNullOrEmpty(message))
        {
            Console.WriteLine("Message cannot be empty");
            return;
        }


        await server.SendAsync(clients[clientId], message);


    }

    private static void StartServer(string binding, int port)
    {
        try
        {
            if (!server.IsListening)
            {
                server.Start();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("*** Server Listening on {0}:{1} ***", binding, port);
                Console.ResetColor();
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("Error: {0} ", ex.Message);
            // Quit the app
            Environment.Exit(-1);
        }
    }

    private static void MessageReceived(object? sender, MessageReceivedEventArgs e)
    {
        string messageRaw = Encoding.UTF8.GetString(e.Data);
        var webMessage = JsonSerializer.Deserialize<WebMessage>(JsonDocument.Parse(messageRaw));
        var clientId = GetClientIdFromIp(e.IpPort);
        int id = GetClientIdFromIp(e.IpPort);

        if (webMessage.message.Length > 0)
        {
            Console.WriteLine("{1}({0}): {2}", id, webMessage.name, webMessage.message);
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("*** Client {0} set their name to  '{1}' ***: {0}", clientId, webMessage.name);
            Console.ResetColor();
        }
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


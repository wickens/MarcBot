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

        [Option('a', "auto", Required = false, HelpText = "Set to true to have the bot auto respond")]
        public bool? Autorespond { get; set; }

        [Option('u', "url", Required = false, HelpText = "Supply a URL of a CSV to respond with automatically")]
        public string? ConverationURL { get; set; }

    }

    /// <summary>
    /// the main web socket server
    /// </summary>
    private static WatsonWsServer? server;

    private static ClientManager clientManager;

    private static bool automode = false;
    private static bool respondWithCsv = false;


    static void Main(string[] args)
    {
        clientManager = new ClientManager();


        int port = 8001;
        string binding = "localhost";
        string csvUrl = String.Empty;


        // parse the commandline args
        Parser.Default.ParseArguments<Options>(args)
                 .WithParsed<Options>(o =>
                 {
                     if (!String.IsNullOrEmpty(o.ServerAddress))
                     {
                         binding = o.ServerAddress;
                     }

                     if (o.ServerPort != null)
                     {
                         port = Convert.ToInt32(o.ServerPort);
                     }

                     automode = o.Autorespond.GetValueOrDefault(false);

                     // The user supplied a CSV to respond with
                     if (!automode && !String.IsNullOrEmpty(o.ConverationURL))
                     {
                         respondWithCsv = true;
                         csvUrl = o.ConverationURL;

                     }



                 });

        Console.WriteLine("Welcome to MarcBot Server v1.1");
        Console.WriteLine();
        Console.WriteLine();
        if (respondWithCsv)
        {
            AutomaticResponseManager.LoadResponses(csvUrl);
        }
        else
        {

            Console.WriteLine(@"Usage: Type /[ID] [MSG] to send a message. (where '[ID]' is the numeric ID of the client computer, and '[MSG]' is text you want to send. To set the typing indicator, send a message that says 'typing'.");
        }

        Console.WriteLine();



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
                ProcessInput(userInput);
            }


        }

    }

    private static async void ProcessInput(string userInput)
    {
        if (!respondWithCsv)
        {
            int clientId = -1;
            string message = String.Empty;

            // Extract the message and client
            ExtractClientIDAndMessageFromUserInput(userInput, ref clientId, ref message);
            ConnectedClient client = clientManager.GetById(clientId);

            if (client.Id == -1)
            {
                Console.WriteLine("Cannot find client with ID '{0}'", clientId);
                return;
            }


            if (String.IsNullOrEmpty(message))
            {
                Console.WriteLine("Message cannot be empty");
                return;
            }

            await SendMessageToClient(message, client.Ip);
        }
        else
        {
            Console.WriteLine("Cannot send messages when a CSV is being used to respond with.");
        }

    }

    private static async Task SendMessageToClient(string message, string ip)
    {
        await server.SendAsync(ip, message);
    }

    private static void ExtractClientIDAndMessageFromUserInput(string userInput, ref int clientId, ref string message)
    {
        string[] parts = userInput.Split(' ');
        if (parts.Length > 1)
        {
            try
            {
                clientId = Convert.ToInt32(parts[0].TrimStart('/'));
                string toTrim = String.Format("/{0} ", clientId);
                message = userInput.Substring(toTrim.Length, userInput.Length - toTrim.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Invalid command: " + ex.Message);
            }
        }
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

    private static async void MessageReceived(object? sender, MessageReceivedEventArgs e)
    {
        string messageRaw = Encoding.UTF8.GetString(e.Data);
        WebMessage webMessage = JsonSerializer.Deserialize<WebMessage>(JsonDocument.Parse(messageRaw));

        ConnectedClient client = clientManager.GetByIp(e.IpPort);

        if (webMessage != null)
        {
            if (webMessage.message.Length > 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;

                Console.WriteLine("{3} {1}({0}): {2}", client.Id, webMessage.name, webMessage.message, DateTime.Now);
                Console.ResetColor();


                if (automode)
                {
                    string returnMsg = String.Format("You said '{0}'.", webMessage.message);
                    Console.WriteLine("Responded to {0} with '{1}'.", client, returnMsg);
                    await SendMessageToClient(returnMsg, e.IpPort);
                }
                else if(respondWithCsv)
                {
                    var plannedResponses = AutomaticResponseManager.GetResponserGivenInput(webMessage.message, false, client.userInputExpecteds);
                    foreach (string response in plannedResponses)
                    {
                        await SendMessageToClient(response, e.IpPort);
                        Console.WriteLine("Responded to {0} with '{1}'.", client, response);
                    }
                }


            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("*** Client {0} set their name to  '{1}'.", client, webMessage.name);
                client.Name = webMessage.name;
                Console.ResetColor();

                if (automode)
                {
                    string returnMsg = String.Format("Hello, {0}!", client.Name);
                    Console.WriteLine("Responded to {0} with '{1}'.", client, returnMsg);
                    await SendMessageToClient(returnMsg, e.IpPort);
                }
                else if (respondWithCsv)
                {
                    var plannedResponses = AutomaticResponseManager.GetResponserGivenInput(String.Empty, true, client.userInputExpecteds);
                    foreach(string response in plannedResponses)
                    {
                        await SendMessageToClient(response, e.IpPort);
                        Console.WriteLine("Responded to {0} with '{1}'.", client, response);
                    }

                }

            }
        }
    }

    private static void ClientDisconnected(object? sender, ClientDisconnectedEventArgs e)
    {
        ConnectedClient client = clientManager.GetByIp(e.IpPort);
        clientManager.Remove(e.IpPort);

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("*** Client Disconnected ***: {0}", client);
        Console.ResetColor();
    }

    private static void ClientConnected(object? sender, ClientConnectedEventArgs e)
    {
        clientManager.Add(new ConnectedClient(e.IpPort));
        ConnectedClient client = clientManager.GetByIp(e.IpPort);

        // each client has their own copy of the responses so we can mark each one as "sent" indivudually 
        if (respondWithCsv)
        {
            client.userInputExpecteds = new List<UserInputExpected>(AutomaticResponseManager.Results);
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("*** Client Connected ***: {0}", client);
        Console.ResetColor();

    }






}


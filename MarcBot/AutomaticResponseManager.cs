using System;
using CsvHelper;
using System.Globalization;
using System.Net;


namespace MarcBot
{
    public static class AutomaticResponseManager
    {
        private static List<UserInputExpected> results = new List<UserInputExpected>();
        const string START_ACTION = "[START]";

        public static List<UserInputExpected> Results { get => results; set => results = value; }


        /// <summary>
        /// get a list of bot messages to respond with
        /// </summary>
        /// <param name="actualUserInput"></param>
        /// <param name="firstInput"></param>
        /// <param name="userMessages"></param>
        /// <returns></returns>
        public static string[] GetResponserGivenInput(string actualUserInput, bool firstInput, List<UserInputExpected> userMessages)
        {
            List<string> results = new List<string>();

            UserInputExpected? response = null;
            foreach (var usrMsg in userMessages)
            {

                if (firstInput && usrMsg.IsFirstUserAction)
                {
                    results.AddRange(usrMsg.BotResponses);
                    response = usrMsg;
                    break;
                }
                else if (usrMsg.ShouldRespond(actualUserInput))
                {
                   
                    results.AddRange(usrMsg.BotResponses);
                    response = usrMsg;
                    break;
                }

            }

            if (response != null)
            {
                userMessages.Remove((UserInputExpected)response);
            }


            return results.ToArray();
        }

        public static void LoadResponses(string csvUrl)
        {

            AutomaticResponseManager.Results = new List<UserInputExpected>();

            // test file
            // https://marc.wickens.org.uk/test.csv
            // /Users/marc/Projects/BasicConversationParser/test.csv

            string url = string.Empty;
            using WebClient client = new WebClient();

            // Add a user agent header in case the
            // requested URI contains a query.

            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

            using Stream data = client.OpenRead(csvUrl);
            using StreamReader reader = new StreamReader(data);


            //using (var reader = new StreamReader("/Users/marc/Projects/BasicConversationParser/test.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<RawCsv>();
                List<RawCsv> rows = new List<RawCsv>(records);


                foreach (RawCsv row in rows)
                {
                    // Check the user message
                    if (!String.IsNullOrEmpty(row.User))
                    {
                        UserInputExpected action = new UserInputExpected();
                        Results.Add(action);

                        // The first user action is a special case as there is no input message
                        if (row.User.Equals(START_ACTION))
                        {
                            action.IsFirstUserAction = true;
                        }
                        else
                        {
                            action.UserInputAsExpected = row.User;
                        }

                    }

                    if (!String.IsNullOrEmpty(row.Bot))
                    {

                        // The result will be the previous action  
                        UserInputExpected action = Results[Results.Count - 1];
                        action.AddBotResponse(row.Bot);
                    }


                }


            }

            Console.WriteLine("Successfully loaded the following automatic conversation from {0}:", csvUrl);
            // Print output:
            foreach (var result in Results)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                if (result.IsFirstUserAction)
                {
                    Console.WriteLine("User: <Open Bot>");
                }
                else
                {
                    Console.WriteLine("User Says: {0}", result.UserInputAsExpected);
                }

                foreach (string response in result.BotResponses)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Bot Says: {0}", response);

                }
            }



        }

    }



}


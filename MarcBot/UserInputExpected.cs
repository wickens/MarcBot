using System;
namespace MarcBot
{

    public class UserInputExpected
    {
        /// <summary>
        /// True if this is the first user action (e.g. not a message, but opening the chatbot)
        /// </summary>
        public bool IsFirstUserAction = false;

        /// <summary>
        /// True if we've responded to this user action within the conversation and so shouldn't do so again (even if there's a match)
        /// </summary>
        //public bool IsUsed = false;


        public UserInputExpected()
        {
            this.UserInputAsExpected = String.Empty;
            this.BotResponses = new List<string>().ToArray();
        }

        /// <summary>
        ///  The expected user input
        /// </summary>
        public string UserInputAsExpected { get; set; }

        /// <summary>
        /// A list of responses that should be sent back when the user input matched (see ShouldRespond)
        /// </summary>
        public string[] BotResponses { get; set; }

        public void AddBotResponse(string response)
        {
            List<string> list = new List<string>(this.BotResponses);
            list.Add(response);
            this.BotResponses = list.ToArray();
        }

        /// <summary>
        /// Returns true if the bot should respond the bot responses
        /// </summary>
        /// <param name="actualUserInput"></param>
        /// <returns></returns>
        public bool ShouldRespond(string actualUserInput)
        {
            // Respond with true if the input is what we expect and we have something to respond with
            if (actualUserInput.Trim().Equals(this.UserInputAsExpected.Trim(), StringComparison.InvariantCultureIgnoreCase) && this.BotResponses.Length > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


    }


}


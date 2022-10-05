using System;
namespace MarcBot
{
	public class ClientManager
	{
		private int clientCounter = 0;
		private ConnectedClient nullClient;
		public Dictionary<string, ConnectedClient> clientList = new Dictionary<string, ConnectedClient>();
		

		/// <summary>
		/// Class for managing which clients are connected
		/// </summary>
        public ClientManager()
		{
			this.nullClient = new ConnectedClient(String.Empty, -1);
			nullClient.Name = String.Empty;
		}


		/// <summary>
		/// Add a new client and set its ID
		/// </summary>
		/// <param name="client"></param>
        public new void Add(ConnectedClient client)
		{
			lock (nullClient)
			{
				if (!clientList.ContainsKey(client.Ip))
				{
					clientCounter++;
					client.Id = clientCounter;
					clientList.Add(client.Ip, client);
				}
			}
		}


		/// <summary>
		/// Remove a client
		/// </summary>
		/// <param name="ip"></param>
		public void Remove(string ip)
		{
			lock (nullClient)
			{
				if (clientList.ContainsKey(ip))
				{
					clientList.Remove(ip);
				}
			}
		}


		/// <summary>
		/// Get client by the friendly ID
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public ConnectedClient GetById(int id)
		{
			foreach(KeyValuePair<string, ConnectedClient> kvp in this.clientList)
			{
				if(id == kvp.Value.Id)
				{
					return kvp.Value;
				}
			}

            return nullClient;

        }

		/// <summary>
		/// Get a client by it's unique IP address
		/// </summary>
		/// <param name="ip"></param>
		/// <returns></returns>
        public ConnectedClient GetByIp(string ip)
        {
			if(this.clientList.ContainsKey(ip))
			{
				return this.clientList[ip];
			}
			else
			{
				return nullClient;
			}
        }


		/// <summary>
		/// Get a list of all connected clients
		/// </summary>
		/// <returns></returns>
		public ConnectedClient[] ToArray()
		{
			List<ConnectedClient> result = new List<ConnectedClient>();
            foreach (KeyValuePair<string, ConnectedClient> kvp in this.clientList)
            {
				result.Add(kvp.Value);
            }

			return result.ToArray();
        }


    }
}


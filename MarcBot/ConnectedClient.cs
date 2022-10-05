using System;
namespace MarcBot
{
	public class ConnectedClient
	{
		public string Name { get; set; }
        public string Ip { get; set; }
        public int Id { get; set; }

        public ConnectedClient(string ip, int id)
		{
			this.Ip = ip;
			this.Id = id;
			this.Name = "[NAME NOT SET]";
		}


        public ConnectedClient(string ip)
        {
            this.Ip = ip;
            this.Id = -1;
            this.Name = "[NAME NOT SET]";
        }

        public override string ToString()
        {
            return string.Format("{0} / {1} / {2}", this.Id, this.Ip, this.Name);
        }

    }
}


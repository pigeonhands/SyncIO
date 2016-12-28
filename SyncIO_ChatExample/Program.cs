using SyncIO.Client;
using SyncIO.Network;
using SyncIO.Server;
using SyncIO.Transport;
using SyncIO.Transport.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncIO_ChatExample {
    class Program {
        static void Main(string[] args) {

            Console.WriteLine("SyncIO Chat example (Local computer only).");
            int opt = ConsoleExtentions.GetDesision(new string[] { "Client", "Server" });
            Console.Clear();
            if (opt == 1)
                Server();
            else
                Client();
        }


        private static void Client() {
            var packer = new Packager(new Type[] {
                typeof(SetName),
                typeof(ChatMessage)
            });

            var client = new SyncIOClient(TransportProtocal.IPv4, packer);

            client.SetHandler<ChatMessage>((SyncIOClient sender, ChatMessage messagePacket) => {
                Console.WriteLine(messagePacket.Message);
            });

            if (!client.Connect("127.0.0.1", 9999))
                ConsoleExtentions.ErrorAndClose("Failed to connect to server.");

            var name = ConsoleExtentions.GetNonEmptyString("Enter a name: ");
            client.Send(new SetName(name));

            Console.WriteLine("Connected. You may now send messages.");

            bool connected = true;
            client.OnDisconnect += (s, err) => {
                connected = false;
            };

            while (connected) {
                var msg = ConsoleExtentions.GetNonEmptyString("");
                if (connected)
                    client.Send(new ChatMessage(msg));
            }
            ConsoleExtentions.ErrorAndClose("Lost connection to server");
        }


        private static void Server() {
            var packer = new Packager(new Type[] {
                typeof(SetName),
                typeof(ChatMessage)
            });

            var server = new SyncIOServer(TransportProtocal.IPv4, packer);

            var clients = new Dictionary<Guid, ConnectedChatClient>();

            var sendToAll = new Action<IPacket>((p) => {
                foreach (var c in clients.Select(x => x.Value))
                    c.Connection.Send(p);
            });

            server.OnClientConnect += (SyncIOServer sender, SyncIOConnectedClient client) => {
                Console.WriteLine("{0}] New connection", client.ID);
            };

            server.SetHandler<SetName>((c, p) => {
                sendToAll(new ChatMessage($"{p.Name} connected."));
                clients.Add(c.ID, new ConnectedChatClient(c, p.Name));
            });

            server.SetHandler<ChatMessage>((c, p) => {
                var msg = $"<{clients[c.ID].Name}> {p.Message}";
                sendToAll(new ChatMessage(msg));
                Console.WriteLine(msg);
             });

            var sock = server.ListenTCP(9999);
            if(sock == null)
                ConsoleExtentions.ErrorAndClose("Failed to listen on port 9999");

            Console.WriteLine("Listening on {0}", sock);
            while (true)
                Console.ReadLine();
        }

        class ConnectedChatClient {
            public SyncIOConnectedClient Connection { get; }
            public string Name { get; }
            public ConnectedChatClient(SyncIOConnectedClient _conn, string _name) {
                Connection = _conn;
                Name = _name;
            }
        }

        [Serializable]
        class SetName : IPacket {
            public string Name { get; set; }
            public SetName(string _name) {
                Name = _name;
            }
        }

        [Serializable]
        class ChatMessage : IPacket {
            public string Message { get; set; }
            public ChatMessage(string _msg) {
                Message = _msg;
            }
        }
    }
}

using SyncIO.Client;
using SyncIO.Network;
using SyncIO.Server;
using SyncIO.Transport;
using SyncIO.Transport.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

            //When sending custom objects, you must create a new packer and specify them.
            var packer = new Packager(new Type[] {
                typeof(SetName),
                typeof(ChatMessage)
            });

            //Using ipv4 and the packer that has the custom types.
            var client = new SyncIOClient(TransportProtocal.IPv4, packer); 

            //The diffrent types of handlers: (all optional)

            //The type handler.
            //This handler handles a specific object type.
            client.SetHandler<ChatMessage>((SyncIOClient sender, ChatMessage messagePacket) => {
                //All ChatMessage packages will be passed to this callback
                Console.WriteLine(messagePacket.Message);
            });

            //This handler handles any IPacket that does not have a handler.
            client.SetHandler((SyncIOClient sender, IPacket p) => {
                //Any packets without a set handler will be passed here
            });

            //This handler handles anything that is not a SINGLE IPacket object
            client.SetHandler((SyncIOClient sender, object[] data) => {
                //Any object array sent will be passed here, even if the array contains
                //A packet with a handler (e.g. ChatMessage)
            });



            if (!client.Connect("127.0.0.1", new Random().Next(9996, 10000)))//Connect to any of the open ports.
                ConsoleExtentions.ErrorAndClose("Failed to connect to server.");

            //Connecting and handshake are not the same.
            //Connecting = Establishing a connection with a socket
            //Handskake  = Establishing a connection with a SyncIOServer and getting an ID.
            Console.WriteLine("Connected on port {0}. Waiting for handshake.", client.EndPoint.Port);

            bool success = client.WaitForHandshake();
            /*
             * The asynchronous way to get handshake would be subscribing
             * to the client.OnHandshake event.
             */

            if (!success)
                ConsoleExtentions.ErrorAndClose("Handshake failed.");

            Console.WriteLine("Handshake success. Got ID: {0}", client.ID);

            var name = ConsoleExtentions.GetNonEmptyString("Enter a name: ");
            client.Send(new SetName(name));

            Console.WriteLine("Name set. You can now send messages.");

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
                //Send to all clients who have set a name.
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

            Console.WriteLine("Closing socket examples:");
            //Listen on all of the following ports:
            var firstSock = server.ListenTCP(9996); //Add it to a variable for closing example
            server.ListenTCP(9997);
            server.ListenTCP(9998);
            server.ListenTCP(9999);

            if(server.Count() < 1)
                ConsoleExtentions.ErrorAndClose("Failed to listen on any ports.");

            foreach (var sock in server) {
                Console.WriteLine("Listening on {0}", sock.EndPoint.Port);
                sock.OnDisconnect += (sender, err) => {
                    Console.WriteLine("{0}] Socket closed. {1}", sender.EndPoint.Port, err);
                };
            }

            Console.WriteLine("Closing port {0} and 9998", firstSock.EndPoint.Port);
            firstSock.Dispose();    //Either close from var
            server[9997].Dispose(); //Or by server index.

            foreach (var sock in server) 
                Console.WriteLine("Listening on {0}", sock.EndPoint.Port);

            Console.WriteLine("Reopening ports in 3 seconds");
            Thread.Sleep(3000);
            Console.Clear();

            //Reopen:
            server.ListenTCP(9996);
            server.ListenTCP(9997);

            foreach (var sock in server)
                Console.WriteLine("Listening on {0}", sock.EndPoint.Port);


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

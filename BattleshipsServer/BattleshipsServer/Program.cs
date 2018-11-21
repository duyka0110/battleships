using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace BattleshipsServer {

	public class Server {

		/// Fields
		const string ACK = "1";
		const int PORT_NO = 8888;
		const int BUFFER_SIZE = 4096;
		const int MAX_RECEIVE_ATTEMPT = 10;
		const int NUM_OF_CLIENTS = 2;
		static int receiveAttempt = 0;
		static byte[] buffer = new byte[BUFFER_SIZE];
		static List<Socket> clientSockets = new List<Socket>();
		static string SERVER_IP;
		static Socket serverSocket;
		
		/// Methods
		// Get Local IP
		static string GetLocalIP() {

			var host = Dns.GetHostEntry(Dns.GetHostName());
			foreach (var ip in host.AddressList) {
				if (ip.AddressFamily == AddressFamily.InterNetwork)
					return ip.ToString();
			}
			throw new Exception("NO network adapters with an IPv4 address in the system!");

		}

		// Called when accept a client
		static void acceptCallback(IAsyncResult result) {

			Socket socket = null;
			try {
				socket = serverSocket.EndAccept(result);
				clientSockets.Add(socket);
				Console.WriteLine("Connected to client, IP: {0}", socket.LocalEndPoint.ToString());
				//* Server sends ACK
				socket.Send(Encoding.ASCII.GetBytes(ACK));
				Console.WriteLine("Sent message {0} to client", ACK);
				// Start receiving data
				socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(receiveCallback), socket);
				// Accepting new client
				serverSocket.BeginAccept(new AsyncCallback(acceptCallback), null);
			}
			catch (Exception e) {
				Console.WriteLine(e.ToString());
			}

		}

		// Called when receive data from a client
		static void receiveCallback(IAsyncResult result) {

			Socket socket = null;
			try {
				// Get the sender
				socket = (Socket)result.AsyncState;
				if (socket.Connected) {
					int received = socket.EndReceive(result);
					if (received > 0) {
						// Get data received from 'buffer' to 'data'
						byte[] data = new byte[received];
						Buffer.BlockCopy(buffer, 0, data, 0, data.Length);
						//
						Console.WriteLine(Encoding.ASCII.GetString(data));
						
						receiveAttempt = 0; // reset receive attempts
																// Begin receiving from that client again
						socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(receiveCallback), socket);
					}
					else if (receiveAttempt < MAX_RECEIVE_ATTEMPT) {
						++receiveAttempt;
						socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(receiveCallback), socket);
					}
					else {
						Console.WriteLine("receiveCallback fails");
						receiveAttempt = 0;
					}
				}
			}
			catch (Exception e) {
				Console.WriteLine("receiveCallback fails with exception: {0}", e.ToString());
			}

		}


		static void Main(string[] args) {

			// Listening at SERVER_IP and PORT_NO
			SERVER_IP = GetLocalIP();
			Console.WriteLine("Server started at IP: {0}, Port: {1}", SERVER_IP, PORT_NO);
			serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			serverSocket.Bind(new IPEndPoint(IPAddress.Parse(SERVER_IP), PORT_NO));
			serverSocket.Listen(NUM_OF_CLIENTS); // change to 2 when ready to play game
			Console.WriteLine("Listening...");

			// Begin accepting clients, Asyncly
			serverSocket.BeginAccept(new AsyncCallback(acceptCallback), null);

			Console.Read();
		}

	}

}

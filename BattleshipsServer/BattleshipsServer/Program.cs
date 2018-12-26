using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace BattleshipsServer {

    /// Main 
	public class Server {

		/// Fields
		const string ACK = "1";											// ACK message
		const int PORT_NO = 8888;										// Port number
		const int BUFFER_SIZE = 4096;									// Receiving data's size
		const int MAX_RECEIVE_ATTEMPT = 10;								// Max receive attempt
		const int NUM_OF_CLIENTS = 2;									// Max number of clients
		static int receiveAttempt = 0;				
		static byte[] buffer = new byte[BUFFER_SIZE];					// Store received data
		static List<Socket> clientSockets = new List<Socket>();			// Store list of client socket, to transmit data
		static string SERVER_IP;										// Server IP
		static Socket serverSocket;										// Server socket, to establish connection only
		static bool flag1time;											// Used to signify 1 time transmitting data only
		static AutoResetEvent eventConnect1 = new AutoResetEvent(false);        // To signal and wait, connecting specific
		static AutoResetEvent eventConnect2 = new AutoResetEvent(false);        // To signal and wait, connecting specific
		static AutoResetEvent eventReceive1 = new AutoResetEvent(false);     // To signal and wait, receiving specific, client1
		static AutoResetEvent eventReceive2 = new AutoResetEvent(false);     // To signal and wait, receiving specific, client2
		static AutoResetEvent eventSend1 = new AutoResetEvent(false);        // To signal and wait, sending specific, client1
		static AutoResetEvent eventSend2 = new AutoResetEvent(false);        // To signal and wait, sending specific, client1

		// Game logic
		public static Battleships battleships;

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
				// Block third connection attempts
				if (clientSockets.Count == NUM_OF_CLIENTS) {
					Console.WriteLine("Maximum clients reached");
					return;
				}
				clientSockets.Add(socket);
				Console.WriteLine("Connected to client {0}, IP: {1}", clientSockets.IndexOf(socket), socket.LocalEndPoint.ToString());
				// Server sends ACK no need bcuz client can recognize connection
				//socket.Send(Encoding.ASCII.GetBytes(ACK));
				Console.WriteLine("Sent message {0} to client {1}", ACK, clientSockets.IndexOf(socket));
				// Start receiving data
				socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(receiveCallback), socket);
				// Accepting new client
				serverSocket.BeginAccept(new AsyncCallback(acceptCallback), null);
				// To avoid sending multiple data at the same time
				if (clientSockets.Count == NUM_OF_CLIENTS) {
					eventConnect1.Set();
					eventConnect2.Set();
				}
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
						string msgFromClient = Encoding.ASCII.GetString(data);
						string msgACK;
						byte[] msgACKByte;
						switch (msgFromClient.Substring(0, 1)) {
							// 0 + 2/2 confirmed
							case "0":
								if (msgFromClient.Substring(1, 1) == "1") {
									if (socket == clientSockets[0])
										eventReceive1.Set();
									else if (socket == clientSockets[1])
										eventReceive2.Set();
								}
								break;
							// 1 + dimension
							case "1":
								if (msgFromClient.Substring(1, 1) == "1") {
									if (socket == clientSockets[0])
										eventReceive1.Set();
									else if (socket == clientSockets[1])
										eventReceive2.Set();
								}
								break;
							// 2 + NumShips
							case "2":
								if (msgFromClient.Substring(1, 1) == "1") {
									if (socket == clientSockets[0])
										eventReceive1.Set();
									else if (socket == clientSockets[1])
										eventReceive2.Set();
								}
								break;
							// 3 + Shipkinds
							case "3":
								if (msgFromClient.Substring(1, 1) == "1") {
									if (socket == clientSockets[0])
										eventReceive1.Set();
									else if (socket == clientSockets[1])
										eventReceive2.Set();
								}
								break;
							// 4 + currShipLength
							case "4":
								if (msgFromClient.Substring(1, 1) == "1") {
									if (socket == clientSockets[0])
										eventReceive1.Set();
									else if (socket == clientSockets[1])
										eventReceive2.Set();
								}
								break;
							// 5 + ships positions + position value
							case "5":
								string strIndexes = msgFromClient.Substring(1, msgFromClient.Length - 1);
								// client1
								if (socket == clientSockets[0]) {
									for (int i = 0; i < strIndexes.Length - 2; i += 3) {
										string strMatrixIndex = strIndexes.Substring(i, 3);
										int indexI = Convert.ToInt32(strMatrixIndex.Substring(0, 1));
										int indexJ = Convert.ToInt32(strMatrixIndex.Substring(1, 1));
										int indexValue = Convert.ToInt32(strMatrixIndex.Substring(2, 1));
										battleships.matrix1[indexI, indexJ] = indexValue;
									}
								}
								// client2
								if (socket == clientSockets[1]) {
									for (int i = 0; i < strIndexes.Length - 2; i += 3) {
										string strMatrixIndex = strIndexes.Substring(i, 3);
										int indexI = Convert.ToInt32(strMatrixIndex.Substring(0, 1));
										int indexJ = Convert.ToInt32(strMatrixIndex.Substring(1, 1));
										int indexValue = Convert.ToInt32(strMatrixIndex.Substring(2, 1));
										battleships.matrix2[indexI, indexJ] = indexValue;
									}
								}
								// Send ACK back to client
								msgACK = "51";
								msgACKByte = Encoding.ASCII.GetBytes(msgACK);
								socket.Send(msgACKByte);
								break;
							// 6 + hitPoints
							case "6":
								if (socket == clientSockets[0])
									battleships.hitPoints1 = Convert.ToInt32(msgFromClient.Substring(1, msgFromClient.Length - 1));
								else if (socket == clientSockets[1]) 
									battleships.hitPoints2 = Convert.ToInt32(msgFromClient.Substring(1, msgFromClient.Length - 1));
								// Send ACK back to client
								msgACK = "61";
								msgACKByte = Encoding.ASCII.GetBytes(msgACK);
								socket.Send(msgACKByte);
								// signal that both players are ready
								if (battleships.hitPoints1 != 0 && battleships.hitPoints2 != 0) {
									eventReceive1.Set();
									eventReceive2.Set();
									// Send Turn first
									WaitHandle.WaitAll(new WaitHandle[] { eventSend1, eventSend2 });
									msgACK = "02";
									msgACKByte = Encoding.ASCII.GetBytes(msgACK);
									foreach (Socket client in clientSockets)
										client.Send(msgACKByte);
								}
								break;
							// 9 + Shoot Position
							case "9":
								string strShootIndex = msgFromClient.Substring(1, msgFromClient.Length - 1);
								int iShoot = Convert.ToInt32(strShootIndex.Substring(0, 1));
								int jShoot = Convert.ToInt32(strShootIndex.Substring(1, 1));
								int checkHit = battleships.CheckHit(iShoot, jShoot);
								battleships.CheckGameOver();
								// Send Shoot to be-shot client
								// 9 + Shoot Position + value
								string strShootIndexBeShot = string.Concat("9", strShootIndex, checkHit.ToString());
								if (battleships.Turn1)
									clientSockets[1].Send(Encoding.ASCII.GetBytes(strShootIndexBeShot));
								else
									clientSockets[0].Send(Encoding.ASCII.GetBytes(strShootIndexBeShot));
								// There's a winner. End game in client side
								// "A" + GameOver
								if (battleships.GameOver != 0) {
									string msg = string.Concat("A", battleships.GameOver);
									byte[] msgByte = Encoding.ASCII.GetBytes(msg);
									foreach (Socket client in clientSockets) 
										client.Send(msgByte);
									break;
								}
								// not yet
								else {
									// Shot + Hit
									if (checkHit == -1) {
										// Send Hit to just-shot client
										// 8 + Hit 1, Miss 0
										string msg = "81";
										byte[] msgByte = Encoding.ASCII.GetBytes(msg);
										socket.Send(msgByte);
										// Send Turn1 again to all clients
										battleships.Turn1 = !battleships.Turn1;
										// 7 + Turn
										if (battleships.Turn1) {
											msg = "71";
											msgByte = Encoding.ASCII.GetBytes(msg);
											clientSockets[0].Send(msgByte);
											eventSend1.Set();
											msg = "70";
											msgByte = Encoding.ASCII.GetBytes(msg);
											clientSockets[1].Send(msgByte);
											eventSend2.Set();
										}
										else {
											msg = "70";
											msgByte = Encoding.ASCII.GetBytes(msg);
											clientSockets[0].Send(msgByte);
											eventSend1.Set();
											msg = "71";
											msgByte = Encoding.ASCII.GetBytes(msg);
											clientSockets[1].Send(msgByte);
											eventSend2.Set();
										}
									}
									// Shot + !Hit
									else if (checkHit == -2) {
										// Send Hit to just-shot client
										// 8 + Hit 1, Miss 0
										string msg = "80";
										byte[] msgByte = Encoding.ASCII.GetBytes(msg);
										socket.Send(msgByte);
										// Send Turn1 again to all clients
										battleships.Turn1 = !battleships.Turn1;
										// 7 + Turn
										if (battleships.Turn1) {
											msg = "71";
											msgByte = Encoding.ASCII.GetBytes(msg);
											clientSockets[0].Send(msgByte);
											eventSend1.Set();
											msg = "70";
											msgByte = Encoding.ASCII.GetBytes(msg);
											clientSockets[1].Send(msgByte);
											eventSend2.Set();
										}
										else {
											msg = "70";
											msgByte = Encoding.ASCII.GetBytes(msg);
											clientSockets[0].Send(msgByte);
											eventSend1.Set();
											msg = "71";
											msgByte = Encoding.ASCII.GetBytes(msg);
											clientSockets[1].Send(msgByte);
											eventSend2.Set();
										}
									}
								}
								break;
						}
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
				clientSockets.Remove(socket);		// not covering enough cases.
			}

		}

		// Called when send data
		/*
		static void sendCallback(IAsyncResult result) {

			try {
				Socket socket = (Socket) result.AsyncState;
				int bytesSent = socket.EndSend(result);
				Console.WriteLine("Sent {0} bytes to client", bytesSent);
				// Signal sent
				if (socket == clientSockets[0])
					eventSend1.Set();
				else if (socket == clientSockets[1])
					eventSend2.Set();
			}
			catch (Exception e) {
				Console.WriteLine(e.ToString());
			}

		}
		*/

		static void Main(string[] args) {

			// Listening at SERVER_IP and PORT_NO
			SERVER_IP = GetLocalIP();
			Console.WriteLine("Server started at IP: {0}, Port: {1}", SERVER_IP, PORT_NO);
			serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			serverSocket.Bind(new IPEndPoint(IPAddress.Parse(SERVER_IP), PORT_NO));
			serverSocket.Listen(NUM_OF_CLIENTS); 
			Console.WriteLine("Listening...");

			// Begin accepting clients, Asyncly
			serverSocket.BeginAccept(new AsyncCallback(acceptCallback), null);

			WaitHandle.WaitAll(new WaitHandle[] { eventConnect1, eventConnect2 });
			battleships = new Battleships();
			if (clientSockets.Count == NUM_OF_CLIENTS && flag1time == false) {
					for (int i = 0; i < clientSockets.Count; i++) {
						// 2 Players connected
						flag1time = false;
						string msg ;
						byte[] msgByte;
						/// Send Battleships data once
						int iter = 0;
						while (!flag1time) {
							switch (iter) {
								// 0 + 2/2 confirmed
								case 0:
									msg = string.Concat(iter.ToString(), ACK);
									msgByte = Encoding.ASCII.GetBytes(msg);
									//clientSockets[i].BeginSend(msgByte, 0, msgByte.Length, SocketFlags.None, new AsyncCallback(sendCallback), clientSockets[i]);
									clientSockets[i].Send(msgByte);
									switch (i) {
										case 0:
											//WaitHandle.WaitAll(new WaitHandle[] { eventReceive1, eventSend1 });
											WaitHandle.WaitAll(new WaitHandle[] { eventReceive1 });
											break;
										case 1:
											//WaitHandle.WaitAll(new WaitHandle[] { eventReceive2, eventSend2 });
											WaitHandle.WaitAll(new WaitHandle[] { eventReceive2 });
											break;
									}
									iter++;
									break;
								// 1 + dimension
								case 1:
									msg = string.Concat(iter.ToString(), battleships.dimension.ToString());
									msgByte = Encoding.ASCII.GetBytes(msg);
									//clientSockets[i].BeginSend(msgByte, 0, msgByte.Length, SocketFlags.None, new AsyncCallback(sendCallback), clientSockets[i]);
									clientSockets[i].Send(msgByte);
									switch (i) {
										case 0:
											//WaitHandle.WaitAll(new WaitHandle[] { eventReceive1, eventSend1 });
											WaitHandle.WaitAll(new WaitHandle[] { eventReceive1 });
											break;
										case 1:
											//WaitHandle.WaitAll(new WaitHandle[] { eventReceive2, eventSend2 });
											WaitHandle.WaitAll(new WaitHandle[] { eventReceive2 });
											break;
									}
									iter++;
									break;
								// 2 + NumShips
								case 2:
									msg = iter.ToString();
									foreach (int n in battleships.NumShips) {
										msg = string.Concat(msg, n.ToString());
									}
									msgByte = Encoding.ASCII.GetBytes(msg);
									//clientSockets[i].BeginSend(msgByte, 0, msgByte.Length, SocketFlags.None, new AsyncCallback(sendCallback), clientSockets[i]);
									clientSockets[i].Send(msgByte);
									switch (i) {
										case 0:
											//WaitHandle.WaitAll(new WaitHandle[] { eventReceive1, eventSend1 });
											WaitHandle.WaitAll(new WaitHandle[] { eventReceive1 });
											break;
										case 1:
											//WaitHandle.WaitAll(new WaitHandle[] { eventReceive2, eventSend2 });
											WaitHandle.WaitAll(new WaitHandle[] { eventReceive2 });
											break;
									}
									iter++;
									break;
								// 3 + shipkinds
								case 3:
									// ship5
									msg = string.Concat(iter.ToString(), "5");
									foreach (int n in battleships.ship5) {
										msg = string.Concat(msg, n.ToString());
									}
									msgByte = Encoding.ASCII.GetBytes(msg);
									//clientSockets[i].BeginSend(msgByte, 0, msgByte.Length, SocketFlags.None, new AsyncCallback(sendCallback), clientSockets[i]);
									clientSockets[i].Send(msgByte);
									switch (i) {
										case 0:
											//WaitHandle.WaitAll(new WaitHandle[] { eventReceive1, eventSend1 });
											WaitHandle.WaitAll(new WaitHandle[] { eventReceive1 });
											break;
										case 1:
											//WaitHandle.WaitAll(new WaitHandle[] { eventReceive2, eventSend2 });
											WaitHandle.WaitAll(new WaitHandle[] { eventReceive2 });
											break;
									}
									// ship4
									msg = string.Concat(iter.ToString(), "4");
									foreach (int n in battleships.ship4) {
										msg = string.Concat(msg, n.ToString());
									}
									msgByte = Encoding.ASCII.GetBytes(msg);
									//clientSockets[i].BeginSend(msgByte, 0, msgByte.Length, SocketFlags.None, new AsyncCallback(sendCallback), clientSockets[i]);
									clientSockets[i].Send(msgByte);
									switch (i) {
										case 0:
											//WaitHandle.WaitAll(new WaitHandle[] { eventReceive1, eventSend1 });
											WaitHandle.WaitAll(new WaitHandle[] { eventReceive1 });
											break;
										case 1:
											//WaitHandle.WaitAll(new WaitHandle[] { eventReceive2, eventSend2 });
											WaitHandle.WaitAll(new WaitHandle[] { eventReceive2 });
											break;
									}
									// ship3
									msg = string.Concat(iter.ToString(), "3");
									foreach (int n in battleships.ship3) {
										msg = string.Concat(msg, n.ToString());
									}
									msgByte = Encoding.ASCII.GetBytes(msg);
									//clientSockets[i].BeginSend(msgByte, 0, msgByte.Length, SocketFlags.None, new AsyncCallback(sendCallback), clientSockets[i]);
									clientSockets[i].Send(msgByte);
									switch (i) {
										case 0:
											//WaitHandle.WaitAll(new WaitHandle[] { eventReceive1, eventSend1 });
											WaitHandle.WaitAll(new WaitHandle[] { eventReceive1 });
											break;
										case 1:
											//WaitHandle.WaitAll(new WaitHandle[] { eventReceive2, eventSend2 });
											WaitHandle.WaitAll(new WaitHandle[] { eventReceive2 });
											break;
									}
									// ship2
									msg = string.Concat(iter.ToString(), "2");
									foreach (int n in battleships.ship2) {
										msg = string.Concat(msg, n.ToString());
									}
									msgByte = Encoding.ASCII.GetBytes(msg);
									//clientSockets[i].BeginSend(msgByte, 0, msgByte.Length, SocketFlags.None, new AsyncCallback(sendCallback), clientSockets[i]);
									clientSockets[i].Send(msgByte);
									switch (i) {
										case 0:
											//WaitHandle.WaitAll(new WaitHandle[] { eventReceive1, eventSend1 });
											WaitHandle.WaitAll(new WaitHandle[] { eventReceive1 });
											break;
										case 1:
											//WaitHandle.WaitAll(new WaitHandle[] { eventReceive2, eventSend2 });
											WaitHandle.WaitAll(new WaitHandle[] { eventReceive2 });
											break;
									}
									iter++;
									break;
								// 4 + currShipLength
								case 4:
									msg = string.Concat(iter.ToString(), battleships.currShipLength.ToString());
									msgByte = Encoding.ASCII.GetBytes(msg);
									//clientSockets[i].BeginSend(msgByte, 0, msgByte.Length, SocketFlags.None, new AsyncCallback(sendCallback), clientSockets[i]);
									clientSockets[i].Send(msgByte);
									switch (i) {
										case 0:
											//WaitHandle.WaitAll(new WaitHandle[] { eventReceive1, eventSend1 });
											WaitHandle.WaitAll(new WaitHandle[] { eventReceive1 });
											break;
										case 1:
											//WaitHandle.WaitAll(new WaitHandle[] { eventReceive2, eventSend2 });
											WaitHandle.WaitAll(new WaitHandle[] { eventReceive2 });
											break;
									}
									iter++;
									break;
								// done send all things
								default: 
									flag1time = true;
									break;
							}
						}
					}
					flag1time = true;
					// 7 + Turn
					// wait until player 2 ready
					WaitHandle.WaitAll(new WaitHandle[] { eventReceive1, eventReceive2 });
					if (battleships.Turn1) {
						string msg = "71";
						byte[] msgByte = Encoding.ASCII.GetBytes(msg);
						clientSockets[0].Send(msgByte);
						eventSend1.Set();
						msg = "70";
						msgByte = Encoding.ASCII.GetBytes(msg);
						clientSockets[1].Send(msgByte);
						eventSend2.Set();
					}
					else {
						string msg = "70";
						byte[] msgByte = Encoding.ASCII.GetBytes(msg);
						clientSockets[0].Send(msgByte);
						eventSend1.Set();
						msg = "71";
						msgByte = Encoding.ASCII.GetBytes(msg);
						clientSockets[1].Send(msgByte);
						eventSend2.Set();
					}
			}
			// Core loop, keep server running
			while (true) ;

		}

	}

    /// Game
    public class Battleships {

	/// Data
		
		public int dimension;												// Board dimension
		public int[,] matrix1, matrix2;										// 2 boards for 2 players
		public int hitPoints1, hitPoints2;									// 2 HPs for 2 players
		public bool Turn1;													// 1 Turn. true if turn for player1
		public int[] NumShips;												// 1 No. Ships
		public int currShipLength;											// CurrShip for 2 players
		public int GameOver;												// 1 Player1 wwins
																			// 2 Player2 wins
																			// 0 not yet

		public readonly int[] ship5 = new int[5] { 1, 1, 1, 1, 1 };         // Ship Kinds
		public readonly int[] ship4 = new int[4] { 1, 1, 1, 1 };
		public readonly int[] ship3 = new int[3] { 1, 1, 1 };
		public readonly int[] ship2 = new int[2] { 1, 1 };

	/// Logic
		
		// Initiate everything
		public Battleships() {

			dimension = 10;	
			matrix1 = new int[dimension, dimension];
			matrix2 = new int[dimension, dimension];
			hitPoints1 = 0;
			hitPoints2 = 0;
			Turn1 = true;
			NumShips = new int[] { 1, 1, 2, 2 };				// 1 ship5, 1 ship4, 2 ship3, 2 ship2
			currShipLength = 5;
			GameOver = 0;
			
		}

		// Check hit
		public int CheckHit(int i, int j) {

			int checkHit = 0;
			// client1 shot
			if (Turn1) {
				// Shot + Hit
				if (matrix2[i, j] == 1) {
					matrix2[i, j] = -1;
					hitPoints2 -= 1;
					checkHit = -1;
				}
				// Shot + !Hit
				else {
					matrix2[i, j] = -2;
					checkHit = -2;
				}
			}
			// client2 shot
			else {
				// Shot + Hit
				if (matrix1[i, j] == 1) {
					matrix1[i, j] = -1;
					hitPoints1 -= 1;
					checkHit = -1;
				}
				// Shot + !Hit
				else {
					matrix1[i, j] = -2;
					checkHit = -2;
				}
			}
			//Turn1 = !Turn1;
			return checkHit;

		}

		// Check game over
		public void CheckGameOver() {

			if (hitPoints1 == 0) {
				GameOver = 2;						// Player 2 wins
			}
			if (hitPoints2 == 0) {
				GameOver = 1;						// Player 1 wins
			}
			// Not yet = 0

		}

	}

}



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Battleships {

	public static class Client {

		const int PORT_NO = 8888;
		const int BUFFER_SIZE = 4096;
		const int MAX_RECEIVE_ATTEMPT = 10;
		static int receiveAttempt = 0;
		static byte[] buffer = new byte[BUFFER_SIZE];
		static string SERVER_IP;
		static Socket clientSocket;

		// Main entry for client 
		public static void Start(string ServerIP, RichTextBox rtp) {

			clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			LoopConnect(ServerIP, 3, 3);
			rtp = Form1.tbInfo;
		}

		// Connect attempts
		private static void LoopConnect(string ServerIP, int noOfRetry, int attemptPeriodInSeconds) {

			int attempts = 0;
			while (!clientSocket.Connected && attempts < noOfRetry) {
				try {
					++attempts;
					IAsyncResult result = clientSocket.BeginConnect(IPAddress.Parse(ServerIP), PORT_NO, endConnectCallback, null);
					result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(attemptPeriodInSeconds));
					System.Threading.Thread.Sleep(attemptPeriodInSeconds * 1000);
				}
				catch (Exception e) {
					Console.WriteLine(e.ToString());
				}
			}
			if (!clientSocket.Connected) {
				Console.WriteLine("Connection attempts unsuccessful!");
				return;
			}

		}

		// Finally connected
		private static void endConnectCallback(IAsyncResult ar) {

			try {
				clientSocket.EndConnect(ar);
				if (clientSocket.Connected)
					clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(receiveCallback), clientSocket);
				else
					Console.WriteLine("End of connection attempt, fail to connect");
			}
			catch (Exception e) {
				Console.WriteLine(e.ToString());
			}
		}

		// When received data
		private static void receiveCallback(IAsyncResult ar) {

			Socket socket = null;
			try {
				socket = (Socket)ar.AsyncState;
				if (socket.Connected) {
					int received = socket.EndReceive(ar);
					if (received > 0) {
						receiveAttempt = 0;
						byte[] data = new byte[received];
						Buffer.BlockCopy(buffer, 0, data, 0, data.Length);
						Console.WriteLine("Server sent: ");
						Console.WriteLine(Encoding.ASCII.GetString(data));
						socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(receiveCallback), socket);
					}
					else if (receiveAttempt < MAX_RECEIVE_ATTEMPT) {
						++receiveAttempt;
						socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(receiveCallback), socket);
					}
					else {
						Console.WriteLine("receiveCallback has failed!");
						receiveAttempt = 0;
						clientSocket.Close();
					}
				}
			}
			catch (Exception e) {
				Console.WriteLine(e.ToString());
			}

		}

	}

}

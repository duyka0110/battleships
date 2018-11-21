using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;

namespace Battleships { 

	public partial class Form1 : Form {

		#region Fields
		/// CLient Fields
		const int PORT_NO = 8888;
		const int BUFFER_SIZE = 4096;
		const int MAX_RECEIVE_ATTEMPT = 10;
		static int receiveAttempt = 0;
		static byte[] buffer = new byte[BUFFER_SIZE];
		static string SERVER_IP;
		static Socket clientSocket;
		/// Form fields
		private int dimension = 10;
		private int boardNum = 2;
		private int leftAnchorSize = 20;
		Size sqSize = new Size(45, 45);
		private int midAnchorSize = 60;
		/// Others
		delegate void SetTextCallback(string text);
		/// Logic
		private bool TurnPlayer1 = true;
		private int hitPointsPlayer1;
		private int hitPointsPlayer2;
		private bool GameOver = false;
		private Graphics graphics;
		private int[,] matrix1 = new int[10, 10];
		private int[,] matrix2 = new int[10, 10];
		private int currentShipLength;
		private bool canPlaceShip = false;
		private int[] numOfEachShips = new int[] { 1, 1, 2, 2 };	// order: ship5, ship4, ship3, ship2
		private int[] ship5 = new int[5] { 1, 1, 1, 1, 1 }; // 1 of this
		private int[] ship4 = new int[4] { 1, 1, 1, 1 };	// 1 of this
		private int[] ship3 = new int[3] { 1, 1, 1 };		// 2 of this
		private int[] ship2 = new int[2] { 1, 1 };          // 2 of this
		private bool vertical = true;
		// player 2
		private int[] numOfEachShips2 = new int[] { 1, 1, 2, 2 };
		private int currentShipLength2;
		#endregion

		#region UI
		/// Form methods
		public Form1() {

			InitializeComponent();

		}

		private void Form1_Load(object sender, EventArgs e) {

			currentShipLength = 5;
			currentShipLength2 = 5;

			MouseDown += new MouseEventHandler(PlaceShipsOnMouseDown);
			attackBtn.Enabled = false;
			findBtn.Enabled = false;
			tbIP.Visible = false;
			createBtn.Text = "Connect to Server";

			// want to show this after 2 players connected
			SetText(Environment.NewLine + "Please place your ship on board (Length: " + currentShipLength + ", Orientation: " + (vertical ? "Vertical)" : "Horizontal)"));
			SetText(Environment.NewLine + "<Press 'O' to change orientation>");

		}

		private void DrawBoard(object sender, PaintEventArgs p) {

			// Some datas
			Point startLocation;
			//midAnchorSize = Size.Width - sqSize.Width * 10 - leftAnchorSize;

			Graphics g = p.Graphics;
			Pen pen = new Pen(Color.Black, 1);

			// Draw left board
			for (int i = leftAnchorSize; i < sqSize.Width * dimension; i += sqSize.Width) {
				for (int j = leftAnchorSize; j < sqSize.Width * dimension; j += sqSize.Width) {
					startLocation = new Point(i, j);
					Rectangle rect = new Rectangle(startLocation, sqSize);
					g.DrawRectangle(pen, rect);
				}
			}

			// Draw right board
			int leftAnchorSize2 = leftAnchorSize + sqSize.Width * dimension + midAnchorSize;
			int totalBoardsSizeHor = leftAnchorSize + sqSize.Width * dimension * boardNum + midAnchorSize;
			for (int i = leftAnchorSize2; i < totalBoardsSizeHor; i += sqSize.Width) {
				for (int j = leftAnchorSize; j < sqSize.Width * dimension; j += sqSize.Width) {
					startLocation = new Point(i, j);
					Rectangle rect = new Rectangle(startLocation, sqSize);
					g.DrawRectangle(pen, rect);
				}
			}

		}

		private void createBtn_Click(object sender, EventArgs e) {

			tbInfo.Text = "Please enter Server IP on the right side.";
			tbIP.Visible = true;

		}

		private void tbIP_KeyDown(object sender, KeyEventArgs e) {

			if (e.KeyCode == Keys.Enter) {
				StartClient(tbIP.Text);
				tbIP.Visible = false;
			}


		}
		#endregion

		#region Client
		/// Client Methods
		// Main entry for client 
		private void StartClient(string ServerIP) {

			clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			LoopConnect(ServerIP, 3, 3);
			
		}

		// Connect attempts
		private  void LoopConnect(string ServerIP, int noOfRetry, int attemptPeriodInSeconds) {

			int attempts = 0;
			while (!clientSocket.Connected && attempts < noOfRetry) {
				try {
					++attempts;
					IAsyncResult result = clientSocket.BeginConnect(IPAddress.Parse(ServerIP), PORT_NO, endConnectCallback, null);
					result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(attemptPeriodInSeconds));
					System.Threading.Thread.Sleep(attemptPeriodInSeconds * 1000);
				}
				catch (Exception e) {
					SetText(Environment.NewLine + e.ToString());
				}
			}
			if (!clientSocket.Connected) {
				SetText(Environment.NewLine + "Connection attempts unsuccessful!");
				return;
			}

		}

		// Finally connected
		private void endConnectCallback(IAsyncResult ar) {

			try {
				clientSocket.EndConnect(ar);
				if (clientSocket.Connected)
					clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(receiveCallback), clientSocket);
				else
					SetText(Environment.NewLine + "End of connection attempt, fail to connect");
			}
			catch (Exception e) {
				SetText(Environment.NewLine + e.ToString());
			}
		}

		// When received data
		private void receiveCallback(IAsyncResult ar) {

			Socket socket = null;
			try {
				socket = (Socket)ar.AsyncState;
				if (socket.Connected) {
					int received = socket.EndReceive(ar);
					if (received > 0) {
						receiveAttempt = 0;
						byte[] data = new byte[received];
						Buffer.BlockCopy(buffer, 0, data, 0, data.Length);
						SetText(Environment.NewLine + "Server sent: ");
						SetText(Environment.NewLine + Encoding.ASCII.GetString(data));
						socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(receiveCallback), socket);
					}
					else if (receiveAttempt < MAX_RECEIVE_ATTEMPT) {
						++receiveAttempt;
						socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(receiveCallback), socket);
					}
					else {
						SetText(Environment.NewLine + "receiveCallback has failed!");
						receiveAttempt = 0;
						clientSocket.Close();
					}
				}
			}
			catch (Exception e) {
				SetText(Environment.NewLine + e.ToString());
			}

		}
		#endregion

		#region Others
		/// Others
		// For thread-safe Controls calling
		private void SetText(string text) {
			if (tbInfo.InvokeRequired) {
				SetTextCallback d = new SetTextCallback(SetText);
				Invoke(d, new object[] { text });
			}
			else {
				tbInfo.Text += text;
			}
			tbInfo.SelectionStart = tbInfo.Text.Length;
			tbInfo.ScrollToCaret();
		}
		#endregion

		#region Logic
		// Ticking method
		private void ShootOnMouseDown(object sender, MouseEventArgs e) {

			int leftAnchorSize2 = leftAnchorSize + sqSize.Width * dimension + midAnchorSize;
			int totalBoardsSizeHor = leftAnchorSize + sqSize.Width * dimension * boardNum + midAnchorSize;
			int totalBoardSizeVer = leftAnchorSize + sqSize.Width * dimension;
			SolidBrush brush = new SolidBrush(Color.Red);
			Point location = e.Location;
			graphics = CreateGraphics();
			if (location.X < leftAnchorSize || location.X > totalBoardsSizeHor || location.Y < leftAnchorSize || location.Y > totalBoardSizeVer)
				return;
			// matrix1
			else if (location.X > leftAnchorSize && location.X < leftAnchorSize2 - midAnchorSize) {
				// Board index
				int i = (location.X - leftAnchorSize) / sqSize.Width; // col for matrix, row for UI. so stupid
				int j = (location.Y - leftAnchorSize) / sqSize.Width;
				// UI distance
				int midX = i * sqSize.Width + sqSize.Width / 2 + leftAnchorSize;
				int midY = j * sqSize.Width + sqSize.Width / 2 + leftAnchorSize;
				Point point = new Point(midX - leftAnchorSize/2, midY - leftAnchorSize/2);
				Size size = new Size(leftAnchorSize, leftAnchorSize);
				Rectangle rect = new Rectangle(point, size);
				graphics.FillEllipse(brush, rect);
				ShootShips(j, i);
			}
			// matrix2
			else if (location.X > leftAnchorSize2 && location.X < totalBoardsSizeHor) {
				// Board index
				int i = (location.X - leftAnchorSize2) / sqSize.Width; // same as above. so stupid, again
				int j = (location.Y - leftAnchorSize) / sqSize.Width;
				// UI distance
				int midX = i * sqSize.Width + sqSize.Width / 2 + leftAnchorSize2;
				int midY = j * sqSize.Width + sqSize.Width / 2 + leftAnchorSize;
				Point point = new Point(midX - leftAnchorSize/2, midY - leftAnchorSize/2);
				Size size = new Size(leftAnchorSize, leftAnchorSize);
				Rectangle rect = new Rectangle(point, size);
				graphics.FillEllipse(brush, rect);
				ShootShips(j, i);
			}
			else return;

		}

		// Change orientation of a ship
		private void Form_OnO(object sender, KeyEventArgs e) {

			if (e.KeyCode == Keys.O)
				vertical = !vertical;
			if (currentShipLength == 1) return;
			if (vertical) {
				SetText(Environment.NewLine + "Please place your ship on board (Length: " + currentShipLength + ", Orientation: Vertical)");
				SetText(Environment.NewLine + "<Press 'O' to change orientation>");
			}
			else {
				SetText(Environment.NewLine + "Please place your ship on board (Length: " + currentShipLength +", Orientation: Horizontal)");
				SetText(Environment.NewLine + "<Press 'O' to change orientation>");
			}

		}

		// Place ships method
		private void PlaceShipsOnMouseDown(object sender, MouseEventArgs e) {

			int leftAnchorSize2 = leftAnchorSize + sqSize.Width * dimension + midAnchorSize;
			int totalBoardsSizeHor = leftAnchorSize + sqSize.Width * dimension * boardNum + midAnchorSize;
			int totalBoardSizeVer = leftAnchorSize + sqSize.Width * dimension;
			SolidBrush brush = new SolidBrush(Color.DarkGray);
			Pen pen = new Pen(Color.Purple, 3);
			Point location = e.Location;
			graphics = CreateGraphics();

			/// outside board
			if (location.X < leftAnchorSize || location.X > totalBoardsSizeHor || location.Y < leftAnchorSize || location.Y > totalBoardSizeVer)
				return;
			// Magic number = 1, better UI. No magic number, worse UI.
			/// matrix1
			else if (location.X > leftAnchorSize && location.X < leftAnchorSize2 - midAnchorSize) {
				// Board index
				int i = (location.X - leftAnchorSize) / sqSize.Width; // col for matrix, row for UI. so stupid
				int j = (location.Y - leftAnchorSize) / sqSize.Width;
				// UI distance
				int x = i * sqSize.Width + leftAnchorSize + 1;
				int y = j * sqSize.Width + leftAnchorSize + 1;
				PlaceShips(currentShipLength, vertical, matrix1, j, i);
				if (canPlaceShip) {
					if (vertical) {
						graphics.DrawRectangle(pen, x - 1, y - 1, sqSize.Width, sqSize.Width * currentShipLength);
						for (int a = 0; a < currentShipLength; a++) {
							graphics.FillRectangle(brush, x, y, sqSize.Width - 1, sqSize.Height - 1);
							y += sqSize.Width;
						}
					}
					else {
						graphics.DrawRectangle(pen, x - 1, y - 1, sqSize.Width * currentShipLength, sqSize.Width);
						for (int a = 0; a < currentShipLength; a++) {
							graphics.FillRectangle(brush, x, y, sqSize.Width - 1, sqSize.Height - 1);
							x += sqSize.Width;
						}
					}
				}
				else return;

				/// Put outside when ready LAN
				// Continue placing ship until met requirements
				// magic number 1
				int currentShipIndex = numOfEachShips.Length + 1 - currentShipLength;
				numOfEachShips[currentShipIndex] -= 1;
				if (numOfEachShips[currentShipIndex] == 0) {
					currentShipLength--;
					if (currentShipLength == 1) {
						SetText(Environment.NewLine + "NO MORE SHIPS FOR PLAYER 1!");
						SetText(Environment.NewLine + "-------------------------------------------------");
						SetText(Environment.NewLine + "Player 2 placing...");
						SetText(Environment.NewLine + "Please place your ship on board (Length: " + currentShipLength2 + ", Orientation: " + (vertical ? "Vertical)" : "Horizontal)"));
						SetText(Environment.NewLine + "<Press 'O' to change orientation>");
						return; // no more ships placing
					}
					SetText(Environment.NewLine + "Please place your ship on board (Length: " + currentShipLength + ", Orientation: " + (vertical ? "Vertical)" : "Horizontal)"));
					SetText(Environment.NewLine + "<Press 'O' to change orientation>");
				}
				else {
					SetText(Environment.NewLine + "Please place your ship on board (Length: " + currentShipLength + ", Orientation: " + (vertical ? "Vertical)" : "Horizontal)"));
					SetText(Environment.NewLine + "<Press 'O' to change orientation>");
				}
			}
			/// matrix2
			else if (location.X > leftAnchorSize2 && location.X < totalBoardsSizeHor) {
				// Board index
				int i = (location.X - leftAnchorSize2) / sqSize.Width; // same as above. so stupid, again
				int j = (location.Y - leftAnchorSize) / sqSize.Width;
				// UI distance
				int x = i * sqSize.Width + leftAnchorSize2 + 1;
				int y = j * sqSize.Width + leftAnchorSize + 1;
				PlaceShips(currentShipLength2, vertical, matrix2, j, i);
				if (canPlaceShip) {
					if (vertical) {
						graphics.DrawRectangle(pen, x - 1, y - 1, sqSize.Width, sqSize.Width * currentShipLength2);
						for (int a = 0; a < currentShipLength2; a++) {
							graphics.FillRectangle(brush, x, y, sqSize.Width - 1, sqSize.Height - 1);
							y += sqSize.Width;
						}
					}
					else {
						graphics.DrawRectangle(pen, x - 1, y - 1, sqSize.Width * currentShipLength2, sqSize.Width);
						for (int a = 0; a < currentShipLength2; a++) {
							graphics.FillRectangle(brush, x, y, sqSize.Width - 1, sqSize.Height - 1);
							x += sqSize.Width;
						}
					}
				}
				else return;

				/// Put outside when ready LAN
				// Continue placing ship until met requirements
				// magic number 1
				int currentShipIndex2 = numOfEachShips2.Length + 1 - currentShipLength2;
				numOfEachShips2[currentShipIndex2] -= 1;
				if (numOfEachShips2[currentShipIndex2] == 0) {
					currentShipLength2--;
					if (currentShipLength2 == 1) {
						SetText(Environment.NewLine + "NO MORE SHIPS FOR PLAYER 2!");
						SetText(Environment.NewLine + "Click READY to start Battleshipping");
						MouseDown -= PlaceShipsOnMouseDown;
						return; // no more ships placing
					}
					SetText(Environment.NewLine + "Please place your ship on board (Length: " + currentShipLength2 + ", Orientation: " + (vertical ? "Vertical)" : "Horizontal)"));
					SetText(Environment.NewLine + "<Press 'O' to change orientation>");
				}
				else {
					SetText(Environment.NewLine + "Please place your ship on board (Length: " + currentShipLength2 + ", Orientation: " + (vertical ? "Vertical)" : "Horizontal)"));
					SetText(Environment.NewLine + "<Press 'O' to change orientation>");
				}
			}
			else return;

			/// Place to put code

		}

		// methods for placing ships
		private void PlaceShips(int length, bool vertical, int[,] matrix, int i, int j) {
			
			// Create the ship
			int[] ship = new int[length];
			for (int s = 0; s < length; s++)
				ship[s] = 1;
			// Check condition to place the ship
			// vertical type
			if (vertical) {
				if (i > dimension - length) {
					SetText(Environment.NewLine + "!CAN'T PLACE THE SHIP THERE!");
					canPlaceShip = false;
					return;
				}
				else {
					int ss = 0;
					for (int s = i; s < length + i; s++) {
						if (matrix[s, j] == 1) {
							SetText(Environment.NewLine + "!CAN'T PLACE THE SHIP THERE!");
							canPlaceShip = false;
							return;
						}
						else {
							matrix[s, j] = ship[ss++];
							canPlaceShip = true;
						}
					}
				}
			}
			// horizontal type
			else {
				if (j > dimension - length) {
					SetText(Environment.NewLine + "!CAN'T PLACE THE SHIP THERE!");
					canPlaceShip = false;
					return;
				}
				else {
					int ss = 0;
					for (int s = j; s < length + j; s++) {
						if (matrix[i, s] == 1) {
							SetText(Environment.NewLine + "!CAN'T PLACE THE SHIP THERE!");
							canPlaceShip = false;
							return;
						}
						else {
							matrix[i, s] = ship[ss++];
							canPlaceShip = true;
						}
					}
				}
			}

		}

		// methods for shooting ships
		// could work on preventing player from shooting at the same spot
		private void ShootShips(int i, int j) {

			if (TurnPlayer1) {
				if (matrix2[i, j] == 1) {
					matrix2[i, j] = -1; // Shot + Hit
					hitPointsPlayer2 -= 1;
					SetText(Environment.NewLine + "Hit!");
					if (hitPointsPlayer2 <= 0) {
						GameOver = true;
						SetText(Environment.NewLine + "GAME IS OVER!");
						SetText(Environment.NewLine + "PLAYER 1 WINS!!!");
						MouseDown -= ShootOnMouseDown;
						return;
					}
				}
				else if (matrix2[i, j] == 0)
					matrix2[i, j] = -2; // Shot + !Hit
				else if (matrix2[i, j] < 0) {
					SetText(Environment.NewLine + "You have shot there");
					SetText(Environment.NewLine + "Turn: " + (TurnPlayer1 ? "Player 1" : "Player2"));
					return;
				}
			}
			else {
				if (matrix1[i, j] == 1) {
					matrix1[i, j] = -1; // Shot + Hit
					hitPointsPlayer1 -= 1;
					SetText(Environment.NewLine + "Hit!");
					if (hitPointsPlayer1 <= 0) {
						GameOver = true;
						SetText(Environment.NewLine + "GAME IS OVER!");
						SetText(Environment.NewLine + "PLAYER 2 WINS!!!");
						MouseDown -= ShootOnMouseDown;
						return;
					}
				}
				else if (matrix1[i, j] == 0)
					matrix1[i, j] = -2; // Shot + !Hit
				else if (matrix1[i, j] < 0) {
					SetText(Environment.NewLine + "You have shot there");
					SetText(Environment.NewLine + "Turn: " + (TurnPlayer1 ? "Player 1" : "Player2"));
					return;
				}
			}

			TurnPlayer1 = !TurnPlayer1;
			SetText(Environment.NewLine + "Turn: " + (TurnPlayer1 ? "Player 1" : "Player2"));

		}

		// After ready, let's shoot
		private void readyBtn_Click(object sender, EventArgs e) {

			MouseDown -= PlaceShipsOnMouseDown;
			MouseDown += new MouseEventHandler(ShootOnMouseDown);
			readyBtn.Enabled = false;

			// recreate numOfEachShips due to placing ships problem
			numOfEachShips = new int[] { 1, 1, 2, 2 };
			numOfEachShips2 = new int[] { 1, 1, 2, 2 };
			int num = 0;
			hitPointsPlayer1 = hitPointsPlayer2 = 0;
			for (int i = 5; i > 1; i--) {
				hitPointsPlayer1 += numOfEachShips[num] * i;
				hitPointsPlayer2 += numOfEachShips2[num] * i;
				num += 1;
			}

			SetText(Environment.NewLine + "Turn: " + (TurnPlayer1 ? "Player 1" : "Player2"));

		}

		// take turns to play

		#endregion

	}

}

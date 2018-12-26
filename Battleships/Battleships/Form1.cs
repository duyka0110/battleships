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
using System.Threading;

namespace Battleships { 

	public partial class Form1 : Form {

		#region Fields
		/// CLient Fields
		const string ACK = "1";									// Acknowledge msg			
		const int PORT_NO = 8888;								// Port number
		const int BUFFER_SIZE = 4096;							// Received data size
		const int MAX_RECEIVE_ATTEMPT = 10;						// max Receive attempts
		static bool flag1time;									// 1 time data
		static bool flag1timeTurn;
		static int receiveAttempt = 0;
		static byte[] buffer = new byte[BUFFER_SIZE];			// Store received data
		static Socket clientSocket;	
		static string msgFromServer;							// Store msg from server
		static Queue<byte[]> DataQueue = new Queue<byte[]>();
		static AutoResetEvent eventConnect = new AutoResetEvent(false);
		static AutoResetEvent eventSend = new AutoResetEvent(false);
		static AutoResetEvent eventReceive = new AutoResetEvent(false);
		static AutoResetEvent eventPaint = new AutoResetEvent(false);

		/// Form fields
		private int dimension;									// Board dimension, from Server
		private int boardNum = 2;								// Number of boards to use
		private int leftAnchorSize = 20;						// Starting point to draw board from the left
		Size sqSize = new Size(45, 45);							// Size of each individual square
		private int midAnchorSize = 60;                         // Distance from between 2 boards, from the left
		private Graphics graphics;
		private bool boardFlag;
		private bool hitFlag;
		private bool beShotFlag;

		/// Others
		delegate void SetTextCallback(string text);				// Thread safe 

		/// Logic
		private int[,] matrixAttack, matrixDefend;				// matrixAttack to write your attack 
																// matrixDefend to read opp's attack and write PlaceShips
		private bool CanPlaceShip;								// check if can place ship
		private bool Vertical;									// ship orientation, true = Vertical, false = horizontal
		private bool Turn;										// Turn to shoot ships, from Server
		private int[] NumShips;									// Number of each ships, from Server
		private int GameOver;									// Game Over checking, from Server
		private int hitPointsPlayer;							// Player hit points
		private int currShipLength;								// Current ship length to place on board
		private int[] ship5;									// 1 of this
		private int[] ship4;									// 1 of this
		private int[] ship3;									// 2 of this	
		private int[] ship2;									// 2 of this
		private int shootI;										// Shooting indices
		private int shootJ;
		private int beShotI2;
		private int beShotJ2;
		#endregion

		#region UI
		/// Form methods
		public Form1() {

			InitializeComponent();

		}

		private void Form1_Load(object sender, EventArgs e) {

			//matrixAttack = matrixDefend = new int[10,10];
			//currShipLength = 5;
			//currShipLength2 = 5;
			Vertical = true;

			boardFlag = false;
			hitFlag = false;
			beShotFlag = false;

			readyBtn.Enabled = false;
			tbIP.Visible = false;
			createBtn.Text = "Connect to Server";			

		}

		/*
		private void DrawBoard(object sender, PaintEventArgs p) {

			// Some datas
			Point startLocation;
			//midAnchorSize = Size.Width - sqSize.Width * 10 - leftAnchorSize;

			graphics = CreateGraphics();
			Pen pen = new Pen(Color.Black, 1);

			// Draw left board
			for (int i = leftAnchorSize; i < sqSize.Width * dimension; i += sqSize.Width) {
				for (int j = leftAnchorSize; j < sqSize.Width * dimension; j += sqSize.Width) {
					startLocation = new Point(i, j);
					Rectangle rect = new Rectangle(startLocation, sqSize);
					graphics.DrawRectangle(pen, rect);
				}
			}

			// Draw right board
			int leftAnchorSize2 = leftAnchorSize + sqSize.Width * dimension + midAnchorSize;
			int totalBoardsSizeHor = leftAnchorSize + sqSize.Width * dimension * boardNum + midAnchorSize;
			for (int i = leftAnchorSize2; i < totalBoardsSizeHor; i += sqSize.Width) {
				for (int j = leftAnchorSize; j < sqSize.Width * dimension; j += sqSize.Width) {
					startLocation = new Point(i, j);
					Rectangle rect = new Rectangle(startLocation, sqSize);
					graphics.DrawRectangle(pen, rect);
				}
			}
			graphics.Dispose();

		}

		private void DrawBoard() {

			// Some datas
			Point startLocation;
			//midAnchorSize = Size.Width - sqSize.Width * 10 - leftAnchorSize;

			graphics = CreateGraphics();
			Pen pen = new Pen(Color.Black, 1);

			// Draw left board
			for (int i = leftAnchorSize; i < sqSize.Width * dimension; i += sqSize.Width) {
				for (int j = leftAnchorSize; j < sqSize.Width * dimension; j += sqSize.Width) {
					startLocation = new Point(i, j);
					Rectangle rect = new Rectangle(startLocation, sqSize);
					graphics.DrawRectangle(pen, rect);
				}
			}

			// Draw right board
			int leftAnchorSize2 = leftAnchorSize + sqSize.Width * dimension + midAnchorSize;
			int totalBoardsSizeHor = leftAnchorSize + sqSize.Width * dimension * boardNum + midAnchorSize;
			for (int i = leftAnchorSize2; i < totalBoardsSizeHor; i += sqSize.Width) {
				for (int j = leftAnchorSize; j < sqSize.Width * dimension; j += sqSize.Width) {
					startLocation = new Point(i, j);
					Rectangle rect = new Rectangle(startLocation, sqSize);
					graphics.DrawRectangle(pen, rect);
				}
			}
			Invalidate();
			//graphics.Dispose();

		}
		*/

		private void createBtn_Click(object sender, EventArgs e) {

			SetText(Environment.NewLine + "Please enter Server IP on the right side.");
			tbIP.Visible = true;

		}

		private void tbIP_KeyDown(object sender, KeyEventArgs e) {

			if (e.KeyCode == Keys.Enter) {
				SetText(Environment.NewLine + "Connecting ... ");
				StartClient(tbIP.Text);
				tbIP.Visible = false;
				createBtn.Enabled = false;
			}

		}

		// Ticking method
		private void ShootOnMouseDown(object sender, MouseEventArgs e) {

			int leftAnchorSize2 = leftAnchorSize + sqSize.Width * dimension + midAnchorSize;
			int totalBoardsSizeHor = leftAnchorSize + sqSize.Width * dimension * boardNum + midAnchorSize;
			int totalBoardSizeVer = leftAnchorSize + sqSize.Width * dimension;
			SolidBrush brush = new SolidBrush(Color.Red);
			Point location = e.Location;
			graphics = CreateGraphics();
			if (location.X < leftAnchorSize || location.X > totalBoardsSizeHor || location.Y < leftAnchorSize || location.Y > totalBoardSizeVer || (location.X > leftAnchorSize2 && location.X < totalBoardsSizeHor))
				return;
			// matrix1
			// This is matrixAttack
			else if (location.X > leftAnchorSize && location.X < leftAnchorSize2 - midAnchorSize) {
				// Board index
				shootJ = (location.X - leftAnchorSize) / sqSize.Width; // col for matrix, row for UI. so stupid
				shootI = (location.Y - leftAnchorSize) / sqSize.Width;
				// Check if that place can be shot
				if (matrixAttack[shootI, shootJ] < 0) {
					SetText(Environment.NewLine + "You have already shot there!");
					return;
				}
				// UI distance
				int midX = shootJ * sqSize.Width + sqSize.Width / 2 + leftAnchorSize;
				int midY = shootI * sqSize.Width + sqSize.Width / 2 + leftAnchorSize;
				Point point = new Point(midX - leftAnchorSize / 2, midY - leftAnchorSize / 2);
				Size size = new Size(leftAnchorSize, leftAnchorSize);
				Rectangle rect = new Rectangle(point, size);
				graphics.FillEllipse(brush, rect);
				// SEND position to server
				// 9 + Shoot postions
				string strShootIndex = "9";
				// position of shoot is (j,i)
				strShootIndex = string.Concat(strShootIndex, shootI.ToString(), shootJ.ToString());
				clientSocket.Send(Encoding.ASCII.GetBytes(strShootIndex));
			}
			else return;
			graphics.Dispose();

		}

		/*
		// Draw visual Hit Position, on matrixAttack
		private void DrawHitPos() {

			graphics = CreateGraphics();
			SolidBrush brush = new SolidBrush(Color.Red);
			// UI distance
			int topX = shootJ * sqSize.Width + leftAnchorSize;
			int topY = shootI * sqSize.Width + leftAnchorSize;
			Point point = new Point(topX, topY);
			Rectangle rect = new Rectangle(point, sqSize);
			graphics.FillRectangle(brush, rect);
			Invalidate(rect);
			graphics.Dispose();

		}
		
		// Draw visual Shoot Position, on matrixDefend
		private void DrawShootPos(int beShootI, int beShootJ, int beShotValue) {

			int leftAnchorSize2 = leftAnchorSize + sqSize.Width * dimension + midAnchorSize;
			graphics = CreateGraphics();
			SolidBrush brush;
			Pen pen;
			int x, y;
			/*
			if (beShotValue == -1) {
				/// Redraw board element UI
				brush = new SolidBrush(Color.DarkGray);
				pen = new Pen(Color.Purple, 3);
				// UI distance
				x = beShootJ * sqSize.Width + leftAnchorSize2 + 1;
				y = beShootI * sqSize.Width + leftAnchorSize + 1;
				graphics.FillRectangle(brush, x, y, sqSize.Width - 1, sqSize.Height - 1);
			}
			
			/// Draw Shoot UI
			brush = new SolidBrush(Color.Purple);
			// UI distance
			x = beShootJ * sqSize.Width + sqSize.Width / 2 + leftAnchorSize2;
			y = beShootI * sqSize.Width + sqSize.Width / 2 + leftAnchorSize;
			Point point = new Point(x - leftAnchorSize / 2, y - leftAnchorSize / 2);
			Size size = new Size(leftAnchorSize, leftAnchorSize);
			Rectangle rect = new Rectangle(point, size);
			graphics.FillEllipse(brush, rect);
			rect = new Rectangle(x, y, sqSize.Width - 1, sqSize.Width - 1);
			Invalidate(rect);
			graphics.Dispose();

		}
		*/

		// A specific region
		private Rectangle ThisRect() {

			Rectangle rect = new Rectangle();
			// Hit region
			if (hitFlag) {
				int topX = shootJ * sqSize.Width + leftAnchorSize;
				int topY = shootI * sqSize.Width + leftAnchorSize;
				Point point = new Point(topX, topY);
				rect = new Rectangle(point, sqSize);
			}

			// BeShot region
			if (beShotFlag) {
				int leftAnchorSize2 = leftAnchorSize + sqSize.Width * dimension + midAnchorSize;
				int x, y;
				x = beShotJ2 * sqSize.Width + sqSize.Width / 2 + leftAnchorSize2;
				y = beShotI2 * sqSize.Width + sqSize.Width / 2 + leftAnchorSize;
				Point point = new Point(x - leftAnchorSize / 2, y - leftAnchorSize / 2);
				Size size = new Size(leftAnchorSize, leftAnchorSize);
				rect = new Rectangle(point, size);
			}
			return rect;

		}

		// OnPaint
		protected override void OnPaint(PaintEventArgs e) {

			base.OnPaint(e);
			graphics = e.Graphics;
			// Draw board
			if (boardFlag) {
				// Some datas
				Point startLocation;
				//midAnchorSize = Size.Width - sqSize.Width * 10 - leftAnchorSize;
				Pen pen = new Pen(Color.Black, 1);
				// Draw left board
				for (int i = leftAnchorSize; i < sqSize.Width * dimension; i += sqSize.Width) {
					for (int j = leftAnchorSize; j < sqSize.Width * dimension; j += sqSize.Width) {
						startLocation = new Point(i, j);
						Rectangle rect = new Rectangle(startLocation, sqSize);
						graphics.DrawRectangle(pen, rect);
					}
				}
				// Draw right board
				int leftAnchorSize2 = leftAnchorSize + sqSize.Width * dimension + midAnchorSize;
				int totalBoardsSizeHor = leftAnchorSize + sqSize.Width * dimension * boardNum + midAnchorSize;
				for (int i = leftAnchorSize2; i < totalBoardsSizeHor; i += sqSize.Width) {
					for (int j = leftAnchorSize; j < sqSize.Width * dimension; j += sqSize.Width) {
						startLocation = new Point(i, j);
						Rectangle rect = new Rectangle(startLocation, sqSize);
						graphics.DrawRectangle(pen, rect);
					}
				}
				eventPaint.Set();
			}

			// Draw Hit position
			if (hitFlag) {
				SolidBrush brush = new SolidBrush(Color.Red);
				// UI distance
				int topX = shootJ * sqSize.Width + leftAnchorSize;
				int topY = shootI * sqSize.Width + leftAnchorSize;
				Point point = new Point(topX, topY);
				Rectangle rect = new Rectangle(point, sqSize);
				graphics.FillRectangle(brush, rect);
				eventPaint.Set();
			}

			// Draw BeShot position
			if (beShotFlag) {
				int leftAnchorSize2 = leftAnchorSize + sqSize.Width * dimension + midAnchorSize;
				SolidBrush brush;
				/// Draw Shoot UI
				brush = new SolidBrush(Color.Purple);
				// UI distance
				int x = beShotJ2 * sqSize.Width + sqSize.Width / 2 + leftAnchorSize2;
				int y = beShotI2 * sqSize.Width + sqSize.Width / 2 + leftAnchorSize;
				Point point = new Point(x - leftAnchorSize / 2, y - leftAnchorSize / 2);
				Size size = new Size(leftAnchorSize, leftAnchorSize);
				Rectangle rect = new Rectangle(point, size);
				graphics.FillEllipse(brush, rect);
				eventPaint.Set();
			}

		}
		

		// Change orientation of a ship
		private void Form_OnO(object sender, KeyEventArgs e) {

			if (e.KeyCode == Keys.O)
				Vertical = !Vertical;
			if (currShipLength == 1) return;
			if (Vertical) {
				SetText(Environment.NewLine + "Please place your ship on board (Length: " + currShipLength + ", Orientation: Vertical)");
				SetText(Environment.NewLine + "<Press 'O' to change orientation>");
			}
			else {
				SetText(Environment.NewLine + "Please place your ship on board (Length: " + currShipLength + ", Orientation: Horizontal)");
				SetText(Environment.NewLine + "<Press 'O' to change orientation>");
			}

		}

		// Place ships method
		private void PlaceShipsOnMouseDown(object sender, MouseEventArgs e) {

			// some data related to UI
			int leftAnchorSize2 = leftAnchorSize + sqSize.Width * dimension + midAnchorSize;
			int totalBoardsSizeHor = leftAnchorSize + sqSize.Width * dimension * boardNum + midAnchorSize;
			int totalBoardSizeVer = leftAnchorSize + sqSize.Width * dimension;
			SolidBrush brush = new SolidBrush(Color.DarkGray);
			Pen pen = new Pen(Color.Purple, 3);
			Point location = e.Location;
			graphics = CreateGraphics();

			/// outside board
			if (location.X < leftAnchorSize || location.X > totalBoardsSizeHor || location.Y < leftAnchorSize || location.Y > totalBoardSizeVer || (location.X > leftAnchorSize && location.X < leftAnchorSize2 - midAnchorSize))
				return;
			/// this is matrixDefend. PlaceShip and attacked here
			else if (location.X > leftAnchorSize2 && location.X < totalBoardsSizeHor) {
				// Board index
				int i = (location.X - leftAnchorSize2) / sqSize.Width; // same as above. so stupid, again
				int j = (location.Y - leftAnchorSize) / sqSize.Width;
				// UI distance
				int x = i * sqSize.Width + leftAnchorSize2 + 1;
				int y = j * sqSize.Width + leftAnchorSize + 1;
				PlaceShips(currShipLength, Vertical, matrixDefend, j, i);
				if (CanPlaceShip) {
					if (Vertical) {
						graphics.DrawRectangle(pen, x - 1, y - 1, sqSize.Width, sqSize.Width * currShipLength);
						for (int a = 0; a < currShipLength; a++) {
							graphics.FillRectangle(brush, x, y, sqSize.Width - 1, sqSize.Height - 1);
							y += sqSize.Width;
						}
					}
					else {
						graphics.DrawRectangle(pen, x - 1, y - 1, sqSize.Width * currShipLength, sqSize.Width);
						for (int a = 0; a < currShipLength; a++) {
							graphics.FillRectangle(brush, x, y, sqSize.Width - 1, sqSize.Height - 1);
							x += sqSize.Width;
						}
					}
				}
				else return;

				/// Put outside when ready LAN
				// Continue placing ship until met requirements
				// magic number 1
				// hitPointPlayer
				hitPointsPlayer += currShipLength;
				int currentShipIndex = NumShips.Length + 1 - currShipLength;
				NumShips[currentShipIndex] -= 1;
				if (NumShips[currentShipIndex] == 0) {
					currShipLength--;
					if (currShipLength == 1) {
						SetText(Environment.NewLine + "NO MORE SHIPS FOR PLACING!");
						SetText(Environment.NewLine + "Click READY to start Battleshipping");
						readyBtn.Enabled = true;
						MouseDown -= PlaceShipsOnMouseDown;
						return; // no more ships placing
					}
					SetText(Environment.NewLine + "Please place your ship on ATTACKED board (Length: " + currShipLength + ", Orientation: " + (Vertical ? "Vertical)" : "Horizontal)"));
					SetText(Environment.NewLine + "<Press 'O' to change orientation>");
				}
				else {
					SetText(Environment.NewLine + "Please place your ship on ATTACKED board (Length: " + currShipLength + ", Orientation: " + (Vertical ? "Vertical)" : "Horizontal)"));
					SetText(Environment.NewLine + "<Press 'O' to change orientation>");
				}
			}
			else return;
			graphics.Dispose();

		}

		// methods for placing ships
		private void PlaceShips(int length, bool Vertical, int[,] matrix, int i, int j) {

			// Create the ship
			int[] ship = new int[length];
			for (int s = 0; s < length; s++)
				ship[s] = 1;
			// Check condition to place the ship
			// Vertical type
			if (Vertical) {
				if (i > dimension - length) {
					SetText(Environment.NewLine + "!CAN'T PLACE THE SHIP THERE!");
					CanPlaceShip = false;
					return;
				}
				else {
					int ss = 0;
					for (int s = i; s < length + i; s++) {
						if (matrix[s, j] == 1) {
							SetText(Environment.NewLine + "!CAN'T PLACE THE SHIP THERE!");
							CanPlaceShip = false;
							return;
						}
						else {
							matrix[s, j] = ship[ss++];
							CanPlaceShip = true;
						}
					}
				}
			}
			// horizontal type
			else {
				if (j > dimension - length) {
					SetText(Environment.NewLine + "!CAN'T PLACE THE SHIP THERE!");
					CanPlaceShip = false;
					return;
				}
				else {
					int ss = 0;
					for (int s = j; s < length + j; s++) {
						if (matrix[i, s] == 1) {
							SetText(Environment.NewLine + "!CAN'T PLACE THE SHIP THERE!");
							CanPlaceShip = false;
							return;
						}
						else {
							matrix[i, s] = ship[ss++];
							CanPlaceShip = true;
						}
					}
				}
			}

		}

		// After ready, let's shoot
		private void readyBtn_Click(object sender, EventArgs e) {

			MouseDown -= PlaceShipsOnMouseDown;
			readyBtn.Enabled = false;
			// Send position of placed ships
			// 5 + Ships positions
			string strMatrixIndex = "5";
			for (int i = 0; i < dimension; i++) {
				for (int j = 0; j < dimension; j++) {
					// if that element == 1, means placed ship
					if (matrixDefend[i, j] == 1) {
						strMatrixIndex = string.Concat(strMatrixIndex, i.ToString(), j.ToString(), matrixDefend[i, j].ToString()); 
					}
				}
			}
			clientSocket.Send(Encoding.ASCII.GetBytes(strMatrixIndex));
			// Wait for server to accept
			WaitHandle.WaitAll(new WaitHandle[] { eventReceive });
			// 6 + hitPointsPlayer
			strMatrixIndex = string.Concat("6", hitPointsPlayer.ToString());
			clientSocket.Send(Encoding.ASCII.GetBytes(strMatrixIndex));
			WaitHandle.WaitAll(new WaitHandle[] { eventReceive });
			
		}

		#endregion

		#region Client
		/// Client Methods
		// Main entry for client 
		private void StartClient(string ServerIP) {

			clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			flag1time = false;
			flag1timeTurn = false;
			LoopConnect(ServerIP, 3, 3);
			
		}

		// Connect attempts
		private void LoopConnect(string ServerIP, int noOfRetry, int attemptPeriodInSeconds) {

			int attempts = 0;
			while (!clientSocket.Connected && attempts < noOfRetry) {
				try {
					++attempts;
					IAsyncResult result = clientSocket.BeginConnect(IPAddress.Parse(ServerIP), PORT_NO, endConnectCallback, null);
					result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(attemptPeriodInSeconds));
					Thread.Sleep(attemptPeriodInSeconds * 1000);
				}
				catch (Exception e) {
					SetText(Environment.NewLine + e.ToString());
					createBtn.Enabled = true;
				}
			}
			if (!clientSocket.Connected) {
				SetText(Environment.NewLine + "Connection attempts unsuccessful!");
				createBtn.Enabled = true;
				return;
			}

		}

		// Finally connected
		private void endConnectCallback(IAsyncResult ar) {

			try {
				clientSocket.EndConnect(ar);
				if (clientSocket.Connected) {
					SetText(Environment.NewLine + "Connected");
					SetText(Environment.NewLine + "Waiting for Player 2");
					try {
						clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(receiveCallback), clientSocket);
						eventConnect.Set();
					}
					catch (Exception e) {
						SetText(Environment.NewLine + e);
					}
				}
				else {
					SetText(Environment.NewLine + "End of connection attempt, fail to connect");
					createBtn.Enabled = true;
				}
			}
			catch (Exception e) {
				SetText(Environment.NewLine + e.ToString());
				createBtn.Enabled = true;
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
						string msg;
						receiveAttempt = 0;
						byte[] data = new byte[received];
						Buffer.BlockCopy(buffer, 0, data, 0, data.Length);
						msgFromServer = Encoding.ASCII.GetString(data);
						switch (msgFromServer.Substring(0,1)) {
							// 0 + 2/2 confirmed
							case "0":
								// wait for eventConnect: tb writing "Waiting for Player 2"
								WaitHandle.WaitAll(new WaitHandle [] { eventConnect });
								if (msgFromServer.Substring(1, msgFromServer.Length - 1) == ACK) {
									SetText(Environment.NewLine + "Player 2 connected");
									SetText(Environment.NewLine + "Starting game now");
									SetText(Environment.NewLine + "Loading dimension...");
									// Signal server to send next data
									msg = "01";
									clientSocket.Send(Encoding.ASCII.GetBytes(msg));
								}
								else if (msgFromServer.Substring(1, msgFromServer.Length - 1) == "2") {
									flag1timeTurn = true;
									// Need to wait for player 2 also ready
									if (Turn) {
										SetText(Environment.NewLine + "Your Turn");
										MouseDown += new MouseEventHandler(ShootOnMouseDown);
									}
									else {
										SetText(Environment.NewLine + "Waiting for other player to shoot...");
									}
								}
								break;
							// 1 + dimension
							case "1":
								dimension = Convert.ToInt32(msgFromServer.Substring(1, msgFromServer.Length - 1));
								SetText(Environment.NewLine + "Loaded dimension" + dimension);
								SetText(Environment.NewLine + "Loading NumShips...");
								matrixAttack = new int[dimension, dimension];
								matrixDefend = new int[dimension, dimension];
								// Draw board now
								//this.Paint += new PaintEventHandler(DrawBoard);
								//this.Invalidate();
								//this.Paint -= DrawBoard;
								boardFlag = true;
								Invalidate();
								WaitHandle.WaitAll(new WaitHandle[] { eventPaint });
								boardFlag = false;
								msg = "11";
								clientSocket.Send(Encoding.ASCII.GetBytes(msg));
								
								break;
							// 2 + NumShips
							case "2":
								NumShips = new int[msgFromServer.Length - 1];
								for (int i = 0; i < NumShips.Length; i++)
									NumShips[i] = int.Parse(msgFromServer.Substring(i + 1, 1));
								SetText(Environment.NewLine + "Loaded NumShips" + msgFromServer.Substring(1, msgFromServer.Length - 1).ToString());
								SetText(Environment.NewLine + "Loading ShipKinds...");
								msg = "21";
								clientSocket.Send(Encoding.ASCII.GetBytes(msg));
								break;
							// 3 + shipkinds
							case "3":
								switch (msgFromServer.Substring(1, 1)) {
									// ship5
									case "5":
										ship5 = new int[msgFromServer.Length - 2];
										for (int i = 0; i < ship5.Length; i++)
											ship5[i] = int.Parse(msgFromServer.Substring(i + 2, 1));
										SetText(Environment.NewLine + "Loaded ship5" + msgFromServer.Substring(2, msgFromServer.Length - 2).ToString());
										SetText(Environment.NewLine + "Loading ShipKinds...");
										msg = "31";
										clientSocket.Send(Encoding.ASCII.GetBytes(msg));
										break;
									// ship4
									case "4":
										ship4 = new int[msgFromServer.Length - 2];
										for (int i = 0; i < ship4.Length; i++)
											ship4[i] = int.Parse(msgFromServer.Substring(i + 2, 1));
										SetText(Environment.NewLine + "Loaded ship4" + msgFromServer.Substring(2, msgFromServer.Length - 2).ToString());
										SetText(Environment.NewLine + "Loading ShipKinds...");
										msg = "31";
										clientSocket.Send(Encoding.ASCII.GetBytes(msg));
										break;
									// ship3
									case "3":
										ship3 = new int[msgFromServer.Length - 2];
										for (int i = 0; i < ship3.Length; i++)
											ship3[i] = int.Parse(msgFromServer.Substring(i + 2, 1));
										SetText(Environment.NewLine + "Loaded ship3" + msgFromServer.Substring(2, msgFromServer.Length - 2).ToString());
										SetText(Environment.NewLine + "Loading ShipKinds...");
										msg = "31";
										clientSocket.Send(Encoding.ASCII.GetBytes(msg));
										break;
									// ship2
									case "2":
										ship2 = new int[msgFromServer.Length - 2];
										for (int i = 0; i < ship2.Length; i++)
											ship2[i] = int.Parse(msgFromServer.Substring(i + 2, 1));
										SetText(Environment.NewLine + "Loaded ship2" + msgFromServer.Substring(2, msgFromServer.Length - 2).ToString());
										SetText(Environment.NewLine + "Loading currShipLength...");
										msg = "31";
										clientSocket.Send(Encoding.ASCII.GetBytes(msg));
										break;
								}
								break;
							// 4 + currShipLength
							case "4":
								currShipLength = Convert.ToInt32(msgFromServer.Substring(1, msgFromServer.Length - 1));
								SetText(Environment.NewLine + "Loaded currShipLength" + msgFromServer.Substring(1, msgFromServer.Length - 1).ToString());
								SetText(Environment.NewLine + "READY!!!");
								msg = "41";
								clientSocket.Send(Encoding.ASCII.GetBytes(msg));
								// Start place ships here?
								MouseDown += new MouseEventHandler(PlaceShipsOnMouseDown);
								KeyDown += new KeyEventHandler(Form_OnO);
								SetText(Environment.NewLine + "Please place your ship on ATTACKED board (Length: " + currShipLength + ", Orientation: " + (Vertical ? "Vertical)" : "Horizontal)"));
								SetText(Environment.NewLine + "<Press 'O' to change orientation>");
								break;
							// 5 + ships positions
							case "5":
								string msgACK = msgFromServer.Substring(1, msgFromServer.Length - 1);
								if (msgACK == ACK)
									// Signal to Send(hitPointsPlayer)
									eventReceive.Set();
								break;
							// 6 + hitPointsPlayer
							case "6":
								msgACK = msgFromServer.Substring(1, msgFromServer.Length - 1);
								break;
							// 7 + Turn
							case "7":
								int turn = Convert.ToInt32(msgFromServer.Substring(1, msgFromServer.Length - 1));
								if (turn == 0) { 
									Turn = false;
									if (flag1timeTurn) {
										SetText(Environment.NewLine + "Wait for other player to shoot...");
										MouseDown -= ShootOnMouseDown;
									}
								}
								else { 
									Turn = true;
									if (flag1timeTurn) {
										SetText(Environment.NewLine + "Your turn");
										MouseDown += ShootOnMouseDown;
									}
								}
								eventReceive.Set();
								// To signal "02" msg
								eventConnect.Set();
								break;
							// 8 + Hit or not
							// + update visual guide for Hit position
							case "8":
								Rectangle rect;
								int isHit = Convert.ToInt32(msgFromServer.Substring(1, msgFromServer.Length - 1));
								if (isHit == 1) {
									matrixAttack[shootI, shootJ] = -1;
									SetText(Environment.NewLine + "You Hit!");
									//DrawHitPos();
									hitFlag = true;
									rect = ThisRect();
									Invalidate(rect);
									WaitHandle.WaitAll(new WaitHandle[] { eventPaint });
									hitFlag = false;
								}
								else if (isHit == 0) {
									matrixAttack[shootI, shootJ] = -2;
									SetText(Environment.NewLine + "You Miss");
								}
								break;
							// 9 + Be-Shot Position
							case "9":
								string strShootIndex = msgFromServer.Substring(1, msgFromServer.Length - 1);
								int beShotI = Convert.ToInt32(strShootIndex.Substring(0, 1));
								int beShotJ = Convert.ToInt32(strShootIndex.Substring(1, 1));
								beShotI2 = beShotI;
								beShotJ2 = beShotJ;
								int beShotValue = int.Parse(strShootIndex.Substring(2, strShootIndex.Length - 2));
								matrixDefend[beShotI, beShotJ] = beShotValue;
								//DrawShootPos(beShotI, beShotJ, beShotValue);
								beShotFlag = true;
								rect = ThisRect();
								Invalidate(rect);
								WaitHandle.WaitAll(new WaitHandle[] { eventPaint });
								beShotFlag = false;
								break;
							// "A" + GameOver
							case "A":
								int GameOver = Convert.ToInt32(msgFromServer.Substring(1, msgFromServer.Length - 1));
								// Lose
								if (!Turn && GameOver != 0) 
									SetText(Environment.NewLine + "YOU LOSEEE!!!");
								// Win
								else if (Turn && GameOver != 0) {
									SetText(Environment.NewLine + "YOU WINNNNNN!!!!!!!!");
									MouseDown -= ShootOnMouseDown;
								}

								break;
						}
						clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(receiveCallback), clientSocket);
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
				tbInfo.SelectionStart = tbInfo.Text.Length;
				tbInfo.ScrollToCaret();
			}
			
		}
		#endregion

	}

}

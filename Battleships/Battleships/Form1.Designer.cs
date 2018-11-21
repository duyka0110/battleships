namespace Battleships
{
	partial class Form1
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.createBtn = new System.Windows.Forms.Button();
			this.attackBtn = new System.Windows.Forms.Button();
			this.readyBtn = new System.Windows.Forms.Button();
			this.findBtn = new System.Windows.Forms.Button();
			this.tbInfo = new System.Windows.Forms.RichTextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.tbIP = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// createBtn
			// 
			this.createBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.createBtn.Location = new System.Drawing.Point(805, 583);
			this.createBtn.Name = "createBtn";
			this.createBtn.Size = new System.Drawing.Size(151, 60);
			this.createBtn.TabIndex = 0;
			this.createBtn.Text = "Create Game";
			this.createBtn.UseVisualStyleBackColor = true;
			this.createBtn.Click += new System.EventHandler(this.createBtn_Click);
			// 
			// attackBtn
			// 
			this.attackBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.attackBtn.Location = new System.Drawing.Point(564, 583);
			this.attackBtn.Name = "attackBtn";
			this.attackBtn.Size = new System.Drawing.Size(151, 60);
			this.attackBtn.TabIndex = 0;
			this.attackBtn.Text = "ATTACK [pos]";
			this.attackBtn.UseVisualStyleBackColor = true;
			// 
			// readyBtn
			// 
			this.readyBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.readyBtn.Location = new System.Drawing.Point(564, 512);
			this.readyBtn.Name = "readyBtn";
			this.readyBtn.Size = new System.Drawing.Size(151, 60);
			this.readyBtn.TabIndex = 0;
			this.readyBtn.Text = "READY";
			this.readyBtn.UseVisualStyleBackColor = true;
			this.readyBtn.Click += new System.EventHandler(this.readyBtn_Click);
			// 
			// findBtn
			// 
			this.findBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.findBtn.Location = new System.Drawing.Point(805, 512);
			this.findBtn.Name = "findBtn";
			this.findBtn.Size = new System.Drawing.Size(151, 60);
			this.findBtn.TabIndex = 0;
			this.findBtn.Text = "Find Game";
			this.findBtn.UseVisualStyleBackColor = true;
			// 
			// tbInfo
			// 
			this.tbInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tbInfo.Location = new System.Drawing.Point(20, 512);
			this.tbInfo.Name = "tbInfo";
			this.tbInfo.ReadOnly = true;
			this.tbInfo.Size = new System.Drawing.Size(450, 133);
			this.tbInfo.TabIndex = 1;
			this.tbInfo.Text = "";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(186, 475);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(123, 22);
			this.label1.TabIndex = 2;
			this.label1.Text = "ATTACKING";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(706, 475);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(116, 22);
			this.label2.TabIndex = 2;
			this.label2.Text = "ATTACKED";
			// 
			// tbIP
			// 
			this.tbIP.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tbIP.Location = new System.Drawing.Point(805, 623);
			this.tbIP.Name = "tbIP";
			this.tbIP.Size = new System.Drawing.Size(151, 24);
			this.tbIP.TabIndex = 3;
			this.tbIP.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbIP_KeyDown);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1004, 661);
			this.Controls.Add(this.tbIP);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.tbInfo);
			this.Controls.Add(this.findBtn);
			this.Controls.Add(this.readyBtn);
			this.Controls.Add(this.attackBtn);
			this.Controls.Add(this.createBtn);
			this.KeyPreview = true;
			this.Name = "Form1";
			this.Text = "Battleships";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.DrawBoard);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form_OnO);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button createBtn;
		private System.Windows.Forms.Button attackBtn;
		private System.Windows.Forms.Button readyBtn;
		private System.Windows.Forms.Button findBtn;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox tbIP;
		private System.Windows.Forms.RichTextBox tbInfo;
	}
}


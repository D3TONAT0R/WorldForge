using System.ComponentModel;

namespace NetheriteFinder
{
	partial class MainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			canvas = new NetheriteFinder.CanvasPanel();
			toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			toolStrip1 = new System.Windows.Forms.ToolStrip();
			loadChunks = new System.Windows.Forms.ToolStripButton();
			reload = new System.Windows.Forms.ToolStripButton();
			toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
			yMinControl = new NetheriteFinder.NumericUpDownToolStripControl();
			yMaxControl = new NetheriteFinder.NumericUpDownToolStripControl();
			toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			zoomIn = new System.Windows.Forms.ToolStripButton();
			zoomOut = new System.Windows.Forms.ToolStripButton();
			zoomControl = new NetheriteFinder.NumericUpDownToolStripControl();
			toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			toolStripLabel3 = new System.Windows.Forms.ToolStripLabel();
			toolStripLabel4 = new System.Windows.Forms.ToolStripLabel();
			playerXControl = new NetheriteFinder.NumericUpDownToolStripControl();
			toolStripLabel5 = new System.Windows.Forms.ToolStripLabel();
			playerZControl = new NetheriteFinder.NumericUpDownToolStripControl();
			toolStripLabel6 = new System.Windows.Forms.ToolStripLabel();
			playerYControl = new NetheriteFinder.NumericUpDownToolStripControl();
			toolStripLabel7 = new System.Windows.Forms.ToolStripLabel();
			playerYawControl = new NetheriteFinder.NumericUpDownToolStripControl();
			toolStrip1.SuspendLayout();
			SuspendLayout();
			// 
			// canvas
			// 
			canvas.BackColor = System.Drawing.SystemColors.AppWorkspace;
			canvas.Dock = System.Windows.Forms.DockStyle.Fill;
			canvas.Location = new System.Drawing.Point(0, 26);
			canvas.Name = "canvas";
			canvas.Size = new System.Drawing.Size(800, 424);
			canvas.TabIndex = 1;
			canvas.Paint += OnDraw;
			// 
			// toolStripMenuItem1
			// 
			toolStripMenuItem1.Name = "toolStripMenuItem1";
			toolStripMenuItem1.Size = new System.Drawing.Size(32, 19);
			// 
			// toolStrip1
			// 
			toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { loadChunks, reload, toolStripSeparator2, toolStripLabel1, yMinControl, yMaxControl, toolStripSeparator1, zoomIn, zoomOut, zoomControl, toolStripSeparator3, toolStripLabel3, toolStripLabel4, playerXControl, toolStripLabel5, playerZControl, toolStripLabel6, playerYControl, toolStripLabel7, playerYawControl });
			toolStrip1.Location = new System.Drawing.Point(0, 0);
			toolStrip1.Name = "toolStrip1";
			toolStrip1.Size = new System.Drawing.Size(800, 26);
			toolStrip1.TabIndex = 1;
			toolStrip1.Text = "toolStrip1";
			toolStrip1.MouseEnter += FocusStrip;
			// 
			// loadChunks
			// 
			loadChunks.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			loadChunks.Image = ((System.Drawing.Image)resources.GetObject("loadChunks.Image"));
			loadChunks.ImageTransparentColor = System.Drawing.Color.Magenta;
			loadChunks.Name = "loadChunks";
			loadChunks.Size = new System.Drawing.Size(23, 23);
			loadChunks.Text = "toolStripButton1";
			loadChunks.ToolTipText = "Load chunks from folder ...";
			loadChunks.Click += OnOpenClick;
			// 
			// reload
			// 
			reload.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			reload.Image = ((System.Drawing.Image)resources.GetObject("reload.Image"));
			reload.ImageTransparentColor = System.Drawing.Color.Magenta;
			reload.Name = "reload";
			reload.Size = new System.Drawing.Size(23, 23);
			reload.Text = "toolStripButton2";
			reload.ToolTipText = "Reload chunks";
			reload.Click += OnReloadClick;
			// 
			// toolStripSeparator2
			// 
			toolStripSeparator2.Name = "toolStripSeparator2";
			toolStripSeparator2.Size = new System.Drawing.Size(6, 26);
			// 
			// toolStripLabel1
			// 
			toolStripLabel1.Margin = new System.Windows.Forms.Padding(10, 1, 0, 2);
			toolStripLabel1.Name = "toolStripLabel1";
			toolStripLabel1.Size = new System.Drawing.Size(66, 23);
			toolStripLabel1.Text = "Y Min/Max";
			// 
			// yMinControl
			// 
			yMinControl.Font = new System.Drawing.Font("Segoe UI", 9F);
			yMinControl.Increment = new decimal(new int[] { 1, 0, 0, 0 });
			yMinControl.Maximum = new decimal(new int[] { 320, 0, 0, 0 });
			yMinControl.Minimum = new decimal(new int[] { 64, 0, 0, -2147483648 });
			yMinControl.Name = "yMinControl";
			yMinControl.Size = new System.Drawing.Size(41, 23);
			yMinControl.Text = "12";
			yMinControl.Value = new decimal(new int[] { 12, 0, 0, 0 });
			// 
			// yMaxControl
			// 
			yMaxControl.Increment = new decimal(new int[] { 1, 0, 0, 0 });
			yMaxControl.Maximum = new decimal(new int[] { 320, 0, 0, 0 });
			yMaxControl.Minimum = new decimal(new int[] { 64, 0, 0, -2147483648 });
			yMaxControl.Name = "yMaxControl";
			yMaxControl.Size = new System.Drawing.Size(41, 23);
			yMaxControl.Text = "18";
			yMaxControl.Value = new decimal(new int[] { 18, 0, 0, 0 });
			// 
			// toolStripSeparator1
			// 
			toolStripSeparator1.Name = "toolStripSeparator1";
			toolStripSeparator1.Size = new System.Drawing.Size(6, 26);
			// 
			// zoomIn
			// 
			zoomIn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			zoomIn.Image = ((System.Drawing.Image)resources.GetObject("zoomIn.Image"));
			zoomIn.ImageTransparentColor = System.Drawing.Color.Magenta;
			zoomIn.Name = "zoomIn";
			zoomIn.Size = new System.Drawing.Size(23, 23);
			zoomIn.Text = "toolStripButton3";
			zoomIn.ToolTipText = "Zoom In";
			zoomIn.Click += zoomIn_Click;
			// 
			// zoomOut
			// 
			zoomOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			zoomOut.Image = ((System.Drawing.Image)resources.GetObject("zoomOut.Image"));
			zoomOut.ImageTransparentColor = System.Drawing.Color.Magenta;
			zoomOut.Name = "zoomOut";
			zoomOut.Size = new System.Drawing.Size(23, 23);
			zoomOut.Text = "toolStripButton4";
			zoomOut.ToolTipText = "Zoom Out";
			zoomOut.Click += zoomOut_Click;
			// 
			// zoomControl
			// 
			zoomControl.Increment = new decimal(new int[] { 1, 0, 0, 0 });
			zoomControl.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
			zoomControl.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
			zoomControl.Name = "zoomControl";
			zoomControl.Size = new System.Drawing.Size(35, 23);
			zoomControl.Text = "5";
			zoomControl.Value = new decimal(new int[] { 5, 0, 0, 0 });
			// 
			// toolStripSeparator3
			// 
			toolStripSeparator3.Name = "toolStripSeparator3";
			toolStripSeparator3.Size = new System.Drawing.Size(6, 26);
			// 
			// toolStripLabel3
			// 
			toolStripLabel3.Name = "toolStripLabel3";
			toolStripLabel3.Size = new System.Drawing.Size(39, 23);
			toolStripLabel3.Text = "Player";
			// 
			// toolStripLabel4
			// 
			toolStripLabel4.Name = "toolStripLabel4";
			toolStripLabel4.Size = new System.Drawing.Size(14, 23);
			toolStripLabel4.Text = "X";
			// 
			// playerXControl
			// 
			playerXControl.Increment = new decimal(new int[] { 1, 0, 0, 0 });
			playerXControl.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
			playerXControl.Minimum = new decimal(new int[] { 100000, 0, 0, -2147483648 });
			playerXControl.Name = "playerXControl";
			playerXControl.Size = new System.Drawing.Size(59, 23);
			playerXControl.Text = "0";
			playerXControl.Value = new decimal(new int[] { 0, 0, 0, 0 });
			// 
			// toolStripLabel5
			// 
			toolStripLabel5.Name = "toolStripLabel5";
			toolStripLabel5.Size = new System.Drawing.Size(14, 23);
			toolStripLabel5.Text = "Z";
			// 
			// playerZControl
			// 
			playerZControl.Increment = new decimal(new int[] { 1, 0, 0, 0 });
			playerZControl.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
			playerZControl.Minimum = new decimal(new int[] { 100000, 0, 0, -2147483648 });
			playerZControl.Name = "playerZControl";
			playerZControl.Size = new System.Drawing.Size(59, 23);
			playerZControl.Text = "0";
			playerZControl.Value = new decimal(new int[] { 0, 0, 0, 0 });
			// 
			// toolStripLabel6
			// 
			toolStripLabel6.Name = "toolStripLabel6";
			toolStripLabel6.Size = new System.Drawing.Size(14, 23);
			toolStripLabel6.Text = "Y";
			// 
			// playerYControl
			// 
			playerYControl.Increment = new decimal(new int[] { 1, 0, 0, 0 });
			playerYControl.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
			playerYControl.Minimum = new decimal(new int[] { 1000, 0, 0, -2147483648 });
			playerYControl.Name = "playerYControl";
			playerYControl.Size = new System.Drawing.Size(41, 23);
			playerYControl.Text = "0";
			playerYControl.Value = new decimal(new int[] { 0, 0, 0, 0 });
			// 
			// toolStripLabel7
			// 
			toolStripLabel7.Name = "toolStripLabel7";
			toolStripLabel7.Size = new System.Drawing.Size(28, 23);
			toolStripLabel7.Text = "Yaw";
			// 
			// playerYawControl
			// 
			playerYawControl.Increment = new decimal(new int[] { 15, 0, 0, 0 });
			playerYawControl.Maximum = new decimal(new int[] { 360, 0, 0, 0 });
			playerYawControl.Minimum = new decimal(new int[] { 360, 0, 0, -2147483648 });
			playerYawControl.Name = "playerYawControl";
			playerYawControl.Size = new System.Drawing.Size(41, 23);
			playerYawControl.Text = "0";
			playerYawControl.Value = new decimal(new int[] { 0, 0, 0, 0 });
			// 
			// MainForm
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			ClientSize = new System.Drawing.Size(800, 450);
			Controls.Add(canvas);
			Controls.Add(toolStrip1);
			Icon = ((System.Drawing.Icon)resources.GetObject("$this.Icon"));
			MinimumSize = new System.Drawing.Size(400, 300);
			Text = "NetheriteFinder";
			toolStrip1.ResumeLayout(false);
			toolStrip1.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;

		private NetheriteFinder.CanvasPanel canvas;

		#endregion

		private System.Windows.Forms.ToolStrip toolStrip1;
		private ToolStripButton loadChunks;
		private ToolStripButton reload;
		private ToolStripLabel toolStripLabel1;
		private NetheriteFinder.NumericUpDownToolStripControl yMinControl;
		private NetheriteFinder.NumericUpDownToolStripControl yMaxControl;
		private NumericUpDownToolStripControl zoomControl;
		private ToolStripSeparator toolStripSeparator2;
		private ToolStripSeparator toolStripSeparator1;
		private ToolStripButton zoomIn;
		private ToolStripButton zoomOut;
		private ToolStripSeparator toolStripSeparator3;
		private ToolStripLabel toolStripLabel3;
		private ToolStripLabel toolStripLabel4;
		private NetheriteFinder.NumericUpDownToolStripControl playerXControl;
		private ToolStripLabel toolStripLabel5;
		private NetheriteFinder.NumericUpDownToolStripControl playerZControl;
		private ToolStripLabel toolStripLabel6;
		private NetheriteFinder.NumericUpDownToolStripControl playerYControl;
		private ToolStripLabel toolStripLabel7;
		private NetheriteFinder.NumericUpDownToolStripControl playerYawControl;
	}
}
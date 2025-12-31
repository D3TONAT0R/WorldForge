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
			ComponentResourceManager resources = new ComponentResourceManager(typeof(MainForm));
			canvas = new CanvasPanel();
			toolStripMenuItem1 = new ToolStripMenuItem();
			toolStrip1 = new ToolStrip();
			loadChunks = new ToolStripButton();
			reload = new ToolStripButton();
			toolStripSeparator2 = new ToolStripSeparator();
			toolStripLabel1 = new ToolStripLabel();
			yMinControl = new NumericUpDownToolStripControl();
			yMaxControl = new NumericUpDownToolStripControl();
			toolStripSeparator1 = new ToolStripSeparator();
			zoomIn = new ToolStripButton();
			zoomOut = new ToolStripButton();
			zoomControl = new NumericUpDownToolStripControl();
			toolStripSeparator3 = new ToolStripSeparator();
			toolStripLabel3 = new ToolStripLabel();
			toolStripLabel4 = new ToolStripLabel();
			playerXControl = new NumericUpDownToolStripControl();
			toolStripLabel5 = new ToolStripLabel();
			playerZControl = new NumericUpDownToolStripControl();
			toolStripLabel6 = new ToolStripLabel();
			playerYControl = new NumericUpDownToolStripControl();
			toolStripLabel7 = new ToolStripLabel();
			playerYawControl = new NumericUpDownToolStripControl();
			toolStrip1.SuspendLayout();
			SuspendLayout();
			// 
			// canvas
			// 
			canvas.BackColor = SystemColors.AppWorkspace;
			canvas.Dock = DockStyle.Fill;
			canvas.Location = new Point(0, 26);
			canvas.Name = "canvas";
			canvas.Size = new Size(800, 424);
			canvas.TabIndex = 1;
			canvas.Paint += OnDraw;
			// 
			// toolStripMenuItem1
			// 
			toolStripMenuItem1.Name = "toolStripMenuItem1";
			toolStripMenuItem1.Size = new Size(32, 19);
			// 
			// toolStrip1
			// 
			toolStrip1.Items.AddRange(new ToolStripItem[] { loadChunks, reload, toolStripSeparator2, toolStripLabel1, yMinControl, yMaxControl, toolStripSeparator1, zoomIn, zoomOut, zoomControl, toolStripSeparator3, toolStripLabel3, toolStripLabel4, playerXControl, toolStripLabel5, playerZControl, toolStripLabel6, playerYControl, toolStripLabel7, playerYawControl });
			toolStrip1.Location = new Point(0, 0);
			toolStrip1.Name = "toolStrip1";
			toolStrip1.Size = new Size(800, 26);
			toolStrip1.TabIndex = 1;
			toolStrip1.Text = "toolStrip1";
			// 
			// loadChunks
			// 
			loadChunks.DisplayStyle = ToolStripItemDisplayStyle.Image;
			loadChunks.Image = (Image)resources.GetObject("loadChunks.Image");
			loadChunks.ImageTransparentColor = Color.Magenta;
			loadChunks.Name = "loadChunks";
			loadChunks.Size = new Size(23, 23);
			loadChunks.Text = "toolStripButton1";
			loadChunks.ToolTipText = "Load chunks from folder ...";
			loadChunks.Click += OnOpenClick;
			// 
			// reload
			// 
			reload.DisplayStyle = ToolStripItemDisplayStyle.Image;
			reload.Image = (Image)resources.GetObject("reload.Image");
			reload.ImageTransparentColor = Color.Magenta;
			reload.Name = "reload";
			reload.Size = new Size(23, 23);
			reload.Text = "toolStripButton2";
			reload.ToolTipText = "Reload chunks";
			reload.Click += OnReloadClick;
			// 
			// toolStripSeparator2
			// 
			toolStripSeparator2.Name = "toolStripSeparator2";
			toolStripSeparator2.Size = new Size(6, 26);
			// 
			// toolStripLabel1
			// 
			toolStripLabel1.Margin = new Padding(10, 1, 0, 2);
			toolStripLabel1.Name = "toolStripLabel1";
			toolStripLabel1.Size = new Size(66, 23);
			toolStripLabel1.Text = "Y Min/Max";
			// 
			// yMinControl
			// 
			yMinControl.Font = new Font("Segoe UI", 9F);
			yMinControl.Increment = new decimal(new int[] { 1, 0, 0, 0 });
			yMinControl.Maximum = new decimal(new int[] { 320, 0, 0, 0 });
			yMinControl.Minimum = new decimal(new int[] { 64, 0, 0, int.MinValue });
			yMinControl.Name = "yMinControl";
			yMinControl.Size = new Size(41, 23);
			yMinControl.Text = "12";
			yMinControl.Value = new decimal(new int[] { 12, 0, 0, 0 });
			// 
			// yMaxControl
			// 
			yMaxControl.Increment = new decimal(new int[] { 1, 0, 0, 0 });
			yMaxControl.Maximum = new decimal(new int[] { 320, 0, 0, 0 });
			yMaxControl.Minimum = new decimal(new int[] { 64, 0, 0, int.MinValue });
			yMaxControl.Name = "yMaxControl";
			yMaxControl.Size = new Size(41, 23);
			yMaxControl.Text = "18";
			yMaxControl.Value = new decimal(new int[] { 18, 0, 0, 0 });
			// 
			// toolStripSeparator1
			// 
			toolStripSeparator1.Name = "toolStripSeparator1";
			toolStripSeparator1.Size = new Size(6, 26);
			// 
			// zoomIn
			// 
			zoomIn.DisplayStyle = ToolStripItemDisplayStyle.Image;
			zoomIn.Image = (Image)resources.GetObject("zoomIn.Image");
			zoomIn.ImageTransparentColor = Color.Magenta;
			zoomIn.Name = "zoomIn";
			zoomIn.Size = new Size(23, 23);
			zoomIn.Text = "toolStripButton3";
			zoomIn.ToolTipText = "Zoom In";
			zoomIn.Click += zoomIn_Click;
			// 
			// zoomOut
			// 
			zoomOut.DisplayStyle = ToolStripItemDisplayStyle.Image;
			zoomOut.Image = (Image)resources.GetObject("zoomOut.Image");
			zoomOut.ImageTransparentColor = Color.Magenta;
			zoomOut.Name = "zoomOut";
			zoomOut.Size = new Size(23, 23);
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
			zoomControl.Size = new Size(35, 23);
			zoomControl.Text = "5";
			zoomControl.Value = new decimal(new int[] { 5, 0, 0, 0 });
			// 
			// toolStripSeparator3
			// 
			toolStripSeparator3.Name = "toolStripSeparator3";
			toolStripSeparator3.Size = new Size(6, 26);
			// 
			// toolStripLabel3
			// 
			toolStripLabel3.Name = "toolStripLabel3";
			toolStripLabel3.Size = new Size(39, 23);
			toolStripLabel3.Text = "Player";
			// 
			// toolStripLabel4
			// 
			toolStripLabel4.Name = "toolStripLabel4";
			toolStripLabel4.Size = new Size(14, 23);
			toolStripLabel4.Text = "X";
			// 
			// playerXControl
			// 
			playerXControl.Increment = new decimal(new int[] { 1, 0, 0, 0 });
			playerXControl.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
			playerXControl.Minimum = new decimal(new int[] { 10000, 0, 0, int.MinValue });
			playerXControl.Name = "playerXControl";
			playerXControl.Size = new Size(53, 23);
			playerXControl.Text = "0";
			playerXControl.Value = new decimal(new int[] { 0, 0, 0, 0 });
			// 
			// toolStripLabel5
			// 
			toolStripLabel5.Name = "toolStripLabel5";
			toolStripLabel5.Size = new Size(14, 23);
			toolStripLabel5.Text = "Z";
			// 
			// playerZControl
			// 
			playerZControl.Increment = new decimal(new int[] { 1, 0, 0, 0 });
			playerZControl.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
			playerZControl.Minimum = new decimal(new int[] { 10000, 0, 0, int.MinValue });
			playerZControl.Name = "playerZControl";
			playerZControl.Size = new Size(53, 23);
			playerZControl.Text = "0";
			playerZControl.Value = new decimal(new int[] { 0, 0, 0, 0 });
			// 
			// toolStripLabel6
			// 
			toolStripLabel6.Name = "toolStripLabel6";
			toolStripLabel6.Size = new Size(14, 23);
			toolStripLabel6.Text = "Y";
			// 
			// playerYControl
			// 
			playerYControl.Increment = new decimal(new int[] { 1, 0, 0, 0 });
			playerYControl.Maximum = new decimal(new int[] { 320, 0, 0, 0 });
			playerYControl.Minimum = new decimal(new int[] { 64, 0, 0, int.MinValue });
			playerYControl.Name = "playerYControl";
			playerYControl.Size = new Size(41, 23);
			playerYControl.Text = "0";
			playerYControl.Value = new decimal(new int[] { 0, 0, 0, 0 });
			// 
			// toolStripLabel7
			// 
			toolStripLabel7.Name = "toolStripLabel7";
			toolStripLabel7.Size = new Size(28, 23);
			toolStripLabel7.Text = "Yaw";
			// 
			// playerYawControl
			// 
			playerYawControl.Increment = new decimal(new int[] { 15, 0, 0, 0 });
			playerYawControl.Maximum = new decimal(new int[] { 360, 0, 0, 0 });
			playerYawControl.Minimum = new decimal(new int[] { 360, 0, 0, int.MinValue });
			playerYawControl.Name = "playerYawControl";
			playerYawControl.Size = new Size(41, 23);
			playerYawControl.Text = "0";
			playerYawControl.Value = new decimal(new int[] { 0, 0, 0, 0 });
			// 
			// MainForm
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(800, 450);
			Controls.Add(canvas);
			Controls.Add(toolStrip1);
			Icon = (Icon)resources.GetObject("$this.Icon");
			MinimumSize = new Size(400, 300);
			Name = "MainForm";
			Text = "NetheriteFinder";
			toolStrip1.ResumeLayout(false);
			toolStrip1.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;

		private NetheriteFinder.CanvasPanel canvas;

		#endregion

		private ToolStrip toolStrip1;
		private ToolStripButton loadChunks;
		private ToolStripButton reload;
		private ToolStripLabel toolStripLabel1;
		private NumericUpDownToolStripControl yMinControl;
		private NumericUpDownToolStripControl yMaxControl;
		private NumericUpDownToolStripControl zoomControl;
		private ToolStripSeparator toolStripSeparator2;
		private ToolStripSeparator toolStripSeparator1;
		private ToolStripButton zoomIn;
		private ToolStripButton zoomOut;
		private ToolStripSeparator toolStripSeparator3;
		private ToolStripLabel toolStripLabel3;
		private ToolStripLabel toolStripLabel4;
		private NumericUpDownToolStripControl playerXControl;
		private ToolStripLabel toolStripLabel5;
		private NumericUpDownToolStripControl playerZControl;
		private ToolStripLabel toolStripLabel6;
		private NumericUpDownToolStripControl playerYControl;
		private ToolStripLabel toolStripLabel7;
		private NumericUpDownToolStripControl playerYawControl;
	}
}
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
			menuStrip1 = new System.Windows.Forms.MenuStrip();
			fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			canvas = new NetheriteFinder.CanvasPanel();
			yMinControl = new System.Windows.Forms.NumericUpDown();
			toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			label1 = new System.Windows.Forms.Label();
			label2 = new System.Windows.Forms.Label();
			yMaxControl = new System.Windows.Forms.NumericUpDown();
			label3 = new System.Windows.Forms.Label();
			zoomControl = new System.Windows.Forms.NumericUpDown();
			menuStrip1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)yMinControl).BeginInit();
			((System.ComponentModel.ISupportInitialize)yMaxControl).BeginInit();
			((System.ComponentModel.ISupportInitialize)zoomControl).BeginInit();
			SuspendLayout();
			// 
			// menuStrip1
			// 
			menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { fileToolStripMenuItem });
			menuStrip1.Location = new System.Drawing.Point(0, 0);
			menuStrip1.Name = "menuStrip1";
			menuStrip1.Size = new System.Drawing.Size(800, 24);
			menuStrip1.TabIndex = 0;
			menuStrip1.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { openToolStripMenuItem, toolStripSeparator2, exitToolStripMenuItem });
			fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			fileToolStripMenuItem.Text = "&File";
			// 
			// openToolStripMenuItem
			// 
			openToolStripMenuItem.Image = ((System.Drawing.Image)resources.GetObject("openToolStripMenuItem.Image"));
			openToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
			openToolStripMenuItem.Name = "openToolStripMenuItem";
			openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O));
			openToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
			openToolStripMenuItem.Text = "&Open";
			openToolStripMenuItem.Click += OnOpenDialog;
			// 
			// toolStripSeparator2
			// 
			toolStripSeparator2.Name = "toolStripSeparator2";
			toolStripSeparator2.Size = new System.Drawing.Size(143, 6);
			// 
			// exitToolStripMenuItem
			// 
			exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			exitToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
			exitToolStripMenuItem.Text = "E&xit";
			exitToolStripMenuItem.Click += OnExitDialog;
			// 
			// canvas
			// 
			canvas.BackColor = System.Drawing.SystemColors.AppWorkspace;
			canvas.Dock = System.Windows.Forms.DockStyle.Fill;
			canvas.Location = new System.Drawing.Point(0, 24);
			canvas.Name = "canvas";
			canvas.Size = new System.Drawing.Size(800, 426);
			canvas.TabIndex = 1;
			canvas.Paint += OnDraw;
			// 
			// yMinControl
			// 
			yMinControl.Location = new System.Drawing.Point(101, 0);
			yMinControl.Name = "yMinControl";
			yMinControl.Size = new System.Drawing.Size(40, 23);
			yMinControl.TabIndex = 2;
			yMinControl.Value = new decimal(new int[] { 12, 0, 0, 0 });
			yMinControl.ValueChanged += YMin_ValueChanged;
			// 
			// toolStripMenuItem1
			// 
			toolStripMenuItem1.Name = "toolStripMenuItem1";
			toolStripMenuItem1.Size = new System.Drawing.Size(32, 19);
			// 
			// label1
			// 
			label1.Location = new System.Drawing.Point(55, 1);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(40, 23);
			label1.TabIndex = 3;
			label1.Text = "Y Min";
			label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label2
			// 
			label2.Location = new System.Drawing.Point(147, 1);
			label2.Name = "label2";
			label2.Size = new System.Drawing.Size(40, 23);
			label2.TabIndex = 4;
			label2.Text = "Y Max";
			label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// yMaxControl
			// 
			yMaxControl.Location = new System.Drawing.Point(193, 0);
			yMaxControl.Name = "yMaxControl";
			yMaxControl.Size = new System.Drawing.Size(40, 23);
			yMaxControl.TabIndex = 5;
			yMaxControl.Value = new decimal(new int[] { 18, 0, 0, 0 });
			yMaxControl.ValueChanged += YMax_ValueChanged;
			// 
			// label3
			// 
			label3.Location = new System.Drawing.Point(239, 1);
			label3.Name = "label3";
			label3.Size = new System.Drawing.Size(40, 23);
			label3.TabIndex = 6;
			label3.Text = "Zoom";
			label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// zoomControl
			// 
			zoomControl.Location = new System.Drawing.Point(285, 0);
			zoomControl.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
			zoomControl.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
			zoomControl.Name = "zoomControl";
			zoomControl.Size = new System.Drawing.Size(40, 23);
			zoomControl.TabIndex = 7;
			zoomControl.Value = new decimal(new int[] { 5, 0, 0, 0 });
			zoomControl.ValueChanged += Zoom_ValueChanged;
			// 
			// MainForm
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			ClientSize = new System.Drawing.Size(800, 450);
			Controls.Add(zoomControl);
			Controls.Add(label3);
			Controls.Add(yMaxControl);
			Controls.Add(label2);
			Controls.Add(label1);
			Controls.Add(yMinControl);
			Controls.Add(canvas);
			Controls.Add(menuStrip1);
			Text = "MainForm";
			menuStrip1.ResumeLayout(false);
			menuStrip1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)yMinControl).EndInit();
			((System.ComponentModel.ISupportInitialize)yMaxControl).EndInit();
			((System.ComponentModel.ISupportInitialize)zoomControl).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}

		private System.Windows.Forms.NumericUpDown zoomControl;

		private System.Windows.Forms.Label label3;

		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.NumericUpDown yMaxControl;

		private System.Windows.Forms.Label label1;

		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
		private System.Windows.Forms.NumericUpDown yMinControl;

		private NetheriteFinder.CanvasPanel canvas;

		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;

		private System.Windows.Forms.MenuStrip menuStrip1;

		#endregion
	}
}
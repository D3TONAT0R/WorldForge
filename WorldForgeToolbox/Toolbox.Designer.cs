using System.ComponentModel;

namespace WorldForgeToolbox
{
	partial class Toolbox
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
			panel = new FlowLayoutPanel();
			regionViewer = new Button();
			nbtViewer = new Button();
			button1 = new Button();
			panel.SuspendLayout();
			SuspendLayout();
			// 
			// panel
			// 
			panel.Controls.Add(regionViewer);
			panel.Controls.Add(nbtViewer);
			panel.Controls.Add(button1);
			panel.Dock = DockStyle.Fill;
			panel.Location = new Point(0, 0);
			panel.Name = "panel";
			panel.Size = new Size(284, 261);
			panel.TabIndex = 0;
			// 
			// regionViewer
			// 
			regionViewer.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			regionViewer.AutoSize = true;
			regionViewer.Location = new Point(3, 3);
			regionViewer.Name = "regionViewer";
			regionViewer.Size = new Size(200, 25);
			regionViewer.TabIndex = 1;
			regionViewer.Text = "Region Viewer";
			regionViewer.UseVisualStyleBackColor = true;
			regionViewer.Click += OnRegionViewerClick;
			// 
			// nbtViewer
			// 
			nbtViewer.AutoSize = true;
			nbtViewer.Location = new Point(3, 34);
			nbtViewer.Name = "nbtViewer";
			nbtViewer.Size = new Size(200, 25);
			nbtViewer.TabIndex = 0;
			nbtViewer.Text = "NBT Viewer";
			nbtViewer.UseVisualStyleBackColor = true;
			nbtViewer.Click += OnNBTViewerClick;
			// 
			// button1
			// 
			button1.AutoSize = true;
			button1.Location = new Point(3, 65);
			button1.Name = "button1";
			button1.Size = new Size(200, 25);
			button1.TabIndex = 2;
			button1.Text = "World Viewer";
			button1.UseVisualStyleBackColor = true;
			button1.Click += OnWorldViewerClick;
			// 
			// Toolbox
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(284, 261);
			Controls.Add(panel);
			Name = "Toolbox";
			Text = "Toolbox";
			panel.ResumeLayout(false);
			panel.PerformLayout();
			ResumeLayout(false);
		}

		private System.Windows.Forms.FlowLayoutPanel panel;
		private System.Windows.Forms.Button nbtViewer;
		private System.Windows.Forms.Button regionViewer;

		#endregion

		private Button button1;
	}
}
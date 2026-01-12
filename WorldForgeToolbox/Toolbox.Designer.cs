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
			panel = new System.Windows.Forms.FlowLayoutPanel();
			regionViewer = new System.Windows.Forms.Button();
			nbtViewer = new System.Windows.Forms.Button();
			panel.SuspendLayout();
			SuspendLayout();
			// 
			// panel
			// 
			panel.Controls.Add(regionViewer);
			panel.Controls.Add(nbtViewer);
			panel.Dock = System.Windows.Forms.DockStyle.Fill;
			panel.Location = new System.Drawing.Point(0, 0);
			panel.Name = "panel";
			panel.Size = new System.Drawing.Size(284, 261);
			panel.TabIndex = 0;
			// 
			// regionViewer
			// 
			regionViewer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
			regionViewer.AutoSize = true;
			regionViewer.Location = new System.Drawing.Point(3, 3);
			regionViewer.Name = "regionViewer";
			regionViewer.Size = new System.Drawing.Size(200, 25);
			regionViewer.TabIndex = 1;
			regionViewer.Text = "Region Viewer";
			regionViewer.UseVisualStyleBackColor = true;
			regionViewer.Click += OnRegionViewerClick;
			// 
			// nbtViewer
			// 
			nbtViewer.AutoSize = true;
			nbtViewer.Location = new System.Drawing.Point(3, 34);
			nbtViewer.Name = "nbtViewer";
			nbtViewer.Size = new System.Drawing.Size(200, 25);
			nbtViewer.TabIndex = 0;
			nbtViewer.Text = "NBT Viewer";
			nbtViewer.UseVisualStyleBackColor = true;
			nbtViewer.Click += OnNBTViewerClick;
			// 
			// Toolbox
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			ClientSize = new System.Drawing.Size(284, 261);
			Controls.Add(panel);
			Text = "Toolbox";
			panel.ResumeLayout(false);
			panel.PerformLayout();
			ResumeLayout(false);
		}

		private System.Windows.Forms.FlowLayoutPanel panel;
		private System.Windows.Forms.Button nbtViewer;
		private System.Windows.Forms.Button regionViewer;

		#endregion
	}
}
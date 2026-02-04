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
			regionViewer = new Button();
			nbtViewer = new Button();
			worldViewer = new Button();
			blockDistributionViewer = new Button();
			playerDataViewer = new Button();
			SuspendLayout();
			// 
			// regionViewer
			// 
			regionViewer.Dock = DockStyle.Top;
			regionViewer.Location = new Point(0, 50);
			regionViewer.Name = "regionViewer";
			regionViewer.Size = new Size(244, 25);
			regionViewer.TabIndex = 1;
			regionViewer.Text = "Region Viewer";
			regionViewer.UseVisualStyleBackColor = true;
			regionViewer.Click += OnRegionViewerClick;
			// 
			// nbtViewer
			// 
			nbtViewer.AutoSize = true;
			nbtViewer.Dock = DockStyle.Top;
			nbtViewer.Location = new Point(0, 0);
			nbtViewer.Name = "nbtViewer";
			nbtViewer.Size = new Size(244, 25);
			nbtViewer.TabIndex = 0;
			nbtViewer.Text = "NBT Viewer";
			nbtViewer.UseVisualStyleBackColor = true;
			nbtViewer.Click += OnNBTViewerClick;
			// 
			// worldViewer
			// 
			worldViewer.AutoSize = true;
			worldViewer.Dock = DockStyle.Top;
			worldViewer.Location = new Point(0, 75);
			worldViewer.Margin = new Padding(5);
			worldViewer.Name = "worldViewer";
			worldViewer.Size = new Size(244, 25);
			worldViewer.TabIndex = 2;
			worldViewer.Text = "World Viewer";
			worldViewer.UseVisualStyleBackColor = true;
			worldViewer.Click += OnWorldViewerClick;
			// 
			// blockDistributionViewer
			// 
			blockDistributionViewer.AutoSize = true;
			blockDistributionViewer.Dock = DockStyle.Top;
			blockDistributionViewer.Location = new Point(0, 100);
			blockDistributionViewer.Margin = new Padding(5);
			blockDistributionViewer.Name = "blockDistributionViewer";
			blockDistributionViewer.Size = new Size(244, 25);
			blockDistributionViewer.TabIndex = 3;
			blockDistributionViewer.Text = "Block Distribution";
			blockDistributionViewer.UseVisualStyleBackColor = true;
			blockDistributionViewer.Click += OnBlockDistributionClick;
			// 
			// playerDataViewer
			// 
			playerDataViewer.AutoSize = true;
			playerDataViewer.Dock = DockStyle.Top;
			playerDataViewer.Location = new Point(0, 25);
			playerDataViewer.Name = "playerDataViewer";
			playerDataViewer.Size = new Size(244, 25);
			playerDataViewer.TabIndex = 4;
			playerDataViewer.Text = "Player Data Viewer";
			playerDataViewer.UseVisualStyleBackColor = true;
			playerDataViewer.Click += OnPlayerDataViewerClick;
			// 
			// Toolbox
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			AutoSize = true;
			ClientSize = new Size(244, 128);
			Controls.Add(blockDistributionViewer);
			Controls.Add(worldViewer);
			Controls.Add(regionViewer);
			Controls.Add(playerDataViewer);
			Controls.Add(nbtViewer);
			FormBorderStyle = FormBorderStyle.FixedSingle;
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "Toolbox";
			Text = "Toolbox";
			ResumeLayout(false);
			PerformLayout();
		}
		private System.Windows.Forms.Button nbtViewer;
		private System.Windows.Forms.Button regionViewer;

		#endregion

		private Button worldViewer;
        private Button blockDistributionViewer;
		private Button playerDataViewer;
	}
}
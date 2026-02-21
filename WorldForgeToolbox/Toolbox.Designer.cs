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
			oreFinder = new Button();
			mapPaletteGenerator = new Button();
			SuspendLayout();
			// 
			// regionViewer
			// 
			regionViewer.Dock = DockStyle.Top;
			regionViewer.Location = new Point(0, 50);
			regionViewer.Name = "regionViewer";
			regionViewer.Size = new Size(234, 25);
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
			nbtViewer.Size = new Size(234, 25);
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
			worldViewer.Size = new Size(234, 25);
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
			blockDistributionViewer.Size = new Size(234, 25);
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
			playerDataViewer.Size = new Size(234, 25);
			playerDataViewer.TabIndex = 4;
			playerDataViewer.Text = "Player Data Viewer";
			playerDataViewer.UseVisualStyleBackColor = true;
			playerDataViewer.Click += OnPlayerDataViewerClick;
			// 
			// oreFinder
			// 
			oreFinder.AutoSize = true;
			oreFinder.Dock = DockStyle.Top;
			oreFinder.Location = new Point(0, 125);
			oreFinder.Margin = new Padding(5);
			oreFinder.Name = "oreFinder";
			oreFinder.Size = new Size(234, 25);
			oreFinder.TabIndex = 5;
			oreFinder.Text = "Ore Finder";
			oreFinder.UseVisualStyleBackColor = true;
			oreFinder.Click += OnOreFinderClick;
			// 
			// mapPaletteGenerator
			// 
			mapPaletteGenerator.AutoSize = true;
			mapPaletteGenerator.Dock = DockStyle.Top;
			mapPaletteGenerator.Location = new Point(0, 150);
			mapPaletteGenerator.Margin = new Padding(5);
			mapPaletteGenerator.Name = "mapPaletteGenerator";
			mapPaletteGenerator.Size = new Size(234, 25);
			mapPaletteGenerator.TabIndex = 6;
			mapPaletteGenerator.Text = "Map Palette Generator";
			mapPaletteGenerator.UseVisualStyleBackColor = true;
			mapPaletteGenerator.Click += OnMapPaletteGeneratorClick;
			// 
			// Toolbox
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(234, 191);
			Controls.Add(mapPaletteGenerator);
			Controls.Add(oreFinder);
			Controls.Add(blockDistributionViewer);
			Controls.Add(worldViewer);
			Controls.Add(regionViewer);
			Controls.Add(playerDataViewer);
			Controls.Add(nbtViewer);
			FormBorderStyle = FormBorderStyle.FixedSingle;
			MaximizeBox = false;
			MinimizeBox = false;
			MinimumSize = new Size(250, 0);
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
		private Button oreFinder;
		private Button mapPaletteGenerator;
	}
}
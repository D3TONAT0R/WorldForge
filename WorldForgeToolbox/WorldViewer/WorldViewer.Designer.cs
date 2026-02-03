namespace WorldForgeToolbox
{
	partial class WorldViewer
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if(disposing && (components != null))
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WorldViewer));
			toolStrip1 = new ToolStrip();
			toolStripButton1 = new ToolStripButton();
			toolStripSeparator1 = new ToolStripSeparator();
			dimensionSelector = new ToolStripSplitButton();
			toolStripSeparator2 = new ToolStripSeparator();
			resumeMapRender = new ToolStripButton();
			pauseMapRender = new ToolStripButton();
			toolStripSeparator3 = new ToolStripSeparator();
			saveMapCacheButton = new ToolStripButton();
			deleteMapCacheButton = new ToolStripButton();
			toolStripSeparator4 = new ToolStripSeparator();
			zoomIn = new ToolStripButton();
			zoomOut = new ToolStripButton();
			jumpToSpawn = new ToolStripButton();
			canvas = new CanvasPanel();
			statusStrip1 = new StatusStrip();
			statusLabel = new ToolStripStatusLabel();
			toolboxButton = new ToolStripButton();
			toolStripSeparator5 = new ToolStripSeparator();
			toolStrip1.SuspendLayout();
			statusStrip1.SuspendLayout();
			SuspendLayout();
			// 
			// toolStrip1
			// 
			toolStrip1.Items.AddRange(new ToolStripItem[] { toolboxButton, toolStripSeparator5, toolStripButton1, toolStripSeparator1, dimensionSelector, toolStripSeparator2, resumeMapRender, pauseMapRender, toolStripSeparator3, saveMapCacheButton, deleteMapCacheButton, toolStripSeparator4, zoomIn, zoomOut, jumpToSpawn });
			toolStrip1.Location = new Point(0, 0);
			toolStrip1.Name = "toolStrip1";
			toolStrip1.Size = new Size(800, 25);
			toolStrip1.TabIndex = 0;
			toolStrip1.Text = "toolStrip";
			// 
			// toolStripButton1
			// 
			toolStripButton1.DisplayStyle = ToolStripItemDisplayStyle.Image;
			toolStripButton1.Image = (Image)resources.GetObject("toolStripButton1.Image");
			toolStripButton1.ImageTransparentColor = Color.Magenta;
			toolStripButton1.Name = "toolStripButton1";
			toolStripButton1.Size = new Size(23, 22);
			toolStripButton1.Text = "Open World ...";
			toolStripButton1.Click += OnOpenWorldClick;
			// 
			// toolStripSeparator1
			// 
			toolStripSeparator1.Name = "toolStripSeparator1";
			toolStripSeparator1.Size = new Size(6, 25);
			// 
			// dimensionSelector
			// 
			dimensionSelector.AutoSize = false;
			dimensionSelector.DisplayStyle = ToolStripItemDisplayStyle.Text;
			dimensionSelector.Image = (Image)resources.GetObject("dimensionSelector.Image");
			dimensionSelector.ImageTransparentColor = Color.Magenta;
			dimensionSelector.Name = "dimensionSelector";
			dimensionSelector.Size = new Size(100, 22);
			dimensionSelector.Text = "Overworld";
			dimensionSelector.TextAlign = ContentAlignment.MiddleLeft;
			dimensionSelector.DropDownItemClicked += OnDimensionSelect;
			// 
			// toolStripSeparator2
			// 
			toolStripSeparator2.Name = "toolStripSeparator2";
			toolStripSeparator2.Size = new Size(6, 25);
			// 
			// resumeMapRender
			// 
			resumeMapRender.DisplayStyle = ToolStripItemDisplayStyle.Image;
			resumeMapRender.Enabled = false;
			resumeMapRender.Image = (Image)resources.GetObject("resumeMapRender.Image");
			resumeMapRender.ImageTransparentColor = Color.Magenta;
			resumeMapRender.Name = "resumeMapRender";
			resumeMapRender.Size = new Size(23, 22);
			resumeMapRender.Text = "Resume Map Rendering";
			resumeMapRender.Click += resumeMapRender_Click;
			// 
			// pauseMapRender
			// 
			pauseMapRender.DisplayStyle = ToolStripItemDisplayStyle.Image;
			pauseMapRender.Image = (Image)resources.GetObject("pauseMapRender.Image");
			pauseMapRender.ImageTransparentColor = Color.Magenta;
			pauseMapRender.Name = "pauseMapRender";
			pauseMapRender.Size = new Size(23, 22);
			pauseMapRender.Text = "Pause Map Rendering";
			pauseMapRender.Click += pauseMapRender_Click;
			// 
			// toolStripSeparator3
			// 
			toolStripSeparator3.Name = "toolStripSeparator3";
			toolStripSeparator3.Size = new Size(6, 25);
			// 
			// saveMapCacheButton
			// 
			saveMapCacheButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
			saveMapCacheButton.Image = (Image)resources.GetObject("saveMapCacheButton.Image");
			saveMapCacheButton.ImageTransparentColor = Color.Magenta;
			saveMapCacheButton.Name = "saveMapCacheButton";
			saveMapCacheButton.Size = new Size(23, 22);
			saveMapCacheButton.Text = "Save Map Cache";
			saveMapCacheButton.Click += SaveMapCacheButtonClick;
			// 
			// deleteMapCacheButton
			// 
			deleteMapCacheButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
			deleteMapCacheButton.Image = (Image)resources.GetObject("deleteMapCacheButton.Image");
			deleteMapCacheButton.ImageTransparentColor = Color.Magenta;
			deleteMapCacheButton.Name = "deleteMapCacheButton";
			deleteMapCacheButton.Size = new Size(23, 22);
			deleteMapCacheButton.Text = "Delete Map Cache";
			deleteMapCacheButton.ToolTipText = "Delete Map Cache";
			deleteMapCacheButton.Click += deleteMapCacheButton_Click;
			// 
			// toolStripSeparator4
			// 
			toolStripSeparator4.Name = "toolStripSeparator4";
			toolStripSeparator4.Size = new Size(6, 25);
			// 
			// zoomIn
			// 
			zoomIn.DisplayStyle = ToolStripItemDisplayStyle.Image;
			zoomIn.Image = (Image)resources.GetObject("zoomIn.Image");
			zoomIn.ImageTransparentColor = Color.Magenta;
			zoomIn.Name = "zoomIn";
			zoomIn.Size = new Size(23, 22);
			zoomIn.Text = "Zoom In";
			zoomIn.Click += zoomIn_Click;
			// 
			// zoomOut
			// 
			zoomOut.DisplayStyle = ToolStripItemDisplayStyle.Image;
			zoomOut.Image = (Image)resources.GetObject("zoomOut.Image");
			zoomOut.ImageTransparentColor = Color.Magenta;
			zoomOut.Name = "zoomOut";
			zoomOut.Size = new Size(23, 22);
			zoomOut.Text = "Zoom Out";
			zoomOut.Click += zoomOut_Click;
			// 
			// jumpToSpawn
			// 
			jumpToSpawn.DisplayStyle = ToolStripItemDisplayStyle.Image;
			jumpToSpawn.Image = (Image)resources.GetObject("jumpToSpawn.Image");
			jumpToSpawn.ImageTransparentColor = Color.Magenta;
			jumpToSpawn.Name = "jumpToSpawn";
			jumpToSpawn.Size = new Size(23, 22);
			jumpToSpawn.Text = "Jump to Spawnpoint";
			jumpToSpawn.Click += jumpToSpawn_Click;
			// 
			// canvas
			// 
			canvas.BackColor = SystemColors.AppWorkspace;
			canvas.Dock = DockStyle.Fill;
			canvas.Location = new Point(0, 25);
			canvas.Name = "canvas";
			canvas.Size = new Size(800, 403);
			canvas.TabIndex = 1;
			canvas.Paint += OnDraw;
			// 
			// statusStrip1
			// 
			statusStrip1.Items.AddRange(new ToolStripItem[] { statusLabel });
			statusStrip1.Location = new Point(0, 428);
			statusStrip1.Name = "statusStrip1";
			statusStrip1.Size = new Size(800, 22);
			statusStrip1.TabIndex = 0;
			statusStrip1.Text = "statusStrip1";
			// 
			// statusLabel
			// 
			statusLabel.AutoSize = false;
			statusLabel.Name = "statusLabel";
			statusLabel.Size = new Size(200, 17);
			statusLabel.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// toolboxButton
			// 
			toolboxButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
			toolboxButton.Image = (Image)resources.GetObject("toolboxButton.Image");
			toolboxButton.ImageTransparentColor = Color.Magenta;
			toolboxButton.Name = "toolboxButton";
			toolboxButton.Size = new Size(23, 22);
			toolboxButton.Text = "Return to Toolbox";
			toolboxButton.Click += toolboxButton_Click;
			// 
			// toolStripSeparator5
			// 
			toolStripSeparator5.Name = "toolStripSeparator5";
			toolStripSeparator5.Size = new Size(6, 25);
			// 
			// WorldViewer
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(800, 450);
			Controls.Add(canvas);
			Controls.Add(statusStrip1);
			Controls.Add(toolStrip1);
			Name = "WorldViewer";
			Text = "WorldViewer";
			toolStrip1.ResumeLayout(false);
			toolStrip1.PerformLayout();
			statusStrip1.ResumeLayout(false);
			statusStrip1.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private ToolStrip toolStrip1;
		private WorldForgeToolbox.CanvasPanel canvas;
		private ToolStripButton toolStripButton1;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel statusLabel;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripSplitButton dimensionSelector;
		private ToolStripSeparator toolStripSeparator2;
		private ToolStripButton resumeMapRender;
		private ToolStripButton pauseMapRender;
		private ToolStripSeparator toolStripSeparator3;
		private ToolStripButton deleteMapCacheButton;
		private ToolStripButton saveMapCacheButton;
		private ToolStripSeparator toolStripSeparator4;
		private ToolStripButton zoomIn;
		private ToolStripButton zoomOut;
		private ToolStripButton jumpToSpawn;
		private ToolStripButton toolboxButton;
		private ToolStripSeparator toolStripSeparator5;
	}
}
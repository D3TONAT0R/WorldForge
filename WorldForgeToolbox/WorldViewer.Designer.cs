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
            canvas = new CanvasPanel();
            statusStrip1 = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            toolStrip1.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // toolStrip1
            // 
            toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripButton1, toolStripSeparator1, dimensionSelector });
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
    }
}
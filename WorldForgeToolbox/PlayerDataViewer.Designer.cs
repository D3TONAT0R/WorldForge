namespace WorldForgeToolbox
{
	partial class PlayerDataViewer
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PlayerDataViewer));
			splitContainer1 = new SplitContainer();
			nbtView = new NBTTreeView();
			canvas = new CanvasPanel();
			toolStrip1 = new ToolStrip();
			toolboxButton = new ToolStripButton();
			((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
			splitContainer1.Panel1.SuspendLayout();
			splitContainer1.Panel2.SuspendLayout();
			splitContainer1.SuspendLayout();
			toolStrip1.SuspendLayout();
			SuspendLayout();
			// 
			// splitContainer1
			// 
			splitContainer1.Dock = DockStyle.Fill;
			splitContainer1.Location = new Point(0, 25);
			splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			splitContainer1.Panel1.Controls.Add(nbtView);
			// 
			// splitContainer1.Panel2
			// 
			splitContainer1.Panel2.Controls.Add(canvas);
			splitContainer1.Size = new Size(800, 425);
			splitContainer1.SplitterDistance = 266;
			splitContainer1.TabIndex = 0;
			// 
			// nbtView
			// 
			nbtView.Dock = DockStyle.Fill;
			nbtView.Location = new Point(0, 0);
			nbtView.Name = "nbtView";
			nbtView.Size = new Size(266, 425);
			nbtView.TabIndex = 0;
			// 
			// canvas
			// 
			canvas.Dock = DockStyle.Fill;
			canvas.Location = new Point(0, 0);
			canvas.Name = "canvas";
			canvas.Size = new Size(530, 425);
			canvas.TabIndex = 0;
			// 
			// toolStrip1
			// 
			toolStrip1.Items.AddRange(new ToolStripItem[] { toolboxButton });
			toolStrip1.Location = new Point(0, 0);
			toolStrip1.Name = "toolStrip1";
			toolStrip1.Size = new Size(800, 25);
			toolStrip1.TabIndex = 0;
			toolStrip1.Text = "toolStrip1";
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
			// PlayerDataViewer
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(800, 450);
			Controls.Add(splitContainer1);
			Controls.Add(toolStrip1);
			Name = "PlayerDataViewer";
			Text = "PlayerDataViewer";
			splitContainer1.Panel1.ResumeLayout(false);
			splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
			splitContainer1.ResumeLayout(false);
			toolStrip1.ResumeLayout(false);
			toolStrip1.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private SplitContainer splitContainer1;
		private NBTTreeView nbtView;
		private CanvasPanel canvas;
		private ToolStrip toolStrip1;
		private ToolStripButton toolboxButton;
	}
}
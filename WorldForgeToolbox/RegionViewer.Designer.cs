namespace WorldForgeToolbox;

partial class RegionViewer
{
	/// <summary>
	///  Required designer variable.
	/// </summary>
	private System.ComponentModel.IContainer components = null;

	/// <summary>
	///  Clean up any resources being used.
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RegionViewer));
		viewport = new MapView();
		toolStrip1 = new ToolStrip();
		toolboxButton = new ToolStripButton();
		toolStripSeparator2 = new ToolStripSeparator();
		openFile = new ToolStripButton();
		toolStripSeparator1 = new ToolStripSeparator();
		scrollNorth = new ToolStripButton();
		scrollSouth = new ToolStripButton();
		scrollWest = new ToolStripButton();
		scrollEast = new ToolStripButton();
		toolStripSeparator3 = new ToolStripSeparator();
		toggleGrid = new ToolStripButton();
		resetView = new ToolStripButton();
		toolStrip1.SuspendLayout();
		SuspendLayout();
		// 
		// canvas
		// 
		viewport.AllowInteractions = true;
		viewport.AllowPanning = true;
		viewport.AllowZooming = true;
		viewport.AutoSize = true;
		viewport.BackColor = SystemColors.AppWorkspace;
		viewport.Dock = DockStyle.Fill;
		viewport.LabelShadow = true;
		viewport.Location = new Point(0, 25);
		viewport.Margin = new Padding(0);
		viewport.MaxZoom = 4;
		viewport.MinimumSize = new Size(512, 512);
		viewport.MinZoom = 1;
		viewport.Name = "viewport";
		viewport.Size = new Size(609, 535);
		viewport.TabIndex = 0;
		viewport.UnitScale = 1F;
		viewport.Zoom = 1;
		viewport.Paint += Draw;
		viewport.DoubleClick += OnViewportDoubleClick;
		viewport.MouseLeave += OnMouseExit;
		viewport.MouseMove += OnMouseMove;
		// 
		// toolStrip1
		// 
		toolStrip1.Items.AddRange(new ToolStripItem[] { toolboxButton, toolStripSeparator2, openFile, toolStripSeparator1, scrollNorth, scrollSouth, scrollWest, scrollEast, toolStripSeparator3, toggleGrid, resetView });
		toolStrip1.Location = new Point(0, 0);
		toolStrip1.Name = "toolStrip1";
		toolStrip1.Size = new Size(609, 25);
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
		toolboxButton.ToolTipText = "Return to Toolbox";
		toolboxButton.Click += toolboxButton_Click;
		// 
		// toolStripSeparator2
		// 
		toolStripSeparator2.Name = "toolStripSeparator2";
		toolStripSeparator2.Size = new Size(6, 25);
		// 
		// openFile
		// 
		openFile.DisplayStyle = ToolStripItemDisplayStyle.Image;
		openFile.Image = (Image)resources.GetObject("openFile.Image");
		openFile.ImageTransparentColor = Color.Magenta;
		openFile.Name = "openFile";
		openFile.Size = new Size(23, 22);
		openFile.Text = "Open Region File ...";
		openFile.Click += openFile_Click;
		// 
		// toolStripSeparator1
		// 
		toolStripSeparator1.Name = "toolStripSeparator1";
		toolStripSeparator1.Size = new Size(6, 25);
		// 
		// scrollNorth
		// 
		scrollNorth.DisplayStyle = ToolStripItemDisplayStyle.Image;
		scrollNorth.Image = (Image)resources.GetObject("scrollNorth.Image");
		scrollNorth.ImageTransparentColor = Color.Magenta;
		scrollNorth.Name = "scrollNorth";
		scrollNorth.Size = new Size(23, 22);
		scrollNorth.Text = "Scroll North";
		scrollNorth.Click += scrollNorth_Click;
		// 
		// scrollSouth
		// 
		scrollSouth.DisplayStyle = ToolStripItemDisplayStyle.Image;
		scrollSouth.Image = (Image)resources.GetObject("scrollSouth.Image");
		scrollSouth.ImageTransparentColor = Color.Magenta;
		scrollSouth.Name = "scrollSouth";
		scrollSouth.Size = new Size(23, 22);
		scrollSouth.Text = "Scroll South";
		scrollSouth.Click += scrollSouth_Click;
		// 
		// scrollWest
		// 
		scrollWest.DisplayStyle = ToolStripItemDisplayStyle.Image;
		scrollWest.Image = (Image)resources.GetObject("scrollWest.Image");
		scrollWest.ImageTransparentColor = Color.Magenta;
		scrollWest.Name = "scrollWest";
		scrollWest.Size = new Size(23, 22);
		scrollWest.Text = "Scroll West";
		scrollWest.Click += scrollWest_Click;
		// 
		// scrollEast
		// 
		scrollEast.DisplayStyle = ToolStripItemDisplayStyle.Image;
		scrollEast.Image = (Image)resources.GetObject("scrollEast.Image");
		scrollEast.ImageTransparentColor = Color.Magenta;
		scrollEast.Name = "scrollEast";
		scrollEast.Size = new Size(23, 22);
		scrollEast.Text = "Scroll East";
		scrollEast.Click += scrollEast_Click;
		// 
		// toolStripSeparator3
		// 
		toolStripSeparator3.Name = "toolStripSeparator3";
		toolStripSeparator3.Size = new Size(6, 25);
		// 
		// toggleGrid
		// 
		toggleGrid.DisplayStyle = ToolStripItemDisplayStyle.Image;
		toggleGrid.Image = (Image)resources.GetObject("toggleGrid.Image");
		toggleGrid.ImageTransparentColor = Color.Magenta;
		toggleGrid.Name = "toggleGrid";
		toggleGrid.Size = new Size(23, 22);
		toggleGrid.Text = "Toggle Grid";
		toggleGrid.Click += toggleGrid_Click;
		// 
		// resetView
		// 
		resetView.DisplayStyle = ToolStripItemDisplayStyle.Image;
		resetView.Image = (Image)resources.GetObject("resetView.Image");
		resetView.ImageTransparentColor = Color.Magenta;
		resetView.Name = "resetView";
		resetView.Size = new Size(23, 22);
		resetView.Text = "Reset View";
		resetView.Click += resetView_Click;
		// 
		// RegionViewer
		// 
		AutoScaleDimensions = new SizeF(7F, 15F);
		AutoScaleMode = AutoScaleMode.Font;
		AutoSize = true;
		AutoSizeMode = AutoSizeMode.GrowAndShrink;
		ClientSize = new Size(609, 560);
		Controls.Add(viewport);
		Controls.Add(toolStrip1);
		Name = "RegionViewer";
		Text = "Form1";
		toolStrip1.ResumeLayout(false);
		toolStrip1.PerformLayout();
		ResumeLayout(false);
		PerformLayout();
	}

	private MapView viewport;

    #endregion

    private ToolStrip toolStrip1;
    private ToolStripButton scrollNorth;
    private ToolStripButton scrollSouth;
    private ToolStripButton scrollWest;
    private ToolStripButton scrollEast;
    private ToolStripButton openFile;
    private ToolStripSeparator toolStripSeparator1;
	private ToolStripButton toolboxButton;
	private ToolStripSeparator toolStripSeparator2;
	private ToolStripSeparator toolStripSeparator3;
	private ToolStripButton toggleGrid;
	private ToolStripButton resetView;
}
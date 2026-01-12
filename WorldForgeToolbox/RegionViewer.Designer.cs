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
        canvas = new CanvasPanel();
        toolStrip1 = new ToolStrip();
        openFile = new ToolStripButton();
        scrollNorth = new ToolStripButton();
        scrollSouth = new ToolStripButton();
        scrollWest = new ToolStripButton();
        scrollEast = new ToolStripButton();
        toolStripSeparator1 = new ToolStripSeparator();
        toolStrip1.SuspendLayout();
        SuspendLayout();
        // 
        // canvas
        // 
        canvas.BackColor = SystemColors.AppWorkspace;
        canvas.Location = new Point(0, 25);
        canvas.Margin = new Padding(0);
        canvas.Name = "canvas";
        canvas.Size = new Size(512, 512);
        canvas.TabIndex = 0;
        canvas.Click += OnCanvasClick;
        canvas.Paint += Draw;
        canvas.MouseLeave += OnMouseExit;
        canvas.MouseMove += OnMouseMove;
        // 
        // toolStrip1
        // 
        toolStrip1.Items.AddRange(new ToolStripItem[] { openFile, toolStripSeparator1, scrollNorth, scrollSouth, scrollWest, scrollEast });
        toolStrip1.Location = new Point(0, 0);
        toolStrip1.Name = "toolStrip1";
        toolStrip1.Size = new Size(609, 25);
        toolStrip1.TabIndex = 0;
        toolStrip1.Text = "toolStrip1";
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
        // toolStripSeparator1
        // 
        toolStripSeparator1.Name = "toolStripSeparator1";
        toolStripSeparator1.Size = new Size(6, 25);
        // 
        // Form1
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        ClientSize = new Size(609, 560);
        Controls.Add(toolStrip1);
        Controls.Add(canvas);
        Name = "RegionViewer";
        Text = "Form1";
        toolStrip1.ResumeLayout(false);
        toolStrip1.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    private CanvasPanel canvas;

    #endregion

    private ToolStrip toolStrip1;
    private ToolStripButton scrollNorth;
    private ToolStripButton scrollSouth;
    private ToolStripButton scrollWest;
    private ToolStripButton scrollEast;
    private ToolStripButton openFile;
    private ToolStripSeparator toolStripSeparator1;
}
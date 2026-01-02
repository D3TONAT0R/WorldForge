namespace RegionViewer;

partial class Form1
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
		canvas = new CanvasPanel();
		SuspendLayout();
		// 
		// canvas
		// 
		canvas.BackColor = SystemColors.AppWorkspace;
		canvas.Location = new Point(0, 0);
		canvas.Margin = new Padding(0);
		canvas.Name = "canvas";
		canvas.Size = new Size(512, 512);
		canvas.TabIndex = 0;
		canvas.Click += OnCanvasClick;
		canvas.Paint += Draw;
		canvas.MouseLeave += OnMouseExit;
		canvas.MouseMove += OnMouseMove;
		// 
		// Form1
		// 
		AutoScaleDimensions = new SizeF(7F, 15F);
		AutoScaleMode = AutoScaleMode.Font;
		AutoSize = true;
		AutoSizeMode = AutoSizeMode.GrowAndShrink;
		ClientSize = new Size(609, 560);
		Controls.Add(canvas);
		Name = "Form1";
		Text = "Form1";
		ResumeLayout(false);
	}

	private RegionViewer.CanvasPanel canvas;

	#endregion
}
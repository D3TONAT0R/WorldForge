namespace RegionViewer
{
	partial class ProgressWindow
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
			progressLabel = new Label();
			progressBar = new ProgressBar();
			cancelButton = new Button();
			SuspendLayout();
			// 
			// progressLabel
			// 
			progressLabel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			progressLabel.Location = new Point(10, 12);
			progressLabel.Margin = new Padding(3);
			progressLabel.Name = "progressLabel";
			progressLabel.Size = new Size(272, 32);
			progressLabel.TabIndex = 0;
			progressLabel.Text = "Progress";
			progressLabel.TextAlign = ContentAlignment.MiddleCenter;
			// 
			// progressBar
			// 
			progressBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			progressBar.Location = new Point(10, 50);
			progressBar.Name = "progressBar";
			progressBar.Size = new Size(272, 23);
			progressBar.TabIndex = 1;
			// 
			// cancelButton
			// 
			cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			cancelButton.Location = new Point(10, 79);
			cancelButton.Name = "cancelButton";
			cancelButton.Size = new Size(80, 23);
			cancelButton.TabIndex = 2;
			cancelButton.Text = "Cancel";
			cancelButton.UseVisualStyleBackColor = true;
			cancelButton.Click += cancelButton_Click;
			// 
			// ProgressWindow
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			CancelButton = cancelButton;
			ClientSize = new Size(294, 114);
			ControlBox = false;
			Controls.Add(cancelButton);
			Controls.Add(progressBar);
			Controls.Add(progressLabel);
			FormBorderStyle = FormBorderStyle.FixedDialog;
			Name = "ProgressWindow";
			ResumeLayout(false);
		}

		#endregion

		private Label progressLabel;
		private ProgressBar progressBar;
		private Button cancelButton;
	}
}
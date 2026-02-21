namespace WorldForgeToolbox
{
	partial class MapColorPaletteWizard
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MapColorPaletteWizard));
			toolStrip1 = new ToolStrip();
			returnButton = new ToolStripButton();
			toolStripSeparator1 = new ToolStripSeparator();
			generateFromJar = new ToolStripButton();
			toolStripSeparator2 = new ToolStripSeparator();
			save = new ToolStripButton();
			dataGridView1 = new DataGridView();
			toolStrip1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
			SuspendLayout();
			// 
			// toolStrip1
			// 
			toolStrip1.Items.AddRange(new ToolStripItem[] { returnButton, toolStripSeparator1, generateFromJar, toolStripSeparator2, save });
			toolStrip1.Location = new Point(0, 0);
			toolStrip1.Name = "toolStrip1";
			toolStrip1.Size = new Size(800, 25);
			toolStrip1.TabIndex = 0;
			toolStrip1.Text = "toolStrip1";
			// 
			// returnButton
			// 
			returnButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
			returnButton.Image = (Image)resources.GetObject("returnButton.Image");
			returnButton.ImageTransparentColor = Color.Magenta;
			returnButton.Name = "returnButton";
			returnButton.Size = new Size(23, 22);
			returnButton.Text = "Return to Toolbox";
			returnButton.Click += returnButton_Click;
			// 
			// toolStripSeparator1
			// 
			toolStripSeparator1.Name = "toolStripSeparator1";
			toolStripSeparator1.Size = new Size(6, 25);
			// 
			// generateFromJar
			// 
			generateFromJar.DisplayStyle = ToolStripItemDisplayStyle.Image;
			generateFromJar.Image = WorldForgeToolbox.Resources.JARFile;
			generateFromJar.ImageTransparentColor = Color.Magenta;
			generateFromJar.Name = "generateFromJar";
			generateFromJar.Size = new Size(23, 22);
			generateFromJar.Text = "Generate From JAR ...";
			generateFromJar.Click += generateFromJar_Click;
			// 
			// toolStripSeparator2
			// 
			toolStripSeparator2.Name = "toolStripSeparator2";
			toolStripSeparator2.Size = new Size(6, 25);
			// 
			// save
			// 
			save.DisplayStyle = ToolStripItemDisplayStyle.Image;
			save.Enabled = false;
			save.Image = WorldForgeToolbox.Resources.Save;
			save.ImageTransparentColor = Color.Magenta;
			save.Name = "save";
			save.Size = new Size(23, 22);
			save.Text = "Save Palette";
			save.Click += save_Click;
			// 
			// dataGridView1
			// 
			dataGridView1.BackgroundColor = SystemColors.Control;
			dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			dataGridView1.Dock = DockStyle.Fill;
			dataGridView1.Location = new Point(0, 25);
			dataGridView1.Name = "dataGridView1";
			dataGridView1.Size = new Size(800, 425);
			dataGridView1.TabIndex = 1;
			// 
			// MapColorPaletteWizard
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(800, 450);
			Controls.Add(dataGridView1);
			Controls.Add(toolStrip1);
			Name = "MapColorPaletteWizard";
			Text = "MapColorPaletteWizard";
			toolStrip1.ResumeLayout(false);
			toolStrip1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private ToolStrip toolStrip1;
		private ToolStripButton returnButton;
		private ToolStripSeparator toolStripSeparator1;
		private ToolStripButton generateFromJar;
		private ToolStripButton save;
		private ToolStripSeparator toolStripSeparator2;
		private DataGridView dataGridView1;
	}
}
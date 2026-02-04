namespace WorldForgeToolbox
{
    partial class DistributionViewer
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
			LiveChartsCore.SkiaSharpView.SKCharts.SKDefaultLegend skDefaultLegend3 = new LiveChartsCore.SkiaSharpView.SKCharts.SKDefaultLegend();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DistributionViewer));
			LiveChartsCore.Drawing.Padding padding5 = new LiveChartsCore.Drawing.Padding();
			LiveChartsCore.SkiaSharpView.SKCharts.SKDefaultTooltip skDefaultTooltip3 = new LiveChartsCore.SkiaSharpView.SKCharts.SKDefaultTooltip();
			LiveChartsCore.Drawing.Padding padding6 = new LiveChartsCore.Drawing.Padding();
			chart = new LiveChartsCore.SkiaSharpView.WinForms.CartesianChart();
			toolStrip1 = new ToolStrip();
			openButton = new ToolStripButton();
			toolStripSeparator1 = new ToolStripSeparator();
			filterSplitButton = new ToolStripSplitButton();
			zoomToFitButton = new ToolStripButton();
			toolboxButton = new ToolStripButton();
			toolStripSeparator2 = new ToolStripSeparator();
			toolStrip1.SuspendLayout();
			SuspendLayout();
			// 
			// chart
			// 
			chart.AutoUpdateEnabled = true;
			chart.BackColor = Color.FromArgb(32, 32, 32);
			chart.ChartTheme = null;
			chart.Dock = DockStyle.Fill;
			chart.ForceGPU = false;
			skDefaultLegend3.AnimationsSpeed = TimeSpan.Parse("00:00:00.1500000");
			skDefaultLegend3.Content = null;
			skDefaultLegend3.IsValid = false;
			skDefaultLegend3.Opacity = 1F;
			padding5.Bottom = 0F;
			padding5.Left = 0F;
			padding5.Right = 0F;
			padding5.Top = 0F;
			skDefaultLegend3.Padding = padding5;
			skDefaultLegend3.RemoveOnCompleted = false;
			skDefaultLegend3.RotateTransform = 0F;
			skDefaultLegend3.X = 0F;
			skDefaultLegend3.Y = 0F;
			chart.Legend = skDefaultLegend3;
			chart.Location = new Point(0, 25);
			chart.Margin = new Padding(4, 3, 4, 3);
			chart.MatchAxesScreenDataRatio = false;
			chart.Name = "chart";
			chart.Size = new Size(800, 425);
			chart.TabIndex = 0;
			skDefaultTooltip3.AnimationsSpeed = TimeSpan.Parse("00:00:00.1500000");
			skDefaultTooltip3.Content = null;
			skDefaultTooltip3.IsValid = false;
			skDefaultTooltip3.Opacity = 1F;
			padding6.Bottom = 0F;
			padding6.Left = 0F;
			padding6.Right = 0F;
			padding6.Top = 0F;
			skDefaultTooltip3.Padding = padding6;
			skDefaultTooltip3.RemoveOnCompleted = false;
			skDefaultTooltip3.RotateTransform = 0F;
			skDefaultTooltip3.Wedge = 10;
			skDefaultTooltip3.X = 0F;
			skDefaultTooltip3.Y = 0F;
			chart.Tooltip = skDefaultTooltip3;
			chart.TooltipFindingStrategy = LiveChartsCore.Measure.TooltipFindingStrategy.Automatic;
			chart.UpdaterThrottler = TimeSpan.Parse("00:00:00.0500000");
			// 
			// toolStrip1
			// 
			toolStrip1.Items.AddRange(new ToolStripItem[] { toolboxButton, toolStripSeparator2, openButton, toolStripSeparator1, filterSplitButton, zoomToFitButton });
			toolStrip1.Location = new Point(0, 0);
			toolStrip1.Name = "toolStrip1";
			toolStrip1.Size = new Size(800, 25);
			toolStrip1.TabIndex = 1;
			toolStrip1.Text = "toolStrip1";
			// 
			// openButton
			// 
			openButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
			openButton.Image = (Image)resources.GetObject("openButton.Image");
			openButton.ImageTransparentColor = Color.Magenta;
			openButton.Name = "openButton";
			openButton.Size = new Size(23, 22);
			openButton.Text = "Perform Analysis ...";
			openButton.ToolTipText = "Perform Analysis ...";
			openButton.Click += openButton_Click;
			// 
			// toolStripSeparator1
			// 
			toolStripSeparator1.Name = "toolStripSeparator1";
			toolStripSeparator1.Size = new Size(6, 25);
			// 
			// filterSplitButton
			// 
			filterSplitButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
			filterSplitButton.Image = (Image)resources.GetObject("filterSplitButton.Image");
			filterSplitButton.ImageTransparentColor = Color.Magenta;
			filterSplitButton.Name = "filterSplitButton";
			filterSplitButton.Size = new Size(32, 22);
			filterSplitButton.Text = "toolStripSplitButton1";
			filterSplitButton.ToolTipText = "Filter";
			// 
			// zoomToFitButton
			// 
			zoomToFitButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
			zoomToFitButton.Image = (Image)resources.GetObject("zoomToFitButton.Image");
			zoomToFitButton.ImageTransparentColor = Color.Magenta;
			zoomToFitButton.Name = "zoomToFitButton";
			zoomToFitButton.Size = new Size(23, 22);
			zoomToFitButton.Text = "Zoom to Fit";
			zoomToFitButton.Click += zoomToFitButton_Click;
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
			// toolStripSeparator2
			// 
			toolStripSeparator2.Name = "toolStripSeparator2";
			toolStripSeparator2.Size = new Size(6, 25);
			// 
			// DistributionViewer
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(800, 450);
			Controls.Add(chart);
			Controls.Add(toolStrip1);
			Name = "DistributionViewer";
			Text = "DistributionViewer";
			toolStrip1.ResumeLayout(false);
			toolStrip1.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private LiveChartsCore.SkiaSharpView.WinForms.CartesianChart chart;
        private ToolStrip toolStrip1;
        private ToolStripButton openButton;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripSplitButton filterSplitButton;
        private ToolStripButton zoomToFitButton;
		private ToolStripButton toolboxButton;
		private ToolStripSeparator toolStripSeparator2;
	}
}
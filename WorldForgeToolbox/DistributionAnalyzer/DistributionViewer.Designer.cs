namespace RegionViewer.DistributionAnalyzer
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
			LiveChartsCore.SkiaSharpView.SKCharts.SKDefaultLegend skDefaultLegend1 = new LiveChartsCore.SkiaSharpView.SKCharts.SKDefaultLegend();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DistributionViewer));
			LiveChartsCore.Drawing.Padding padding1 = new LiveChartsCore.Drawing.Padding();
			LiveChartsCore.SkiaSharpView.SKCharts.SKDefaultTooltip skDefaultTooltip1 = new LiveChartsCore.SkiaSharpView.SKCharts.SKDefaultTooltip();
			LiveChartsCore.Drawing.Padding padding2 = new LiveChartsCore.Drawing.Padding();
			chart = new LiveChartsCore.SkiaSharpView.WinForms.CartesianChart();
			toolStrip1 = new ToolStrip();
			openButton = new ToolStripButton();
			toolStripSeparator1 = new ToolStripSeparator();
			filterSplitButton = new ToolStripSplitButton();
			zoomToFitButton = new ToolStripButton();
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
			skDefaultLegend1.AnimationsSpeed = TimeSpan.Parse("00:00:00.1500000");
			skDefaultLegend1.Content = null;
			skDefaultLegend1.IsValid = false;
			skDefaultLegend1.Opacity = 1F;
			padding1.Bottom = 0F;
			padding1.Left = 0F;
			padding1.Right = 0F;
			padding1.Top = 0F;
			skDefaultLegend1.Padding = padding1;
			skDefaultLegend1.RemoveOnCompleted = false;
			skDefaultLegend1.RotateTransform = 0F;
			skDefaultLegend1.X = 0F;
			skDefaultLegend1.Y = 0F;
			chart.Legend = skDefaultLegend1;
			chart.Location = new Point(0, 25);
			chart.Margin = new Padding(4, 3, 4, 3);
			chart.MatchAxesScreenDataRatio = false;
			chart.Name = "chart";
			chart.Size = new Size(800, 425);
			chart.TabIndex = 0;
			skDefaultTooltip1.AnimationsSpeed = TimeSpan.Parse("00:00:00.1500000");
			skDefaultTooltip1.Content = null;
			skDefaultTooltip1.IsValid = false;
			skDefaultTooltip1.Opacity = 1F;
			padding2.Bottom = 0F;
			padding2.Left = 0F;
			padding2.Right = 0F;
			padding2.Top = 0F;
			skDefaultTooltip1.Padding = padding2;
			skDefaultTooltip1.RemoveOnCompleted = false;
			skDefaultTooltip1.RotateTransform = 0F;
			skDefaultTooltip1.Wedge = 10;
			skDefaultTooltip1.X = 0F;
			skDefaultTooltip1.Y = 0F;
			chart.Tooltip = skDefaultTooltip1;
			chart.TooltipFindingStrategy = LiveChartsCore.Measure.TooltipFindingStrategy.Automatic;
			chart.UpdaterThrottler = TimeSpan.Parse("00:00:00.0500000");
			// 
			// toolStrip1
			// 
			toolStrip1.Items.AddRange(new ToolStripItem[] { openButton, toolStripSeparator1, filterSplitButton, zoomToFitButton });
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
    }
}
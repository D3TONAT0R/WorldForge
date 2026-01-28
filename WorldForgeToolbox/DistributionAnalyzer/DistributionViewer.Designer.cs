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
            LiveChartsCore.SkiaSharpView.SKCharts.SKDefaultLegend skDefaultLegend2 = new LiveChartsCore.SkiaSharpView.SKCharts.SKDefaultLegend();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DistributionViewer));
            LiveChartsCore.Drawing.Padding padding3 = new LiveChartsCore.Drawing.Padding();
            LiveChartsCore.SkiaSharpView.SKCharts.SKDefaultTooltip skDefaultTooltip2 = new LiveChartsCore.SkiaSharpView.SKCharts.SKDefaultTooltip();
            LiveChartsCore.Drawing.Padding padding4 = new LiveChartsCore.Drawing.Padding();
            chart = new LiveChartsCore.SkiaSharpView.WinForms.CartesianChart();
            toolStrip1 = new ToolStrip();
            openButton = new ToolStripButton();
            filterSplitButton = new ToolStripSplitButton();
            toolStripSeparator1 = new ToolStripSeparator();
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
            skDefaultLegend2.AnimationsSpeed = TimeSpan.Parse("00:00:00.1500000");
            skDefaultLegend2.Content = null;
            skDefaultLegend2.IsValid = false;
            skDefaultLegend2.Opacity = 1F;
            padding3.Bottom = 0F;
            padding3.Left = 0F;
            padding3.Right = 0F;
            padding3.Top = 0F;
            skDefaultLegend2.Padding = padding3;
            skDefaultLegend2.RemoveOnCompleted = false;
            skDefaultLegend2.RotateTransform = 0F;
            skDefaultLegend2.X = 0F;
            skDefaultLegend2.Y = 0F;
            chart.Legend = skDefaultLegend2;
            chart.Location = new Point(0, 25);
            chart.Margin = new Padding(4, 3, 4, 3);
            chart.MatchAxesScreenDataRatio = false;
            chart.Name = "chart";
            chart.Size = new Size(800, 425);
            chart.TabIndex = 0;
            skDefaultTooltip2.AnimationsSpeed = TimeSpan.Parse("00:00:00.1500000");
            skDefaultTooltip2.Content = null;
            skDefaultTooltip2.IsValid = false;
            skDefaultTooltip2.Opacity = 1F;
            padding4.Bottom = 0F;
            padding4.Left = 0F;
            padding4.Right = 0F;
            padding4.Top = 0F;
            skDefaultTooltip2.Padding = padding4;
            skDefaultTooltip2.RemoveOnCompleted = false;
            skDefaultTooltip2.RotateTransform = 0F;
            skDefaultTooltip2.Wedge = 10;
            skDefaultTooltip2.X = 0F;
            skDefaultTooltip2.Y = 0F;
            chart.Tooltip = skDefaultTooltip2;
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
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 25);
            // 
            // zoomToFitButton
            // 
            zoomToFitButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            zoomToFitButton.Image = (Image)resources.GetObject("zoomToFitButton.Image");
            zoomToFitButton.ImageTransparentColor = Color.Magenta;
            zoomToFitButton.Name = "zoomToFitButton";
            zoomToFitButton.Size = new Size(23, 22);
            zoomToFitButton.Text = "Zoom to Fit";
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
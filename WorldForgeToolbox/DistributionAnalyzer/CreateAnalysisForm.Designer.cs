namespace RegionViewer.DistributionAnalyzer
{
    partial class CreateAnalysisForm
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
			Label label1;
			Label label2;
			Label label3;
			Label label5;
			Label label6;
			Label label7;
			Label label4;
			flowLayoutPanel1 = new FlowLayoutPanel();
			flowLayoutPanel2 = new FlowLayoutPanel();
			filePathTextBox = new TextBox();
			browseButton = new Button();
			flowLayoutPanel5 = new FlowLayoutPanel();
			dimensionComboBox = new ComboBox();
			scanModeComboBox = new ComboBox();
			scanAreaGroupBox = new GroupBox();
			flowLayoutPanel4 = new FlowLayoutPanel();
			scanRadius = new NumericUpDown();
			flowLayoutPanel3 = new FlowLayoutPanel();
			centerXNumeric = new NumericUpDown();
			centerZNumeric = new NumericUpDown();
			scanTypesList = new CheckedListBox();
			cancelButton = new Button();
			tableLayoutPanel1 = new TableLayoutPanel();
			startButton = new Button();
			label1 = new Label();
			label2 = new Label();
			label3 = new Label();
			label5 = new Label();
			label6 = new Label();
			label7 = new Label();
			label4 = new Label();
			flowLayoutPanel1.SuspendLayout();
			flowLayoutPanel2.SuspendLayout();
			flowLayoutPanel5.SuspendLayout();
			scanAreaGroupBox.SuspendLayout();
			flowLayoutPanel4.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)scanRadius).BeginInit();
			flowLayoutPanel3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)centerXNumeric).BeginInit();
			((System.ComponentModel.ISupportInitialize)centerZNumeric).BeginInit();
			tableLayoutPanel1.SuspendLayout();
			SuspendLayout();
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Dock = DockStyle.Top;
			label1.Location = new Point(3, 0);
			label1.Name = "label1";
			label1.Size = new Size(222, 15);
			label1.TabIndex = 0;
			label1.Text = "World Folder";
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Dock = DockStyle.Top;
			label2.Location = new Point(3, 215);
			label2.Name = "label2";
			label2.Size = new Size(222, 15);
			label2.TabIndex = 2;
			label2.Text = "Blocks";
			// 
			// label3
			// 
			label3.AutoSize = true;
			label3.Dock = DockStyle.Top;
			label3.Location = new Point(3, 85);
			label3.Name = "label3";
			label3.Size = new Size(222, 15);
			label3.TabIndex = 3;
			label3.Text = "Scan Mode";
			// 
			// label5
			// 
			label5.Location = new Point(3, 0);
			label5.Name = "label5";
			label5.Size = new Size(15, 23);
			label5.TabIndex = 0;
			label5.Text = "X";
			label5.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// label6
			// 
			label6.Location = new Point(110, 0);
			label6.Name = "label6";
			label6.Size = new Size(15, 23);
			label6.TabIndex = 2;
			label6.Text = "Z";
			label6.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// label7
			// 
			label7.Location = new Point(3, 0);
			label7.Name = "label7";
			label7.Size = new Size(122, 23);
			label7.TabIndex = 0;
			label7.Text = "Radius (Chunks)";
			label7.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// label4
			// 
			label4.Location = new Point(3, 0);
			label4.Name = "label4";
			label4.Size = new Size(80, 23);
			label4.TabIndex = 0;
			label4.Text = "Dimension";
			label4.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// flowLayoutPanel1
			// 
			flowLayoutPanel1.Controls.Add(label1);
			flowLayoutPanel1.Controls.Add(flowLayoutPanel2);
			flowLayoutPanel1.Controls.Add(flowLayoutPanel5);
			flowLayoutPanel1.Controls.Add(label3);
			flowLayoutPanel1.Controls.Add(scanModeComboBox);
			flowLayoutPanel1.Controls.Add(scanAreaGroupBox);
			flowLayoutPanel1.Controls.Add(label2);
			flowLayoutPanel1.Controls.Add(scanTypesList);
			flowLayoutPanel1.Dock = DockStyle.Fill;
			flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
			flowLayoutPanel1.Location = new Point(0, 0);
			flowLayoutPanel1.Name = "flowLayoutPanel1";
			flowLayoutPanel1.Size = new Size(234, 373);
			flowLayoutPanel1.TabIndex = 0;
			// 
			// flowLayoutPanel2
			// 
			flowLayoutPanel2.AutoSize = true;
			flowLayoutPanel2.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			flowLayoutPanel2.Controls.Add(filePathTextBox);
			flowLayoutPanel2.Controls.Add(browseButton);
			flowLayoutPanel2.Dock = DockStyle.Top;
			flowLayoutPanel2.Location = new Point(3, 18);
			flowLayoutPanel2.Name = "flowLayoutPanel2";
			flowLayoutPanel2.Size = new Size(222, 29);
			flowLayoutPanel2.TabIndex = 1;
			// 
			// filePathTextBox
			// 
			filePathTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			filePathTextBox.Location = new Point(3, 3);
			filePathTextBox.Name = "filePathTextBox";
			filePathTextBox.Size = new Size(180, 23);
			filePathTextBox.TabIndex = 0;
			// 
			// browseButton
			// 
			browseButton.Dock = DockStyle.Right;
			browseButton.Location = new Point(189, 3);
			browseButton.Name = "browseButton";
			browseButton.Size = new Size(30, 23);
			browseButton.TabIndex = 1;
			browseButton.Text = "...";
			browseButton.UseVisualStyleBackColor = true;
			browseButton.Click += browseButton_Click;
			// 
			// flowLayoutPanel5
			// 
			flowLayoutPanel5.AutoSize = true;
			flowLayoutPanel5.Controls.Add(label4);
			flowLayoutPanel5.Controls.Add(dimensionComboBox);
			flowLayoutPanel5.Dock = DockStyle.Top;
			flowLayoutPanel5.Location = new Point(3, 53);
			flowLayoutPanel5.Name = "flowLayoutPanel5";
			flowLayoutPanel5.Size = new Size(222, 29);
			flowLayoutPanel5.TabIndex = 8;
			// 
			// dimensionComboBox
			// 
			dimensionComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			dimensionComboBox.FormattingEnabled = true;
			dimensionComboBox.Location = new Point(89, 3);
			dimensionComboBox.Name = "dimensionComboBox";
			dimensionComboBox.Size = new Size(130, 23);
			dimensionComboBox.TabIndex = 1;
			// 
			// scanModeComboBox
			// 
			scanModeComboBox.Dock = DockStyle.Top;
			scanModeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			scanModeComboBox.FormattingEnabled = true;
			scanModeComboBox.Location = new Point(3, 103);
			scanModeComboBox.Name = "scanModeComboBox";
			scanModeComboBox.Size = new Size(222, 23);
			scanModeComboBox.TabIndex = 4;
			// 
			// scanAreaGroupBox
			// 
			scanAreaGroupBox.AutoSize = true;
			scanAreaGroupBox.Controls.Add(flowLayoutPanel4);
			scanAreaGroupBox.Controls.Add(flowLayoutPanel3);
			scanAreaGroupBox.Dock = DockStyle.Top;
			scanAreaGroupBox.Location = new Point(3, 132);
			scanAreaGroupBox.Name = "scanAreaGroupBox";
			scanAreaGroupBox.Size = new Size(222, 80);
			scanAreaGroupBox.TabIndex = 8;
			scanAreaGroupBox.TabStop = false;
			scanAreaGroupBox.Text = "Scan Area";
			// 
			// flowLayoutPanel4
			// 
			flowLayoutPanel4.AutoSize = true;
			flowLayoutPanel4.Controls.Add(label7);
			flowLayoutPanel4.Controls.Add(scanRadius);
			flowLayoutPanel4.Dock = DockStyle.Top;
			flowLayoutPanel4.Location = new Point(3, 48);
			flowLayoutPanel4.Name = "flowLayoutPanel4";
			flowLayoutPanel4.Size = new Size(216, 29);
			flowLayoutPanel4.TabIndex = 7;
			// 
			// scanRadius
			// 
			scanRadius.Increment = new decimal(new int[] { 8, 0, 0, 0 });
			scanRadius.Location = new Point(131, 3);
			scanRadius.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
			scanRadius.Minimum = new decimal(new int[] { 8, 0, 0, 0 });
			scanRadius.Name = "scanRadius";
			scanRadius.Size = new Size(80, 23);
			scanRadius.TabIndex = 1;
			scanRadius.Value = new decimal(new int[] { 32, 0, 0, 0 });
			// 
			// flowLayoutPanel3
			// 
			flowLayoutPanel3.AutoSize = true;
			flowLayoutPanel3.Controls.Add(label5);
			flowLayoutPanel3.Controls.Add(centerXNumeric);
			flowLayoutPanel3.Controls.Add(label6);
			flowLayoutPanel3.Controls.Add(centerZNumeric);
			flowLayoutPanel3.Dock = DockStyle.Top;
			flowLayoutPanel3.Location = new Point(3, 19);
			flowLayoutPanel3.Name = "flowLayoutPanel3";
			flowLayoutPanel3.Size = new Size(216, 29);
			flowLayoutPanel3.TabIndex = 6;
			// 
			// centerXNumeric
			// 
			centerXNumeric.Location = new Point(24, 3);
			centerXNumeric.Name = "centerXNumeric";
			centerXNumeric.Size = new Size(80, 23);
			centerXNumeric.TabIndex = 1;
			// 
			// centerZNumeric
			// 
			centerZNumeric.Location = new Point(131, 3);
			centerZNumeric.Name = "centerZNumeric";
			centerZNumeric.Size = new Size(80, 23);
			centerZNumeric.TabIndex = 3;
			// 
			// scanTypesList
			// 
			scanTypesList.CheckOnClick = true;
			scanTypesList.FormattingEnabled = true;
			scanTypesList.Items.AddRange(new object[] { "a", "b", "c", "d", "e", "f", "g", "h" });
			scanTypesList.Location = new Point(3, 233);
			scanTypesList.Name = "scanTypesList";
			scanTypesList.Size = new Size(222, 94);
			scanTypesList.TabIndex = 2;
			// 
			// cancelButton
			// 
			cancelButton.Dock = DockStyle.Bottom;
			cancelButton.Location = new Point(120, 3);
			cancelButton.Name = "cancelButton";
			cancelButton.Size = new Size(111, 23);
			cancelButton.TabIndex = 10;
			cancelButton.Text = "Cancel";
			cancelButton.UseVisualStyleBackColor = true;
			// 
			// tableLayoutPanel1
			// 
			tableLayoutPanel1.AutoSize = true;
			tableLayoutPanel1.ColumnCount = 2;
			tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
			tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
			tableLayoutPanel1.Controls.Add(cancelButton, 1, 0);
			tableLayoutPanel1.Controls.Add(startButton, 0, 0);
			tableLayoutPanel1.Dock = DockStyle.Bottom;
			tableLayoutPanel1.Location = new Point(0, 344);
			tableLayoutPanel1.Margin = new Padding(0);
			tableLayoutPanel1.Name = "tableLayoutPanel1";
			tableLayoutPanel1.RowCount = 1;
			tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
			tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
			tableLayoutPanel1.Size = new Size(234, 29);
			tableLayoutPanel1.TabIndex = 10;
			// 
			// startButton
			// 
			startButton.Dock = DockStyle.Bottom;
			startButton.Location = new Point(3, 3);
			startButton.Name = "startButton";
			startButton.Size = new Size(111, 23);
			startButton.TabIndex = 9;
			startButton.Text = "Create";
			startButton.UseVisualStyleBackColor = true;
			// 
			// CreateAnalysisForm
			// 
			AcceptButton = startButton;
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			AutoSize = true;
			CancelButton = cancelButton;
			ClientSize = new Size(234, 373);
			ControlBox = false;
			Controls.Add(tableLayoutPanel1);
			Controls.Add(flowLayoutPanel1);
			FormBorderStyle = FormBorderStyle.FixedDialog;
			Name = "CreateAnalysisForm";
			ShowIcon = false;
			ShowInTaskbar = false;
			Text = "CreateAnalysisForm";
			flowLayoutPanel1.ResumeLayout(false);
			flowLayoutPanel1.PerformLayout();
			flowLayoutPanel2.ResumeLayout(false);
			flowLayoutPanel2.PerformLayout();
			flowLayoutPanel5.ResumeLayout(false);
			scanAreaGroupBox.ResumeLayout(false);
			scanAreaGroupBox.PerformLayout();
			flowLayoutPanel4.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)scanRadius).EndInit();
			flowLayoutPanel3.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)centerXNumeric).EndInit();
			((System.ComponentModel.ISupportInitialize)centerZNumeric).EndInit();
			tableLayoutPanel1.ResumeLayout(false);
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private FlowLayoutPanel flowLayoutPanel1;
        private Label label1;
        private FlowLayoutPanel flowLayoutPanel2;
        private TextBox filePathTextBox;
        private Button browseButton;
        private ComboBox scanModeComboBox;
        private CheckedListBox scanTypesList;
        private FlowLayoutPanel flowLayoutPanel3;
        private Label label5;
        private NumericUpDown centerXNumeric;
        private Label label6;
        private NumericUpDown centerZNumeric;
        private GroupBox scanAreaGroupBox;
        private FlowLayoutPanel flowLayoutPanel4;
        private NumericUpDown scanRadius;
        private FlowLayoutPanel flowLayoutPanel5;
        private ComboBox dimensionComboBox;
        private Button cancelButton;
        private TableLayoutPanel tableLayoutPanel1;
        private Button startButton;
    }
}
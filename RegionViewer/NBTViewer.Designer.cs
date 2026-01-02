namespace RegionViewer
{
	partial class NBTViewer
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
			if(disposing && (components != null))
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
			components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NBTViewer));
			treeView = new TreeView();
			icons = new ImageList(components);
			SuspendLayout();
			// 
			// treeView
			// 
			treeView.Dock = DockStyle.Fill;
			treeView.ImageIndex = 0;
			treeView.ImageList = icons;
			treeView.Location = new Point(0, 0);
			treeView.Name = "treeView";
			treeView.SelectedImageIndex = 0;
			treeView.Size = new Size(436, 450);
			treeView.TabIndex = 0;
			// 
			// icons
			// 
			icons.ColorDepth = ColorDepth.Depth32Bit;
			icons.ImageStream = (ImageListStreamer)resources.GetObject("icons.ImageStream");
			icons.TransparentColor = Color.Transparent;
			icons.Images.SetKeyName(0, "blank.png");
			icons.Images.SetKeyName(1, "byte.png");
			icons.Images.SetKeyName(2, "short.png");
			icons.Images.SetKeyName(3, "integer.png");
			icons.Images.SetKeyName(4, "long.png");
			icons.Images.SetKeyName(5, "float.png");
			icons.Images.SetKeyName(6, "double.png");
			icons.Images.SetKeyName(7, "byte_array.png");
			icons.Images.SetKeyName(8, "string.png");
			icons.Images.SetKeyName(9, "list.png");
			icons.Images.SetKeyName(10, "compound.png");
			icons.Images.SetKeyName(11, "int_array.png");
			icons.Images.SetKeyName(12, "long_array.png");
			icons.Images.SetKeyName(13, "boolean.png");
			// 
			// NBTViewer
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(436, 450);
			Controls.Add(treeView);
			Name = "NBTViewer";
			Text = "NBTViewer";
			ResumeLayout(false);
		}

		#endregion

		private TreeView treeView;
		private ImageList icons;
	}
}
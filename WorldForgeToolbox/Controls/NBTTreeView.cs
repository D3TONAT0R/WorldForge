using System.ComponentModel;
using WorldForge.NBT;

namespace WorldForgeToolbox
{
	public partial class NBTTreeView : UserControl
	{
		public NBTCompound? Data { get; private set; }

		public NBTTreeView()
		{
			InitializeComponent();
		}

		public void DisplayContent(NBTFile nbt, string title)
		{
			Data = nbt.contents;
			Text = title;
			treeView.Nodes.Clear();
			var dataVersionNode = new TreeNode($"DataVersion: {nbt.dataVersion}");
			dataVersionNode.ImageIndex = (int)NBTTag.TAG_Int;
			dataVersionNode.SelectedImageIndex = dataVersionNode.ImageIndex;
			treeView.Nodes.Add(dataVersionNode);
			var rootNode = new TreeNode("Root");
			rootNode.ImageIndex = (int)NBTTag.TAG_Compound;
			rootNode.SelectedImageIndex = rootNode.ImageIndex;
			PopulateTreeNode(rootNode, nbt.contents);
			treeView.Nodes.Add(rootNode);
		}

		public void DisplayContent(NBTCompound nbt, string title)
		{
			Data = nbt;
			Text = title;
			treeView.Nodes.Clear();
			var rootNode = new TreeNode("Root");
			rootNode.ImageIndex = (int)NBTTag.TAG_Compound;
			rootNode.SelectedImageIndex = rootNode.ImageIndex;
			PopulateTreeNode(rootNode, nbt);
			treeView.Nodes.Add(rootNode);
		}

		private void PopulateTreeNode(TreeNode node, NBTCompound comp)
		{
			foreach (var kvp in comp)
			{
				Add(node, NBTMappings.GetTag(kvp.Value.GetType()), $"{kvp.Key}: {ShowValue(kvp.Value)}", kvp.Value);
			}
		}

		private void PopulateListNode(TreeNode node, NBTList childList)
		{
			for (int i = 0; i < childList.Length; i++)
			{
				Add(node, childList.ContentsType, $"[{i}]: {ShowValue(childList[i])}", childList[i]);
			}
		}

		private void Add(TreeNode node, NBTTag nbtTag, string text, object value)
		{
			var childNode = new TreeNode(text);
			childNode.ImageIndex = (int)nbtTag;
			childNode.Tag = value;
			childNode.SelectedImageIndex = childNode.ImageIndex;
			if (value is NBTCompound childComp)
			{
				PopulateTreeNode(childNode, childComp);
			}
			else if (value is NBTList childList)
			{
				PopulateListNode(childNode, childList);
			}
			node.Nodes.Add(childNode);
		}

		private string ShowValue(object? value)
		{
			return value == null ? "(null)" : $"{value}";
		}

		public void ExpandAll() => treeView.ExpandAll();

		public void CollapseAll() => treeView.CollapseAll();

		private void InitializeComponent()
		{
			components = new Container();
			ComponentResourceManager resources = new ComponentResourceManager(typeof(NBTTreeView));
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
			treeView.Size = new Size(150, 150);
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
			// NBTTreeView
			// 
			Controls.Add(treeView);
			Name = "NBTTreeView";
			ResumeLayout(false);

		}
		public TreeView treeView;
		private ImageList icons;
		private IContainer components;
	}
}

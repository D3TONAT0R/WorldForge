using WorldForge.IO;
using WorldForge.NBT;

namespace WorldForgeToolbox
{
	public partial class NBTViewer : ToolboxForm
	{
		private NBTCompound? data;

		public NBTViewer(string? inputFile) : base(inputFile)
		{
			InitializeComponent();
		}

		protected override void OnShown(EventArgs e)
		{
			if (data != null) return;
			inputFileArg ??= OpenFilePrompt();
			if (inputFileArg != null)
			{
				OpenFile(inputFileArg);
			}
		}

		private string? OpenFilePrompt()
		{
			//Open file dialog to select region file
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Region Files (*.mca;*.mcr)|*.mca;*.mcr|All Files (*.*)|*.*";
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				return openFileDialog.FileName;
			}
			else
			{
				return null;
			}
		}

		private void OpenFile(string file)
		{
			var extension = Path.GetExtension(file).ToLower();
			if (extension == ".mca" || extension == ".mcr")
			{
				var nbt = new NBTCompound();
				var region = RegionDeserializer.LoadMainRegion(file, null);
				for (int z = 0; z < 32; z++)
				{
					for (int x = 0; x < 32; x++)
					{
						if (region.chunks[z, x] != null)
						{
							nbt.Add($"Chunk[{x},{z}]", region.chunks[z, x].SourceData.main.contents);
						}
					}
				}
				DisplayContent(nbt, Path.GetFileName(file));
			}
			else
			{
				var nbt = new NBTFile(file);
				DisplayContent(nbt, Path.GetFileName(file));
			}
		}

		public NBTViewer(NBTFile nbt, string title) : base(null)
		{
			InitializeComponent();
			DisplayContent(nbt, title);
		}

		public void DisplayContent(NBTFile nbt, string title)
		{
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
			data = nbt;
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

		private void Add(TreeNode node, NBTTag tag, string key, object value)
		{
			var childNode = new TreeNode(key);
			childNode.ImageIndex = (int)tag;
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

		private string ShowValue(object value)
		{
			if (value == null) return "(null)";
			else return $"{value}";
		}

		private void toolboxButton_Click(object sender, EventArgs e)
		{
			Toolbox.Instance.Return();
			Close();
		}
	}
}

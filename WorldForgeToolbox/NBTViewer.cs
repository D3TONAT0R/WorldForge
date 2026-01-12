using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using WorldForge.NBT;

namespace WorldForgeToolbox
{
	public partial class NBTViewer : Form
	{
		public NBTViewer(string file)
		{
			InitializeComponent();
			if(string.IsNullOrEmpty(file))
			{
				//Open file dialog to select region file
				OpenFileDialog openFileDialog = new OpenFileDialog();
				openFileDialog.Filter = "Region Files (*.mca;*.mcr)|*.mca;*.mcr|All Files (*.*)|*.*";
				if(openFileDialog.ShowDialog() == DialogResult.OK)
				{
					file = openFileDialog.FileName;
				}
				else
				{
					Close();
					return;
				}
			}
			var nbt = new NBTFile(file);
			DisplayContent(nbt, Path.GetFileName(file));
		}

		public NBTViewer(NBTFile nbt, string title)
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

		private void PopulateTreeNode(TreeNode node, NBTCompound comp)
		{
			foreach(var kvp in comp)
			{
				Add(node, NBTMappings.GetTag(kvp.Value.GetType()), $"{kvp.Key}: {ShowValue(kvp.Value)}", kvp.Value);
			}
		}

		private void PopulateListNode(TreeNode node, NBTList childList)
		{
			for(int i = 0; i < childList.Length; i++)
			{
				Add(node, childList.ContentsType, $"[{i}]: {ShowValue(childList[i])}", childList[i]);
			}
		}

		private void Add(TreeNode node, NBTTag tag, string key, object value)
		{
			var childNode = new TreeNode(key);
			childNode.ImageIndex = (int)tag;
			childNode.SelectedImageIndex = childNode.ImageIndex;
			if(value is NBTCompound childComp)
			{
				PopulateTreeNode(childNode, childComp);
			}
			else if(value is NBTList childList)
			{
				PopulateListNode(childNode, childList);
			}
			node.Nodes.Add(childNode);
		}

		private string ShowValue(object value)
		{
			if(value == null) return "(null)";
			else return $"{value}";
		}
	}
}

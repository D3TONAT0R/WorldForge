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

namespace RegionViewer
{
	public partial class NBTViewer : Form
	{
		public NBTViewer()
		{
			InitializeComponent();
		}

		public void DisplayContent(NBTFile nbt)
		{
			treeView.Nodes.Clear();
			var dataVersionNode = new TreeNode($"DataVersion: {nbt.dataVersion}");
			treeView.Nodes.Add(dataVersionNode);
			var rootNode = new TreeNode("Root");
			PopulateTreeNode(rootNode, nbt.contents);
			treeView.Nodes.Add(rootNode);
		}

		private void PopulateTreeNode(TreeNode node, NBTCompound comp)
		{
			foreach(var kvp in comp)
			{
				Add(node, $"{kvp.Key}: {ShowValue(kvp.Value)}", kvp.Value);
			}
		}

		private void PopulateListNode(TreeNode node, NBTList childList)
		{
			for(int i = 0; i < childList.Length; i++)
			{
				Add(node, $"[{i}]: {ShowValue(childList[i])}", childList[i]);
			}
		}

		private void Add(TreeNode node, string key, object value)
		{
			var childNode = new TreeNode(key);
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
			else return $"{value} [{value.GetType().Name}]";
		}
	}
}

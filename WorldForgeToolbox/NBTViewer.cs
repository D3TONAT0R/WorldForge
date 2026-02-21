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
			if(string.IsNullOrEmpty(inputFileArg))
			{
				FileDialogUtility.OpenFileDialog(out inputFileArg, FileDialogUtility.NBT_DATA_FILTER, FileDialogUtility.REGION_FILTER, FileDialogUtility.ALL_FILES_FILTER);
			}
			if (inputFileArg != null)
			{
				OpenFile(inputFileArg);
			}
		}

		private void OpenFile(string file)
		{
			var extension = Path.GetExtension(file).ToLower();
			Text = Path.GetFileName(file);
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
				nbtView.DisplayContent(nbt, Text);
			}
			else
			{
				var nbt = new NBTFile(file);
				nbtView.DisplayContent(nbt, Text);
			}
		}

		public NBTViewer(NBTFile nbt, string title) : base(null)
		{
			InitializeComponent();
			Text = title;
			data = nbt.contents;
			nbtView.DisplayContent(nbt, title);
		}

		private void toolboxButton_Click(object sender, EventArgs e)
		{
			Toolbox.Instance.Return();
			Close();
		}

		private void expandAllButton_Click(object sender, EventArgs e)
		{
			nbtView.ExpandAll();
		}

		private void collapseAllButton_Click(object sender, EventArgs e)
		{
			nbtView.CollapseAll();
		}
	}
}

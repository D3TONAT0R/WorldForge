using WorldForge.IO;
using WorldForge.NBT;

namespace WorldForgeToolbox
{
	public partial class PlayerDataViewer : ToolboxForm
	{
		private NBTCompound? data;

		public PlayerDataViewer(string? inputFile) : base(inputFile)
		{
			InitializeComponent();
			canvas.Paint += OnDraw;
		}

		private void OnDraw(object? sender, PaintEventArgs e)
		{
			var g = e.Graphics;
			g.Clear(Color.Gray);
			int w = e.ClipRectangle.Width / 10;
			for(int iy = 0; iy < 4; iy++)
			{
				int y = (int)((3.5f - iy) * w);
				if (iy == 0) y += w / 2;
				for(int ix = 0; ix < 9; ix++)
				{
					int x = ix * w + w / 2;
					g.DrawRectangle(Pens.Black, x, y, w, w);
				}
			}
		}

		protected override void OnShown(EventArgs e)
		{
			if (data != null) return;
			if (string.IsNullOrEmpty(inputFileArg))
			{
				OpenFileUtility.OpenFileDialog(out inputFileArg, OpenFileUtility.NBT_DATA_FILTER, OpenFileUtility.ALL_FILES_FILTER);
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

		private void toolboxButton_Click(object sender, EventArgs e)
		{
			ReturnToToolbox();
		}
	}
}

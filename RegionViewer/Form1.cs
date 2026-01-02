using WorldForge;
using WorldForge.Coordinates;
using WorldForge.IO;

namespace RegionViewer;

public partial class Form1 : Form
{
	private Bitmap[,] chunkMaps = new Bitmap[32, 32];
	private Random random = new Random();
	private string fileName;
	private WorldForge.Regions.Region region;

	private ChunkCoord hoveredChunk = new ChunkCoord(-1, -1);

	public Form1()
	{
		WorldForgeManager.Initialize();
		Bitmaps.BitmapFactory = new WinformsBitmapFactory();
		InitializeComponent();
		//Get file name from command line args
		string[] args = Environment.GetCommandLineArgs();
		if(args.Length > 1)
		{
			fileName = args[1];
		}
		else
		{
			//Open file dialog to select region file
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Region Files (*.mca;*.mcr)|*.mca;*.mcr|All Files (*.*)|*.*";
			if(openFileDialog.ShowDialog() == DialogResult.OK)
			{
				fileName = openFileDialog.FileName;
			}
			else
			{
				Environment.Exit(0);
				return;
			}
		}
		Load();
	}

	private void Load()
	{
		//Clear chunk maps
		for(int x = 0; x < 32; x++)
		{
			for(int z = 0; z < 32; z++)
			{
				chunkMaps[x, z] = null;
			}
		}
		//Load region file
		region = RegionDeserializer.LoadMainRegion(fileName, null);
		//Change form title to file name
		Text = $"Region Viewer - {Path.GetFileName(fileName)}";
		Parallel.For(0, 1024, i =>
		{
			int x = i % 32;
			int z = i / 32;
			var chunk = region.GetChunk(x, z);
			if(chunk != null)
			{
				var map = (WinformsBitmap)SurfaceMapGenerator.GenerateSurfaceMap(chunk, HeightmapType.AllBlocks, true, WorldForge.Maps.MapColorPalette.Modern, true);
				chunkMaps[x, z] = map.bitmap;
				canvas.Invalidate();
			}
		});
	}

	private void Draw(object sender, PaintEventArgs e)
	{
		e.Graphics.Clear(Color.Black);
		for(int z = 0; z < 32; z++)
		{
			for(int x = 0; x < 32; x++)
			{
				if(chunkMaps[x, z] != null)
				{
					e.Graphics.DrawImage(chunkMaps[x, z], x * 16, z * 16);
				}
			}
		}
		if(hoveredChunk.x >= 0 && hoveredChunk.z >= 0)
		{
			e.Graphics.DrawRectangle(Pens.Red, hoveredChunk.x * 16, hoveredChunk.z * 16, 16, 16);
		}
	}

	private void OnCanvasClick(object sender, EventArgs e)
	{
		var mouseEvent = (MouseEventArgs)e;
		if(mouseEvent.Button == MouseButtons.Left)
		{
			if(hoveredChunk.x >= 0 && hoveredChunk.z >= 0)
			{
				var chunk = region.GetChunk(hoveredChunk.x, hoveredChunk.z);
				if(chunk != null)
				{
					var nbt = RegionDeserializer.LoadChunkDataAtIndex(region.sourceFilePaths.mainPath, hoveredChunk.LocalRegionPos.x + hoveredChunk.LocalRegionPos.z * 32);
					var nbtViewer = new NBTViewer();
					nbtViewer.DisplayContent(nbt);
					nbtViewer.Show();
				}
			}
		}
		else
		if(mouseEvent.Button == MouseButtons.Right)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Region Files (*.mca;*.mcr)|*.mca;*.mcr|All Files (*.*)|*.*";
			if(openFileDialog.ShowDialog() == DialogResult.OK)
			{
				fileName = openFileDialog.FileName;
				Load();
			}
		}
	}

	private void OnMouseMove(object sender, MouseEventArgs e)
	{
		hoveredChunk = new ChunkCoord(e.X / 16, e.Y / 16);
		canvas.Invalidate();
	}

	private void OnMouseExit(object sender, EventArgs e)
	{
		hoveredChunk = new ChunkCoord(-1, -1);
		canvas.Invalidate();
	}
}
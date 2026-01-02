using WorldForge;
using WorldForge.IO;

namespace RegionViewer;

public partial class Form1 : Form
{
	private Bitmap[,] chunkMaps = new Bitmap[32,32];
	private Random random = new Random();
	private string fileName;
	
	public Form1()
	{
		WorldForgeManager.Initialize();
		Bitmaps.BitmapFactory = new WinformsBitmapFactory();
		InitializeComponent();
		//Get file name from command line args
		string[] args = Environment.GetCommandLineArgs();
		if (args.Length > 1)
		{
			fileName = args[1];
		}
		else
		{
			//Open file dialog to select region file
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Region Files (*.mca;*.mcr)|*.mca;*.mcr|All Files (*.*)|*.*";
			if (openFileDialog.ShowDialog() == DialogResult.OK)
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
		var region = RegionDeserializer.LoadMainRegion(fileName, null);
		//Change form title to file name
		this.Text = $"Region Viewer - {Path.GetFileName(fileName)}";
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
	}

	private void OnCanvasClick(object sender, EventArgs e)
	{
		if (((MouseEventArgs)e).Button == MouseButtons.Right)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Region Files (*.mca;*.mcr)|*.mca;*.mcr|All Files (*.*)|*.*";
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				fileName = openFileDialog.FileName;
				Load();
			}
		}
	}
}
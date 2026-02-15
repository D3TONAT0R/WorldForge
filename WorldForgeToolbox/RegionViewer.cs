using System.Collections.Concurrent;
using System.Drawing.Drawing2D;
using WorldForge;
using WorldForge.Coordinates;
using WorldForge.IO;
using Timer = System.Windows.Forms.Timer;

namespace WorldForgeToolbox;

public partial class RegionViewer : ToolboxForm
{
	private Bitmap?[] chunkMaps = new Bitmap?[1024];
	private WorldForge.Regions.Region? region;
	private RegionLocation currentLocation;

	private ChunkCoord hoveredChunk = new ChunkCoord(-1, -1);

	private List<int> chunkRenderQueue = new List<int>();
	private List<int> currentChunkRenders = new List<int>();
	private object currentChunkRendersLock = new object();

	private Timer updateRenderTimer;

	private Brush regionBackgroundBrush = new HatchBrush(HatchStyle.Percent25, Color.FromArgb(64, 128, 128, 128), Color.Black);
	private Brush renderingChunkBrush = new SolidBrush(Color.FromArgb(160, 128, 128, 128));
	private Brush queuedChunkBrush = new HatchBrush(HatchStyle.Percent50, Color.FromArgb(64, 128, 128, 128), Color.FromArgb(128, 128, 128, 128));

	public RegionViewer(string inputFile) : base(inputFile)
	{
		InitializeComponent();
		updateRenderTimer = new Timer() { Interval = 100 };
		updateRenderTimer.Tick += (sender, eventArgs) => HandleRenders();
	}

	protected override void OnShown(EventArgs _)
	{
		if (string.IsNullOrEmpty(inputFileArg))
		{
			//Open file dialog to select region file
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Region Files (*.mca;*.mcr)|*.mca;*.mcr|All Files (*.*)|*.*";
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				inputFileArg = openFileDialog.FileName;
			}
		}
		try
		{
			if (!string.IsNullOrEmpty(inputFileArg)) Load();
		}
		catch (Exception e)
		{
			MessageBox.Show(e.Message, "Failed to load region", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}

	private void Load()
	{
		viewport.Center = new System.Numerics.Vector2(256, 256);
		currentChunkRenders.Clear();
		chunkRenderQueue.Clear();
		//Queue renders, starting from center
		chunkRenderQueue.AddRange(Enumerable.Range(0, 1024).OrderBy(i =>
		{
			int x = i % 32;
			int z = i / 32;
			x -= 16;
			z -= 16;
			return Math.Max(Math.Abs(x), Math.Abs(z));
		}));
		RegionLocation.TryGetFromFileName(Path.GetFileName(inputFileArg), out currentLocation);
		//Clear chunk maps
		for (int i = 0; i < chunkMaps.Length; i++)
		{
			chunkMaps[i] = null;
		}
		//Load region file
		region = RegionDeserializer.LoadMainRegion(inputFileArg, null);
		//Change form title to file name
		Text = $"Region Viewer - {Path.GetFileName(inputFileArg)}";
		scrollNorth.Enabled = RegionExists(currentLocation.North, out _);
		scrollSouth.Enabled = RegionExists(currentLocation.South, out _);
		scrollWest.Enabled = RegionExists(currentLocation.West, out _);
		scrollEast.Enabled = RegionExists(currentLocation.East, out _);
		Invalidate(true);
		updateRenderTimer.Start();
	}

	private void Draw(object sender, PaintEventArgs e)
	{
		var g = e.Graphics;
		if (region == null)
		{
			viewport.DrawDisabled(g, "No region loaded");
			return;
		}
		var center = viewport.Center;
		center.X = Math.Clamp(center.X, 0, 512);
		center.Y = Math.Clamp(center.Y, 0, 512);
		viewport.Center = center;
		g.PixelOffsetMode = PixelOffsetMode.Half;
		g.InterpolationMode = InterpolationMode.NearestNeighbor;
		HandleRenders();
		g.Clear(Color.Black);
		viewport.FillRectangle(g, regionBackgroundBrush, 0, 0, 512, 512);
		var scale = viewport.ZoomScale;
		var size = 16 * scale;
		for (int z = 0; z < 32; z++)
		{
			for (int x = 0; x < 32; x++)
			{
				var pos = viewport.MapToScreenPos(new System.Numerics.Vector2(x * 16, z * 16));
				var rect = new Rectangle(pos.X, pos.Y, size, size);
				int i = z * 32 + x;
				if (currentChunkRenders.Contains(i)) g.FillRectangle(renderingChunkBrush, rect);
				else
				{
					var map = chunkMaps[i];
					if (map != null) g.DrawImage(map, rect);
					else if (region.chunks[x, z] != null) g.FillRectangle(queuedChunkBrush, rect);
				}
			}
		}
		if (toggleGrid.Checked)
		{
			viewport.DrawGrid(g, Pens.DarkGray, 16);
		}
		if (hoveredChunk.x >= 0 && hoveredChunk.z >= 0)
		{
			var pos = viewport.MapToScreenPos(new System.Numerics.Vector2(hoveredChunk.x * 16, hoveredChunk.z * 16));
			g.DrawRectangle(hoverOutlinePen, pos.X, pos.Y, size, size);
		}
	}

	private void HandleRenders()
	{
		while (chunkRenderQueue.Count > 0 && currentChunkRenders.Count < 16)
		{
			int i = chunkRenderQueue[0];
			chunkRenderQueue.RemoveAt(0);
			int x = i % 32;
			int z = i / 32;
			StartChunkRender(x, z);
		}
	}

	private void StartChunkRender(int x, int z)
	{
		int i = z * 32 + x;
		lock (currentChunkRendersLock)
		{
			currentChunkRenders.Add(i);
		}
		Task.Run(() =>
		{
			try
			{
				var chunk = region.GetChunk(x, z, ChunkLoadFlags.None);
				if (chunk != null)
				{
					if (!chunk.IsLoaded) chunk.Load(ChunkLoadFlags.Blocks);
					var map = (WinformsBitmap)SurfaceMapGenerator.GenerateSurfaceMap(chunk, HeightmapType.AllBlocks, true, WorldForge.Maps.MapColorPalette.Modern, true);
					chunkMaps[i] = map.bitmap;
					viewport.Invalidate();
				}
			}
			finally
			{
				lock (currentChunkRendersLock)
				{
					currentChunkRenders.Remove(i);
				}
			}
		});
	}

	private void OnViewportDoubleClick(object sender, EventArgs e)
	{
		var mouseEvent = (MouseEventArgs)e;
		if (mouseEvent.Button == MouseButtons.Left)
		{
			if (hoveredChunk.x >= 0 && hoveredChunk.z >= 0)
			{
				var chunk = region.GetChunk(hoveredChunk.x, hoveredChunk.z);
				if (chunk != null)
				{
					var nbt = RegionDeserializer.LoadChunkDataAtIndex(region.sourceFilePaths.mainPath, hoveredChunk.LocalRegionPos.x + hoveredChunk.LocalRegionPos.z * 32);
					var nbtViewer = new NBTViewer(nbt, $"Chunk {chunk.WorldSpaceCoord}");
					nbtViewer.Show();
				}
			}
		}
		else
		if (mouseEvent.Button == MouseButtons.Right)
		{
			RegionFileDialog();
		}
	}

	private void RegionFileDialog()
	{
		OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Filter = "Region Files (*.mca;*.mcr)|*.mca;*.mcr|All Files (*.*)|*.*";
		if (openFileDialog.ShowDialog() == DialogResult.OK)
		{
			inputFileArg = openFileDialog.FileName;
			Load();
		}
	}

	private void OnMouseMove(object sender, MouseEventArgs e)
	{
		var mapPos = viewport.ScreenToMapPos(e.Location);
		hoveredChunk = new ChunkCoord((int)mapPos.X / 16, (int)mapPos.Y / 16);
		viewport.Invalidate();
	}

	private void OnMouseExit(object sender, EventArgs e)
	{
		hoveredChunk = new ChunkCoord(-1, -1);
		viewport.Invalidate();
	}

	private void scrollNorth_Click(object sender, EventArgs e)
	{
		TryLoadRegion(currentLocation.North);
	}

	private void scrollSouth_Click(object sender, EventArgs e)
	{
		TryLoadRegion(currentLocation.South);
	}

	private void scrollWest_Click(object sender, EventArgs e)
	{
		TryLoadRegion(currentLocation.West);
	}

	private void scrollEast_Click(object sender, EventArgs e)
	{
		TryLoadRegion(currentLocation.East);
	}


	private void TryLoadRegion(RegionLocation location)
	{
		if (string.IsNullOrEmpty(inputFileArg)) return;
		if (RegionExists(location, out var name))
		{
			inputFileArg = name;
			Load();
		}
		else
		{
			MessageBox.Show("Region not found " + name);
		}
	}

	private bool RegionExists(RegionLocation location, out string filename)
	{
		var directory = Path.GetDirectoryName(inputFileArg);
		var extension = Path.GetExtension(inputFileArg).ToLower();
		filename = Path.Combine(directory, location.ToFileName().Replace(".mca", "") + extension);
		return File.Exists(filename);
	}

	private void openFile_Click(object sender, EventArgs e)
	{
		RegionFileDialog();
	}

	private void toolboxButton_Click(object sender, EventArgs e)
	{
		Toolbox.Instance.Return();
		Close();
	}

	private void toggleGrid_Click(object sender, EventArgs e)
	{
		toggleGrid.Checked = !toggleGrid.Checked;
		viewport.Repaint();
	}

	private void resetView_Click(object sender, EventArgs e)
	{
		viewport.SetCenter(256, 256);
		viewport.Zoom = 1;
		viewport.Repaint();
	}
}
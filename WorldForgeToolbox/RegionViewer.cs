using WorldForge;
using WorldForge.Coordinates;
using WorldForge.IO;

namespace WorldForgeToolbox;

public partial class RegionViewer : Form
{
    private Bitmap[,] chunkMaps = new Bitmap[32, 32];
    private Random random = new Random();
    private string fileName;
    private WorldForge.Regions.Region region;
    private RegionLocation currentLocation;

    private ChunkCoord hoveredChunk = new ChunkCoord(-1, -1);

    public RegionViewer(string file)
    {
        InitializeComponent();
        fileName = file;
        if(string.IsNullOrEmpty(fileName))
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
                Close();
                return;
            }
        }
        Load();
    }

    private void Load()
    {
        RegionLocation.TryGetFromFileName(Path.GetFileName(fileName), out currentLocation);
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
        var task = new Task(() => Parallel.For(0, 1024, i =>
        {
            int x = i % 32;
            int z = i / 32;
            var chunk = region.GetChunk(x, z, ChunkLoadFlags.None);
            if(chunk != null)
            {
                chunk.Load(ChunkLoadFlags.Blocks);
                var map = (WinformsBitmap)SurfaceMapGenerator.GenerateSurfaceMap(chunk, HeightmapType.AllBlocks, true, WorldForge.Maps.MapColorPalette.Modern, true);
                chunkMaps[x, z] = map.bitmap;
                canvas.Invalidate();
            }
        }));
        task.Start();
        scrollNorth.Enabled = RegionExists(currentLocation.North, out _);
        scrollSouth.Enabled = RegionExists(currentLocation.South, out _);
        scrollWest.Enabled = RegionExists(currentLocation.West, out _);
        scrollEast.Enabled = RegionExists(currentLocation.East, out _);
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
                    var nbtViewer = new NBTViewer(nbt, $"Chunk {chunk.WorldSpaceCoord}");
                    nbtViewer.Show();
                }
            }
        }
        else
        if(mouseEvent.Button == MouseButtons.Right)
        {
	        RegionFileDialog();
        }
    }

    private void RegionFileDialog()
    {
	    OpenFileDialog openFileDialog = new OpenFileDialog();
	    openFileDialog.Filter = "Region Files (*.mca;*.mcr)|*.mca;*.mcr|All Files (*.*)|*.*";
	    if(openFileDialog.ShowDialog() == DialogResult.OK)
	    {
		    fileName = openFileDialog.FileName;
		    Load();
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
        if(RegionExists(location, out var name))
        {
            fileName = name;
            Load();
        }
        else
        {
            MessageBox.Show("Region not found " + name);
        }
    }

    private bool RegionExists(RegionLocation location, out string filename)
    {
        var directory = Path.GetDirectoryName(fileName);
        var extension = Path.GetExtension(fileName).ToLower();
        filename = Path.Combine(directory, location.ToFileName().Replace(".mca", "") + extension);
        return File.Exists(filename);
    }

    private void openFile_Click(object sender, EventArgs e)
    {
		RegionFileDialog();
	}
}
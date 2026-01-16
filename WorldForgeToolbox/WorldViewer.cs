using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WorldForge;
using WorldForge.Coordinates;
using WorldForge.Maps;
using Region = WorldForge.Regions.Region;

namespace WorldForgeToolbox
{
	public partial class WorldViewer : Form
	{
		private string fileName;

		private World world;
		private Dictionary<RegionLocation, WinformsBitmap> regionBitmaps = new Dictionary<RegionLocation, WinformsBitmap>();
		private List<Region> currentRenders = new List<Region>();

		private BlockCoord2D center;

		private int zoom = 4;
		private Point lastMousePos;
		private bool mouseDown;
		private Point mousePosition;

		private List<Region> renderQueue = new List<Region>();
		private int runningRenderTasks => currentRenders.Count;

		private Brush currentRenderBrush = new SolidBrush(Color.FromArgb(128, Color.Red));

		public WorldViewer(string file)
		{
			InitializeComponent();
			fileName = file;
			canvas.MouseWheel += OnCanvasScroll;
			canvas.MouseDown += OnCanvasMouseDown;
			canvas.MouseUp += OnCanvasMouseUp;
			canvas.MouseMove += OnCanvasMouseMove;
			canvas.DoubleClick += OnCanvasDoubleClick;
			canvas.Cursor = Cursors.SizeAll;
			if(string.IsNullOrEmpty(fileName))
			{
				OpenFileDialog openFileDialog = new OpenFileDialog();
				openFileDialog.Filter = "World Files (level.dat)|*.dat|All Files (*.*)|*.*";
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
			world = World.Load(Path.GetDirectoryName(fileName));
			center = world.LevelData.spawnpoint.Position;
			Invalidate(true);
		}

		private Point WorldToScreenPoint(BlockCoord pos, Rectangle clipRectangle)
		{
			var spawn = world.LevelData.spawnpoint;
			pos.x -= center.x;
			pos.z -= center.z;
			var x = pos.x / 8;
			var y = pos.z / 8;
			x *= zoom;
			y *= zoom;
			x += clipRectangle.Width / 2;
			y += clipRectangle.Height / 2;
			return new Point(x, y);
		}

		private RegionLocation ScreenToRegion(Point screenPos, Rectangle clipRectangle)
		{
			var spawn = world.LevelData.spawnpoint;
			int x = screenPos.X - clipRectangle.Width / 2;
			int y = screenPos.Y - clipRectangle.Height / 2;
			x /= zoom;
			y /= zoom;
			x *= 8;
			y *= 8;
			x += center.x;
			y += center.z;
			var blockCoord = new BlockCoord(x, 0, y);
			return blockCoord.Region;
		}

		private void OnDraw(object sender, PaintEventArgs e)
		{
			var g = e.Graphics;
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.Clear(Color.Transparent);
			if(world.Overworld != null) DrawDimension(e, g, world.Overworld);
			else if(world.Nether != null) DrawDimension(e, g, world.Nether);
			else if(world.TheEnd != null) DrawDimension(e, g, world.TheEnd);
			else
			{
				g.DrawString("No Dimensions found", Font, Brushes.Red, 10, 10);
			}
		}

		private void DrawDimension(PaintEventArgs e, Graphics g, Dimension dim)
		{
			var spawnPos = WorldToScreenPoint(world.LevelData.spawnpoint.Position, e.ClipRectangle);
			g.DrawLine(Pens.Red, spawnPos.X - 4, spawnPos.Y - 4, spawnPos.X + 4, spawnPos.Y + 4);
			g.DrawLine(Pens.Red, spawnPos.X - 4, spawnPos.Y + 4, spawnPos.X + 4, spawnPos.Y - 4);
			g.InterpolationMode = InterpolationMode.NearestNeighbor;
			foreach(var r in dim.regions)
			{
				var pos = WorldToScreenPoint(r.Key.GetBlockCoord(0, 0, 0), e.ClipRectangle);
				var rect = new Rectangle(pos, new Size(64 * zoom, 64 * zoom));
				if(rect.IntersectsWith(e.ClipRectangle) == false) continue;
				var bmp = RequestSurfaceMap(r.Value);
				if(currentRenders.Contains(r.Value))
				{
					g.FillRectangle(currentRenderBrush, rect);
				}
				else
				{
					g.DrawImage(bmp, rect);
				}
				g.DrawRectangle(Pens.DarkGray, rect);
				//g.DrawString(GetRenderPriority(r.Value).ToString(), Font, Brushes.Gray, rect.X + 2, rect.Y + 2);
			}
			ProcessRenderQueue();
			g.DrawString($"{dim.dimensionID.ID}\nRegion count: " + dim.regions.Count, Font, Brushes.Gray, 10, 10);
		}

		private void ProcessRenderQueue()
		{
			if(runningRenderTasks < 10)
			{
				foreach(var region in renderQueue.OrderBy(GetRenderPriority))
				{
					var bitmap = regionBitmaps[region.regionPos];
					currentRenders.Add(region);
					renderQueue.Remove(region);
					var task = new Task(() => RenderRegionMap(region, bitmap));
					task.ContinueWith(t => currentRenders.Remove(region));
					task.Start();
					if(runningRenderTasks >= 10)
					{
						break;
					}
				}
			}
		}


		private void OnOpenWorldClick(object sender, EventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "World Files (level.dat)|*.dat;";
			if(openFileDialog.ShowDialog() == DialogResult.OK)
			{
				fileName = openFileDialog.FileName;
				Load();
			}
			else
			{
				return;
			}
		}

		private Bitmap RequestSurfaceMap(WorldForge.Regions.Region region)
		{
			if(regionBitmaps.TryGetValue(region.regionPos, out var bmp))
			{
				return bmp.bitmap;
			}
			else
			{
				var bitmap = Bitmaps.Create(64, 64);
				regionBitmaps[region.regionPos] = (WinformsBitmap)bitmap;
				renderQueue.Add(region);
				return regionBitmaps[region.regionPos].bitmap;
			}
		}

		private int GetRenderPriority(Region region)
		{
			//Lower means higher
			var location = region.regionPos;
			var diffX = Math.Abs(location.x - center.Region.x);
			var diffZ = Math.Abs(location.z - center.Region.z);
			return Math.Max(diffX, diffZ);
		}

		private void RenderRegionMap(WorldForge.Regions.Region region, IBitmap bitmap)
		{
			try
			{
				var loaded = region.LoadClone(true, false, WorldForge.IO.ChunkLoadFlags.Blocks);
				for(int x = 0; x < 64; x++)
				{
					for(int z = 0; z < 64; z++)
					{
						int bx = x * 8;
						int bz = z * 8;
						var chunk = loaded.GetChunkAtBlock(new BlockCoord(bx, 0, bz), false);
						if(chunk != null)
						{
							var color = SurfaceMapGenerator.GetSurfaceMapColor(chunk, bx & 15, bz & 15, HeightmapType.AllBlocks, MapColorPalette.Modern);
							bitmap.SetPixel(x, z, color);
						}
					}
				}
			}
			catch 
			{
			}
			canvas.Invalidate();
		}

		private void OnCanvasScroll(object? sender, MouseEventArgs e)
		{
			zoom += Math.Clamp(e.Delta, -1, 1);
			zoom = Math.Clamp(zoom, 1, 8);
			canvas.Invalidate();
		}

		private void OnCanvasMouseDown(object? sender, MouseEventArgs e)
		{
			mouseDown = true;
			lastMousePos = e.Location;
		}

		private void OnCanvasMouseUp(object? sender, MouseEventArgs e)
		{
			mouseDown = false;
		}

		private void OnCanvasMouseMove(object? sender, MouseEventArgs e)
		{
			mousePosition = e.Location;
			if(mouseDown)
			{
				var moveDelta = new Point(e.Location.X - lastMousePos.X, e.Location.Y - lastMousePos.Y);
				center.x -= moveDelta.X * 8 / zoom;
				center.z -= moveDelta.Y * 8 / zoom;
				lastMousePos = e.Location;
				canvas.Invalidate();
			}
		}

		private void OnCanvasDoubleClick(object? sender, EventArgs e)
		{
			var pos = ScreenToRegion(mousePosition, canvas.ClientRectangle);
			if(world.Overworld.TryGetRegion(pos, out var r))
			{
				var viewer = new RegionViewer(r.sourceFilePaths.mainPath);
				viewer.Show();
			}
		}
	}
}

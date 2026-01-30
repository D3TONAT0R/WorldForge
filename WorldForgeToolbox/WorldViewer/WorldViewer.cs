using System.Drawing.Drawing2D;
using WorldForge;
using WorldForge.Coordinates;
using WorldForge.Maps;
using Region = WorldForge.Regions.Region;

namespace WorldForgeToolbox
{
	public partial class WorldViewer : Form
	{
		private const int REGION_RES = 64;
		private const int BLOCKS_PER_PIXEL = 512 / REGION_RES;

		private string fileName;

		private World world;
		private Dimension dimension;
		private RegionMapCache regionBitmaps;
		private List<Region> currentRenders = new List<Region>();

		private BlockCoord2D center;

		private int zoom = 4;
		private Point lastMousePos;
		private bool mouseDown;
		private Point mousePosition;

		private List<Region> renderQueue = new List<Region>();
		private int runningRenderTasks => currentRenders.Count;

		private Brush currentRenderBrush = new SolidBrush(Color.FromArgb(64, 128, 128, 128));

		public WorldViewer(string file)
		{
			InitializeComponent();
			fileName = file;
			canvas.MouseWheel += OnCanvasScroll;
			canvas.MouseDown += OnCanvasMouseDown;
			canvas.MouseUp += OnCanvasMouseUp;
			canvas.MouseMove += OnCanvasMouseMove;
			canvas.MouseLeave += OnCanvasMouseLeave;
			canvas.DoubleClick += OnCanvasDoubleClick;
			canvas.Cursor = Cursors.SizeAll;
			if (string.IsNullOrEmpty(fileName))
			{
				OpenFileDialog openFileDialog = new OpenFileDialog();
				openFileDialog.Filter = "World Files (level.dat)|*.dat|All Files (*.*)|*.*";
				if (openFileDialog.ShowDialog() == DialogResult.OK)
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

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);
			SaveRenderCache();
		}

		private void Load()
		{
			world = World.Load(Path.GetDirectoryName(fileName));
			center = world.LevelData.spawnpoint.Position;
			dimensionSelector.DropDownItems.Clear();
			CreateDimensionMenuItem(world.Overworld);
			CreateDimensionMenuItem(world.Nether);
			CreateDimensionMenuItem(world.TheEnd);
			ShowDimension(world.Overworld ?? world.Nether ?? world.TheEnd);
		}

		private string GetDimensionName(Dimension dim)
		{
            string readableName = dim.dimensionID.ID.Replace("minecraft:", "");
            readableName = readableName.Substring(0, 1).ToUpper() + readableName.Substring(1);
			return readableName;
        }

        private void ShowDimension(Dimension dim)
        {
			SaveRenderCache();
			dimension = dim;
			dimensionSelector.Text = GetDimensionName(dim);
			renderQueue.Clear();
			currentRenders.Clear();
			LoadOrCreateRenderCache();
			Invalidate(true);
        }

        private void CreateDimensionMenuItem(Dimension dim)
        {
			if (dim == null) return;
			
            var button = new ToolStripMenuItem(GetDimensionName(dim))
            {
				Tag = dim
			};
			dimensionSelector.DropDownItems.Add(button);
        }

		private void SaveRenderCache()
		{
			if (!File.Exists(fileName) || world == null || dimension == null) return;
			var worldRoot = Path.GetDirectoryName(fileName);
			var cachePath = Path.Combine(world.GetDimensionDirectory(worldRoot, dimension.dimensionID), "region_map_cache.dat");
			if (!Directory.Exists(Path.GetDirectoryName(cachePath))) return;
			regionBitmaps.Save(cachePath);
		}

		private void LoadOrCreateRenderCache()
		{
			var worldRoot = Path.GetDirectoryName(fileName);
			var cachePath = Path.Combine(world.GetDimensionDirectory(worldRoot, dimension.dimensionID), "region_map_cache.dat");
			regionBitmaps = File.Exists(cachePath) ? RegionMapCache.Load(cachePath) : new RegionMapCache();
		}

		private Point WorldToScreenPoint(BlockCoord pos, Rectangle clipRectangle)
		{
			pos.x -= center.x;
			pos.z -= center.z;
			float x = pos.x / 8f;
			float y = pos.z / 8f;
			x *= zoom;
			y *= zoom;
			x += clipRectangle.Width * 0.5f;
			y += clipRectangle.Height * 0.5f;
			return new Point((int)x, (int)y);
		}

		private BlockCoord2D ScreenToBlockPos(Point screenPos, Rectangle clipRectangle)
		{
			var spawn = world.LevelData.spawnpoint;
			float x = screenPos.X - clipRectangle.Width * 0.5f;
			float y = screenPos.Y - clipRectangle.Height * 0.5f;
			x /= zoom;
			y /= zoom;
			x *= 8;
			y *= 8;
			x += center.x;
			y += center.z;
			return new BlockCoord2D((int)x, (int)y);
		}

		private void OnDraw(object sender, PaintEventArgs e)
		{
			var g = e.Graphics;
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.Clear(Color.Transparent);
			if(dimension != null)
			{
				DrawDimension(e, g, dimension);
			}
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
			g.PixelOffsetMode = PixelOffsetMode.Half;
			g.InterpolationMode = InterpolationMode.NearestNeighbor;
			foreach (var r in dim.regions)
			{
				var pos = WorldToScreenPoint(r.Key.GetBlockCoord(0, 0, 0), e.ClipRectangle);
				var rect = new Rectangle(pos, new Size(64 * zoom, 64 * zoom));
				if (rect.IntersectsWith(e.ClipRectangle) == false) continue;
				var bmp = RequestSurfaceMap(r.Value);
				if (currentRenders.Contains(r.Value))
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
			if (runningRenderTasks < 10)
			{
				foreach (var region in renderQueue.OrderBy(GetRenderPriority))
				{
					var bitmap = regionBitmaps.Get(region.regionPos);
					currentRenders.Add(region);
					renderQueue.Remove(region);
					var wfBitmap = new WinformsBitmap(bitmap);
					var task = new Task(() => RenderRegionMap(region, wfBitmap));
					task.ContinueWith(t =>
					{
						currentRenders.Remove(region);
						regionBitmaps.MarkRenderCompleted(region.regionPos);
					});
					task.Start();
					if (runningRenderTasks >= 10)
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
			if (openFileDialog.ShowDialog() == DialogResult.OK)
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
			if (regionBitmaps.TryGet(region.regionPos, out var bmp))
			{
				return bmp;
			}
			else
			{
				var bitmap = new Bitmap(REGION_RES, REGION_RES);
				var timestamp = DateTime.Now; //TODO: Use timestamp from region file
				regionBitmaps.Set(region.regionPos, bitmap, timestamp, false);
				renderQueue.Add(region);
				return bitmap;
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
				for (int x = 0; x < REGION_RES; x++)
				{
					for (int z = 0; z < REGION_RES; z++)
					{
						if (region.Parent != dimension)
						{
							//Cancel render if dimension or world has changed
							return;
						}
						int bx = x * BLOCKS_PER_PIXEL;
						int bz = z * BLOCKS_PER_PIXEL;
						var chunk = loaded.GetChunkAtBlock(new BlockCoord(bx, 0, bz), false);
						if (chunk != null)
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
			if (mouseDown)
			{
				var moveDelta = new Point(e.Location.X - lastMousePos.X, e.Location.Y - lastMousePos.Y);
				center.x -= moveDelta.X * 8 / zoom;
				center.z -= moveDelta.Y * 8 / zoom;
				lastMousePos = e.Location;
				canvas.Invalidate();
			}
			var blockPos = ScreenToBlockPos(e.Location, canvas.ClientRectangle);
			statusLabel.Text = $"Block {blockPos} | Region {blockPos.Region}";
		}

		private void OnCanvasMouseLeave(object? sender, EventArgs e)
		{
			statusLabel.Text = "";
		}

		private void OnCanvasDoubleClick(object? sender, EventArgs e)
		{
			var pos = ScreenToBlockPos(mousePosition, canvas.ClientRectangle).Region;
			if (dimension.TryGetRegion(pos, out var r))
			{
				var viewer = new RegionViewer(r.sourceFilePaths.mainPath);
				viewer.Show();
			}
		}

		private void OnDimensionSelect(object sender, ToolStripItemClickedEventArgs e)
		{
			ShowDimension((Dimension)e.ClickedItem.Tag);
		}
	}
}

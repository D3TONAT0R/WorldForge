using System.Drawing.Drawing2D;
using WorldForge;
using WorldForge.Coordinates;
using WorldForge.Maps;
using Region = WorldForge.Regions.Region;

namespace WorldForgeToolbox
{
	public partial class WorldViewer : Form
	{
		private class DimensionView
		{
			private string sourceFileName;
			public World world;
			public Dimension dimension;
			public RegionMapCache maps;
			public List<Region> currentRenders = new List<Region>();
			public CancellationTokenSource? cancellationTokenSource;

			public DimensionView(string file, bool loadMapCache = true)
			{
				sourceFileName = file;
				world = World.Load(Path.GetDirectoryName(file));
				if (world.HasOverworld && world.Overworld.regions.Count > 0) dimension = world.Overworld;
				else if (world.HasNether && world.Nether.regions.Count > 0) dimension = world.Nether;
				else if (world.HasTheEnd && world.TheEnd.regions.Count > 0) dimension = world.TheEnd;
				dimension ??= world.Overworld ?? world.Nether ?? world.TheEnd;
				cancellationTokenSource = new CancellationTokenSource();
				if (loadMapCache) LoadOrCreateRenderCache();
				else maps = new RegionMapCache();
			}

			public void Dispose(bool saveCache = true)
			{
				cancellationTokenSource?.Cancel();
				if (saveCache) SaveRenderCache();
				maps = null!;
				world = null!;
				dimension = null!;
			}

			public void SaveRenderCache()
			{
				if (!File.Exists(sourceFileName)) return;
				var worldRoot = Path.GetDirectoryName(sourceFileName);
				var cachePath = Path.Combine(world.GetDimensionDirectory(worldRoot, dimension.dimensionID), "region_map_cache.dat");
				if (!Directory.Exists(Path.GetDirectoryName(cachePath))) return;
				maps.Save(cachePath);
			}

			public void LoadOrCreateRenderCache()
			{
				var worldRoot = Path.GetDirectoryName(sourceFileName);
				var cachePath = Path.Combine(world.GetDimensionDirectory(worldRoot, dimension.dimensionID), "region_map_cache.dat");
				maps = File.Exists(cachePath) ? RegionMapCache.Load(cachePath) : new RegionMapCache();
			}

			public void ClearRenderCache(bool deleteCacheFile)
			{
				maps.Clear();
				var worldRoot = Path.GetDirectoryName(sourceFileName);
				var cachePath = Path.Combine(world.GetDimensionDirectory(worldRoot, dimension.dimensionID), "region_map_cache.dat");
				if (File.Exists(cachePath))
				{
					File.Delete(cachePath);
				}
			}

			public void SwitchDimension(Dimension dim, bool saveMapCache = true)
			{
				cancellationTokenSource?.Cancel();
				cancellationTokenSource = new CancellationTokenSource();
				dimension = dim;
				if (saveMapCache) SaveRenderCache();
				currentRenders.Clear();
				LoadOrCreateRenderCache();
			}
		}

		private const int REGION_RES = 64;
		private const int BLOCKS_PER_PIXEL = 512 / REGION_RES;

		private DimensionView? view;

		private BlockCoord2D center;
		private int zoom = 4;
		private Point lastMousePos;
		private bool mouseDown;
		private Point mousePosition;

		private List<Region> renderQueue = new List<Region>();
		private int RunningRenderTasks => view?.currentRenders.Count ?? 0;
		private bool processNewRenders = true;

		private Brush currentRenderBrush = new SolidBrush(Color.FromArgb(64, 128, 128, 128));

		public WorldViewer(string file)
		{
			InitializeComponent();
			canvas.MouseWheel += OnCanvasScroll;
			canvas.MouseDown += OnCanvasMouseDown;
			canvas.MouseUp += OnCanvasMouseUp;
			canvas.MouseMove += OnCanvasMouseMove;
			canvas.MouseLeave += OnCanvasMouseLeave;
			canvas.DoubleClick += OnCanvasDoubleClick;
			canvas.Cursor = Cursors.SizeAll;
			if (string.IsNullOrEmpty(file))
			{
				if (OpenFileUtility.OpenFileDialog(out file, OpenFileUtility.LEVEL_FILTER, OpenFileUtility.ALL_FILES_FILTER))
				{
					OpenWorld(file);
				}
			}
		}

		private void OpenWorld(string file)
		{
			view?.Dispose();
			view = new DimensionView(file);
			center = view.world.LevelData.spawnpoint.Position;
			dimensionSelector.DropDownItems.Clear();
			CreateDimensionMenuItem(view.world.Overworld);
			CreateDimensionMenuItem(view.world.Nether);
			CreateDimensionMenuItem(view.world.TheEnd);
			Invalidate(true);
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);
			view?.Dispose();
		}

		private string GetDimensionName(Dimension dim)
		{
			string readableName = dim.dimensionID.ID.Replace("minecraft:", "");
			readableName = readableName.Substring(0, 1).ToUpper() + readableName.Substring(1);
			return readableName;
		}

		private void ShowDimension(Dimension dim)
		{
			view.SwitchDimension(dim);
			dimensionSelector.Text = GetDimensionName(dim);
			renderQueue.Clear();
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
			if (view?.dimension != null)
			{
				DrawDimension(e, g, view.dimension);
			}
			else
			{
				g.DrawString("No Dimensions found", Font, Brushes.Red, 10, 10);
			}
		}

		private void DrawDimension(PaintEventArgs e, Graphics g, Dimension dim)
		{
			var spawnPos = WorldToScreenPoint(view.world.LevelData.spawnpoint.Position, e.ClipRectangle);
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
				if (view.currentRenders.Contains(r.Value))
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
			if (!processNewRenders) return;
			if (view != null && RunningRenderTasks < 10)
			{
				foreach (var region in renderQueue.OrderBy(GetRenderPriority))
				{
					var bitmap = view.maps.Get(region.regionPos);
					view.currentRenders.Add(region);
					renderQueue.Remove(region);
					var wfBitmap = new WinformsBitmap(bitmap);
					var task = new Task(() => RenderRegionMap(region, wfBitmap), view.cancellationTokenSource.Token);
					task.ContinueWith(t =>
					{
						if (task.IsCanceled) return;
						view.currentRenders.Remove(region);
						view.maps.MarkRenderCompleted(region.regionPos);
					});
					task.Start();
					if (RunningRenderTasks >= 10)
					{
						break;
					}
					Invalidate(true);
				}
			}
		}

		private void OnOpenWorldClick(object sender, EventArgs e)
		{
			if (OpenFileUtility.OpenFileDialog(out var file, OpenFileUtility.LEVEL_FILTER, OpenFileUtility.ALL_FILES_FILTER))
			{
				OpenWorld(file);
			}
		}

		private Bitmap RequestSurfaceMap(WorldForge.Regions.Region region)
		{
			// Check if we have a cached version that is up to date
			if (view!.maps.TryGet(region.regionPos, out var entry) && region.sourceFilePaths.MainFileLastWriteTimeUtc <= entry.regionTimestamp)
			{
				return entry.bitmap;
			}
			else
			{
				// Create new bitmap and queue for render
				var bitmap = new Bitmap(REGION_RES, REGION_RES);
				var timestamp = region.sourceFilePaths.MainFileLastWriteTimeUtc;
				view.maps.Set(region.regionPos, bitmap, timestamp, false);
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
						if (region.Parent != view!.dimension)
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
			if (view == null) return;
			var pos = ScreenToBlockPos(mousePosition, canvas.ClientRectangle).Region;
			if (view.dimension.TryGetRegion(pos, out var r))
			{
				var viewer = new RegionViewer(r.sourceFilePaths.mainPath);
				viewer.Show();
			}
		}

		private void OnDimensionSelect(object sender, ToolStripItemClickedEventArgs e)
		{
			ShowDimension((Dimension)e.ClickedItem.Tag);
		}

		private void deleteMapCacheButton_Click(object sender, EventArgs e)
		{
			if (view == null) return;
			var result = MessageBox.Show("Are you sure you want to delete the region map cache for the current dimension?", "Delete Map Cache", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
			if (result == DialogResult.Yes)
			{
				view.ClearRenderCache(true);
			}
			Invalidate(true);
		}

		private void resumeMapRender_Click(object sender, EventArgs e)
		{
			processNewRenders = true;
			pauseMapRender.Enabled = true;
			resumeMapRender.Enabled = false;
		}

		private void pauseMapRender_Click(object sender, EventArgs e)
		{
			processNewRenders = false;
			pauseMapRender.Enabled = false;
			resumeMapRender.Enabled = true;
		}

		private void SaveMapCacheButtonClick(object sender, EventArgs e)
		{
			view?.SaveRenderCache();
		}

		private void zoomIn_Click(object sender, EventArgs e)
		{
			zoom = Math.Min(8, zoom + 1);
			Invalidate(true);
		}

		private void zoomOut_Click(object sender, EventArgs e)
		{
			zoom = Math.Max(1, zoom - 1);
			Invalidate(true);
		}

		private void jumpToSpawn_Click(object sender, EventArgs e)
		{
			if(view == null) return;
			center = view.world.LevelData.spawnpoint.Position;
			Invalidate(true);
		}
	}
}

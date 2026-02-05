using System.Drawing.Drawing2D;
using WorldForge;
using WorldForge.Coordinates;
using WorldForge.Maps;
using Region = WorldForge.Regions.Region;

namespace WorldForgeToolbox
{
	public partial class WorldViewer : ToolboxForm
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
		private int Zoom
		{
			get => _zoom;
			set => _zoom = Math.Clamp(value, 1, 6);
		}
		private int ZoomScale => 1 << (Zoom - 1);
		private Point lastMousePos;
		private bool mouseDown;
		private Point mousePosition;
		private RegionLocation? hoveredRegion;

		private List<Region> renderQueue = new List<Region>();
		private int RunningRenderTasks => view?.currentRenders.Count ?? 0;
		private bool processNewRenders = true;

		private Brush currentRenderBrush = new SolidBrush(Color.FromArgb(64, 128, 128, 128));
		private Pen spawnMarker = new Pen(Color.Red, 4);
		private Pen playerMarker = new Pen(Color.LightBlue, 3);
		private Pen missingRenderPen = new Pen(Color.FromArgb(128, 128, 128, 128), 1);

		private int _zoom = 4;

		public WorldViewer(string? inputFile) : base(inputFile)
		{
			InitializeComponent();
			canvas.MouseWheel += OnCanvasScroll;
			canvas.MouseDown += OnCanvasMouseDown;
			canvas.MouseUp += OnCanvasMouseUp;
			canvas.MouseMove += OnCanvasMouseMove;
			canvas.MouseLeave += OnCanvasMouseLeave;
			canvas.DoubleClick += OnCanvasDoubleClick;
			canvas.Cursor = Cursors.SizeAll;
			toggleGrid.Checked = true;
		}

		protected override void OnShown(EventArgs e)
		{
			if (string.IsNullOrEmpty(inputFileArg))
			{
				if (OpenFileUtility.OpenFileDialog(out inputFileArg, OpenFileUtility.LEVEL_FILTER, OpenFileUtility.ALL_FILES_FILTER))
				{
					OpenWorld(inputFileArg);
				}
			}
			else
			{
				OpenWorld(inputFileArg);
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

		private Point WorldToScreenPoint(BlockCoord2D pos, Rectangle clipRectangle)
		{
			pos.x -= center.x;
			pos.z -= center.z;
			float x = pos.x / 16f;
			float y = pos.z / 16f;
			x *= ZoomScale;
			y *= ZoomScale;
			x += clipRectangle.Width * 0.5f;
			y += clipRectangle.Height * 0.5f;
			return new Point((int)x, (int)y);
		}

		private BlockCoord2D ScreenToBlockPos(Point screenPos, Rectangle clipRectangle)
		{
			float x = screenPos.X - clipRectangle.Width * 0.5f;
			float y = screenPos.Y - clipRectangle.Height * 0.5f;
			x /= ZoomScale;
			y /= ZoomScale;
			x *= 16;
			y *= 16;
			x += center.x;
			y += center.z;
			return new BlockCoord2D((int)x, (int)y);
		}

		private void Repaint()
		{
			canvas.Invalidate();
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
			if (view == null) return;
			g.PixelOffsetMode = PixelOffsetMode.Half;
			g.InterpolationMode = InterpolationMode.NearestNeighbor;
			foreach (var r in dim.regions)
			{
				var rect = GetRegionRectangle(e, r.Key);
				if (rect.IntersectsWith(e.ClipRectangle) == false) continue;
				var bmp = RequestSurfaceMap(r.Value);
				if (view.currentRenders.Contains(r.Value))
				{
					g.FillRectangle(currentRenderBrush, rect);
					g.TranslateTransform(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
					int s = rect.Width / 8;
					g.DrawLine(Pens.Gray, -s, -s, s, s);
					g.DrawLine(Pens.Gray, -s, s, s, -s);
					g.DrawLine(Pens.Gray, -s, s, s, s);
					g.DrawLine(Pens.Gray, -s, -s, s, -s);
					g.ResetTransform();
				}
				else
				{
					g.DrawImage(bmp, rect);
					if (renderQueue.Contains(r.Value))
					{
						var rect1 = rect;
						rect1.Inflate(-6, -6);
						g.DrawLine(missingRenderPen, rect1.Left, rect1.Top, rect1.Right, rect1.Bottom);
						g.DrawLine(missingRenderPen, rect1.Left, rect1.Bottom, rect1.Right, rect1.Top);
					}
				}
				if(toggleGrid.Checked) g.DrawRectangle(Pens.DarkGray, rect);
				//g.DrawString(GetRenderPriority(r.Value).ToString(), Font, Brushes.Gray, rect.X + 2, rect.Y + 2);
			}
			if (hoveredRegion.HasValue)
			{
				g.DrawRectangle(hoverOutlinePen, GetRegionRectangle(e, hoveredRegion.Value));
			}
			Cross(g, e, view.world.LevelData.spawnpoint.Position.XZ, spawnMarker, 8);
			foreach(var player in view.world.playerData.Values)
			{
				Cross(g, e, player.player.position.Block.XZ, playerMarker, 6);
			}
			g.DrawString($"{dim.dimensionID.ID}\nRegion count: " + dim.regions.Count, Font, Brushes.Gray, 10, 10);
			ProcessRenderQueue();
		}

		private Rectangle GetRegionRectangle(PaintEventArgs e, RegionLocation location)
		{
			var pos = WorldToScreenPoint(location.GetBlockCoord(0, 0, 0), e.ClipRectangle);
			var rect = new Rectangle(pos, new Size(32 * ZoomScale, 32 * ZoomScale));
			return rect;
		}

		private void Cross(Graphics g, PaintEventArgs e, BlockCoord2D pos, Pen p, int size = 4)
		{
			var spawnPos = WorldToScreenPoint(pos, e.ClipRectangle);
			g.DrawLine(p, spawnPos.X - size, spawnPos.Y - size, spawnPos.X + size, spawnPos.Y + size);
			g.DrawLine(p, spawnPos.X - size, spawnPos.Y + size, spawnPos.X + size, spawnPos.Y - size);
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
					Repaint();
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
			Repaint();
		}

		private void OnCanvasScroll(object? sender, MouseEventArgs e)
		{
			Zoom += Math.Clamp(e.Delta, -1, 1);
			Repaint();
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
			var blockPos = ScreenToBlockPos(e.Location, canvas.ClientRectangle);
			if (mouseDown)
			{
				var moveDelta = new Point(e.Location.X - lastMousePos.X, e.Location.Y - lastMousePos.Y);
				center.x -= moveDelta.X * 16 / ZoomScale;
				center.z -= moveDelta.Y * 16 / ZoomScale;
				lastMousePos = e.Location;
			}
			else
			{
				hoveredRegion = blockPos.Region;
			}
			statusLabel.Text = $"Block {blockPos} | Region {blockPos.Region}";
			Repaint();
		}

		private void OnCanvasMouseLeave(object? sender, EventArgs e)
		{
			statusLabel.Text = "";
			hoveredRegion = null;
			Repaint();
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
			Repaint();
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
			Zoom++;
			Repaint();
		}

		private void zoomOut_Click(object sender, EventArgs e)
		{
			Zoom--;
			Repaint();
		}

		private void jumpToSpawn_Click(object sender, EventArgs e)
		{
			if (view == null) return;
			center = view.world.LevelData.spawnpoint.Position;
			Repaint();
		}

		private void toolboxButton_Click(object sender, EventArgs e)
		{
			Toolbox.Instance.Return();
			Close();
		}

		private void toggleGrid_Click(object sender, EventArgs e)
		{
			toggleGrid.Checked = !toggleGrid.Checked;
			Repaint();
		}
	}
}

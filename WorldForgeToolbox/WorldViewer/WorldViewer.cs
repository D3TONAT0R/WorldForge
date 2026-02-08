using System.Drawing.Drawing2D;
using System.Threading.Tasks;
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
			public string sourceFileName;
			public World world;
			public Dimension dimension;
			public RegionMapCache maps;
			public List<RegionLocation> currentRenders = new List<RegionLocation>();
			public CancellationTokenSource? cancellationTokenSource;

			public Dictionary<UUID, PlayerAccountData> playerAccountDatas = new Dictionary<UUID, PlayerAccountData>();

			public string RootDirectory => Path.GetDirectoryName(sourceFileName);

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

			public PlayerAccountData GetPlayerAccountData(UUID uuid)
			{
				if (!playerAccountDatas.TryGetValue(uuid, out var data))
				{
					data = new PlayerAccountData(uuid);
					playerAccountDatas[uuid] = data;
					data.BeginRequest();
				}
				return data;
			}
		}

		private const int REGION_RES = 64;
		private const int MAX_CONCURRENT_RENDERS = 8;

		private DimensionView? view;

		private readonly Vector3 center = new Vector3();
		private int Zoom
		{
			get => _zoom;
			set => _zoom = Math.Clamp(value, 1, 8);
		}
		private int ZoomScale => 1 << (Zoom - 1);
		private Point lastMousePos;
		private bool mouseDown;
		private Point mousePosition;

		private PlayerData? hoveredPlayer;
		private RegionLocation? hoveredRegion;

		private List<RegionLocation> visibleUnrenderedRegions = new List<RegionLocation>();
		private int RunningRenderTasks => view?.currentRenders.Count ?? 0;
		private bool processNewRenders = true;

		private Brush currentRenderBrush = new SolidBrush(Color.FromArgb(64, 128, 128, 128));
		private Pen spawnMarker = new Pen(Color.DarkOrange, 4);
		private Pen playerMarker = new Pen(Color.LightBlue, 3);
		private Pen missingRenderPen = new Pen(Color.FromArgb(128, 128, 128, 128), 1);
		private Brush darkenMapBrush = new SolidBrush(Color.FromArgb(160, Color.Black));

		private Font boldFont = new Font(SystemFonts.DefaultFont, FontStyle.Bold);

		private int _zoom = 4;

		private StringFormat pointLabelFormat = new StringFormat()
		{
			Alignment = StringAlignment.Near,
			LineAlignment = StringAlignment.Center
		};

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
			center.x = view.world.LevelData.spawnpoint.Position.x;
			center.z = view.world.LevelData.spawnpoint.Position.z;
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
			visibleUnrenderedRegions.Clear();
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
			var centerBlock = center.Block;
			pos.x -= centerBlock.x;
			pos.z -= centerBlock.z;
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
			var centerBlock = center.Block;
			x += centerBlock.x;
			y += centerBlock.z;
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
			bool darken = toggleOpacity.Checked;
			visibleUnrenderedRegions.Clear();
			foreach (var r in dim.regions)
			{
				var rect = GetRegionRectangle(e, r.Key);
				if (rect.IntersectsWith(e.ClipRectangle) == false) continue;
				var bmp = RequestSurfaceMap(r.Value, true);
				if (view.currentRenders.Contains(r.Key))
				{
					// Currently rendering
					g.FillRectangle(currentRenderBrush, rect);
					DrawRenderIcon(g, rect);
				}
				else
				{
					if (bmp != null)
					{
						// Fully rendered
						g.DrawImage(bmp, rect);
						if (darken) g.FillRectangle(darkenMapBrush, rect);
						var hq = view.maps.cache[r.Key].highQualityRender;
						if (hq != null && !hq.renderComplete)
						{
							DrawRenderIcon(g, rect);
						}
					}
					else
					{
						// Not rendered yet
						var rect1 = rect;
						rect1.Inflate(-6, -6);
						g.DrawLine(missingRenderPen, rect1.Left, rect1.Top, rect1.Right, rect1.Bottom);
						g.DrawLine(missingRenderPen, rect1.Left, rect1.Bottom, rect1.Right, rect1.Top);
						visibleUnrenderedRegions.Add(r.Key);
					}
				}
				if (toggleGrid.Checked) g.DrawRectangle(Pens.DarkGray, rect);
				//g.DrawString(GetRenderPriority(r.Value).ToString(), Font, Brushes.Gray, rect.X + 2, rect.Y + 2);
			}
			if (hoveredRegion.HasValue && hoveredPlayer == null)
			{
				g.DrawRectangle(hoverOutlinePen, GetRegionRectangle(e, hoveredRegion.Value));
			}
			Cross(g, e, view.world.LevelData.spawnpoint.Position.XZ, spawnMarker, 8, "Spawn");
			if (togglePlayers.Checked)
			{
				foreach (var player in view.world.playerData.Values)
				{
					var username = view.GetPlayerAccountData(player.player.uuid).GetUsername();
					var avatar = view.GetPlayerAccountData(player.player.uuid).GetAvatar();
					bool hovered = hoveredPlayer == player;
					Pen pen = hovered ? Pens.Red : playerMarker;
					Icon(g, e, player.player.position.Block.XZ, avatar, pen, 16, username, hovered);
				}
			}
			g.DrawString($"{dim.dimensionID.ID}\nRegion count: " + dim.regions.Count, Font, Brushes.Gray, 10, 10);
			ProcessRenderQueue();
		}

		private static void DrawRenderIcon(Graphics g, Rectangle rect)
		{
			g.TranslateTransform(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
			int s = rect.Width / 8;
			g.DrawLine(Pens.Gray, -s, -s, s, s);
			g.DrawLine(Pens.Gray, -s, s, s, -s);
			g.DrawLine(Pens.Gray, -s, s, s, s);
			g.DrawLine(Pens.Gray, -s, -s, s, -s);
			g.ResetTransform();
		}

		private Rectangle GetRegionRectangle(PaintEventArgs e, RegionLocation location)
		{
			var pos = WorldToScreenPoint(location.GetBlockCoord(0, 0, 0), e.ClipRectangle);
			var rect = new Rectangle(pos, new Size(32 * ZoomScale, 32 * ZoomScale));
			return rect;
		}

		private void Cross(Graphics g, PaintEventArgs e, BlockCoord2D pos, Pen p, int size = 4, string? label = null)
		{
			var screenPos = WorldToScreenPoint(pos, e.ClipRectangle);
			g.DrawLine(p, screenPos.X - size, screenPos.Y - size, screenPos.X + size, screenPos.Y + size);
			g.DrawLine(p, screenPos.X - size, screenPos.Y + size, screenPos.X + size, screenPos.Y - size);
			if (label != null)
			{
				g.DrawString(label, boldFont, Brushes.Black, screenPos.X + size + 3, screenPos.Y + 1, pointLabelFormat);
				g.DrawString(label, boldFont, p.Brush, screenPos.X + size + 2, screenPos.Y, pointLabelFormat);
			}
		}

		private void Icon(Graphics g, PaintEventArgs e, BlockCoord2D pos, Image? image, Pen p, int size = 16, string? label = null, bool border = false)
		{
			var screenPos = WorldToScreenPoint(pos, e.ClipRectangle);
			var rect = new Rectangle(screenPos.X - size / 2, screenPos.Y - size / 2, size, size);
			if (image != null) g.DrawImage(image, rect);
			if (border) g.DrawRectangle(p, rect);
			if (label != null)
			{
				g.DrawString(label, boldFont, Brushes.Black, screenPos.X + size / 2 + 3, screenPos.Y + 1, pointLabelFormat);
				g.DrawString(label, boldFont, p.Brush, screenPos.X + size / 2 + 2, screenPos.Y, pointLabelFormat);
			}
		}

		private void ProcessRenderQueue()
		{
			if (!processNewRenders) return;
			int maxConcurrent = forceSingleMapRender.Checked ? 1 : MAX_CONCURRENT_RENDERS;
			if (view != null && RunningRenderTasks < maxConcurrent)
			{
				foreach (var pos in visibleUnrenderedRegions.OrderBy(GetRenderPriority))
				{
					var region = view.dimension.GetRegion(pos);
					if (!view.maps.cache.TryGetValue(pos, out var entry))
					{
						entry = new RegionMapCache.Entry(null, region.sourceFilePaths.MainFileLastWriteTimeUtc);
						view.maps.cache[pos] = entry;
					}
					view.currentRenders.Add(pos);
					visibleUnrenderedRegions.Remove(pos);

					entry.normalRender = BeginRender(region, false);
					if (RunningRenderTasks >= maxConcurrent)
					{
						break;
					}
					Repaint();
				}
			}
		}

		private Render BeginRender(Region region, bool highQuality)
		{
			int res = highQuality ? 512 : REGION_RES;
			return Render.CreateNew(res, res,
				(r, token) =>
				{
					var wfBitmap = new WinformsBitmap(r.bitmap);
					RenderRegionMap(region, wfBitmap, res, token);
				},
				(r, token) =>
				{
					view.currentRenders.Remove(region.regionPos);
					if (token.IsCancellationRequested) return;
					r.renderComplete = true;
					Repaint();
				},
				view.cancellationTokenSource.Token
			);
		}

		private void OnOpenWorldClick(object sender, EventArgs e)
		{
			if (OpenFileUtility.OpenFileDialog(out var file, OpenFileUtility.LEVEL_FILTER, OpenFileUtility.ALL_FILES_FILTER))
			{
				OpenWorld(file);
			}
		}

		private Bitmap? RequestSurfaceMap(Region region, bool allowHighQuality)
		{
			// Check if we have a cached version that is up to date
			if (view!.maps.TryGet(region.regionPos, out var entry) && region.sourceFilePaths.MainFileLastWriteTimeUtc <= entry.regionTimestamp)
			{
				if (allowHighQuality && (entry.highQualityRender?.renderComplete ?? false))
				{
					return entry.highQualityRender.bitmap;
				}
				return entry.normalRender.bitmap;
			}
			else
			{
				return null;
			}
		}

		private int GetRenderPriority(RegionLocation location)
		{
			//Lower means higher
			var centerBlock = center.Block;
			var diffX = Math.Abs(location.x - centerBlock.Region.x);
			var diffZ = Math.Abs(location.z - centerBlock.Region.z);
			return Math.Max(diffX, diffZ);
		}

		private void RenderRegionMap(Region region, IBitmap bitmap, int resolution, CancellationToken token)
		{
			try
			{
				int blocksPerPixel = Math.Max(1, 512 / resolution);
				var loaded = region.LoadClone(true, false, WorldForge.IO.ChunkLoadFlags.Blocks);
				bool fullRes = resolution >= 512;
				for (int x = 0; x < resolution; x++)
				{
					for (int z = 0; z < resolution; z++)
					{
						if (token.IsCancellationRequested)
						{
							//Cancel render if dimension or world has changed
							return;
						}
						int bx = x * blocksPerPixel;
						int bz = z * blocksPerPixel;
						var color = SurfaceMapGenerator.GetSurfaceMapColor(loaded, bx, bz, HeightmapType.AllBlocks, fullRes, MapColorPalette.Modern);
						bitmap.SetPixel(x, z, color);
					}
				}
			}
			catch (Exception e)
			{
				int i = 0;
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
			if (view == null) return;
			if (e.Button == MouseButtons.Right)
			{
				var blockPos = ScreenToBlockPos(e.Location, canvas.ClientRectangle);
				if (view.maps.cache.TryGetValue(blockPos.Region, out var entry) && entry.normalRender.renderComplete)
				{
					entry.highQualityRender = BeginRender(view.dimension.GetRegion(blockPos.Region)!, true);
				}
				Repaint();
			}
		}

		private void OnCanvasMouseMove(object? sender, MouseEventArgs e)
		{
			mousePosition = e.Location;
			if (view == null) return;
			var blockPos = ScreenToBlockPos(e.Location, canvas.ClientRectangle);
			if (mouseDown)
			{
				var moveDelta = new Point(e.Location.X - lastMousePos.X, e.Location.Y - lastMousePos.Y);
				center.x -= moveDelta.X * 16f / ZoomScale;
				center.z -= moveDelta.Y * 16f / ZoomScale;
				lastMousePos = e.Location;
			}
			else
			{
				var regPos = blockPos.Region;
				hoveredRegion = view.dimension.HasRegion(regPos) ? blockPos.Region : null;
				hoveredPlayer = null;
				if (togglePlayers.Checked)
				{
					foreach (var player in view!.world.playerData.Values)
					{
						var playerScreenPos = WorldToScreenPoint(player.player.position.Block.XZ, canvas.ClientRectangle);
						var distance = Math.Sqrt(Math.Pow(playerScreenPos.X - e.Location.X, 2) + Math.Pow(playerScreenPos.Y - e.Location.Y, 2));
						if (distance <= 16)
						{
							hoveredPlayer = player;
							statusLabel.Text = $"Player: {view.GetPlayerAccountData(player.player.uuid).GetUsername()} | Block {blockPos} | Region {blockPos.Region}";
						}
					}
				}
			}
			statusLabel.Text = $"Block {blockPos} | Region {blockPos.Region}";
			canvas.Cursor = hoveredPlayer != null ? Cursors.Hand : Cursors.SizeAll;
			Repaint();
		}

		private void OnCanvasMouseLeave(object? sender, EventArgs e)
		{
			statusLabel.Text = "";
			hoveredRegion = null;
			hoveredPlayer = null;
			Repaint();
		}

		private void OnCanvasDoubleClick(object? sender, EventArgs _)
		{
			if (view == null) return;
			if (hoveredPlayer != null)
			{
				try
				{
					var playerPath = Path.Combine(view.RootDirectory, "playerdata", hoveredPlayer.player.uuid.ToString(true) + ".dat");
					var viewer = new PlayerDataViewer(playerPath);
					viewer.Show();
				}
				catch (Exception e)
				{
					MessageBox.Show(e.ToString());
				}
			}
			else if (hoveredRegion != null)
			{
				if (view.dimension.TryGetRegion(hoveredRegion.Value, out var r))
				{
					var viewer = new RegionViewer(r.sourceFilePaths.mainPath);
					viewer.Show();
				}
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
			center.x = view.world.LevelData.spawnpoint.Position.x;
			center.z = view.world.LevelData.spawnpoint.Position.z;
			Repaint();
		}

		private void toolboxButton_Click(object sender, EventArgs e)
		{
			Toolbox.Instance.Return();
			Close();
		}

		private void ToggleGrid(object sender, EventArgs e)
		{
			toggleGrid.Checked = !toggleGrid.Checked;
			Repaint();
		}

		private void TogglePlayers(object sender, EventArgs e)
		{
			togglePlayers.Checked = !togglePlayers.Checked;
			Repaint();
		}

		private void ToggleMapOpacity(object sender, EventArgs e)
		{
			toggleOpacity.Checked = !toggleOpacity.Checked;
			Repaint();
		}

		private void ToggleSingleRender(object sender, EventArgs e)
		{
			forceSingleMapRender.Checked = !forceSingleMapRender.Checked;

		}
	}
}

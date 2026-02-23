using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using WorldForge;
using WorldForge.Coordinates;
using WorldForge.Maps;
using Region = WorldForge.Regions.Region;
using Timer = System.Windows.Forms.Timer;

namespace WorldForgeToolbox
{
	public partial class WorldViewer : ToolboxForm
	{
		private class DimensionView
		{
			public bool isFromDisk;
			public Server? server;
			public World world;
			public Dimension dimension;
			public RegionMapCache maps;
			public List<RegionLocation> currentRenders = new List<RegionLocation>();
			public CancellationTokenSource? cancellationTokenSource;

			public Dictionary<UUID, PlayerAccountData> playerAccountDatas = new Dictionary<UUID, PlayerAccountData>();

			public bool Dirty { get; private set; }

			public bool IsFromDisk => world.SourceDirectory != null;

			public DimensionView(string file, bool isServer, bool loadMapCache = true)
			{
				if (isServer)
				{
					server = Server.FromDirectory(file, null);
					world = server.MainWorld;
				}
				else
				{
					world = World.Load(Path.GetDirectoryName(file));
				}
				if (world.HasOverworld && world.Overworld.regions.Count > 0) dimension = world.Overworld;
				else if (world.HasNether && world.Nether.regions.Count > 0) dimension = world.Nether;
				else if (world.HasTheEnd && world.TheEnd.regions.Count > 0) dimension = world.TheEnd;
				dimension ??= world.Overworld ?? world.Nether ?? world.TheEnd;
				cancellationTokenSource = new CancellationTokenSource();
				if (loadMapCache) LoadOrCreateRenderCache();
				else maps = new RegionMapCache();
			}

			public DimensionView(World world)
			{
				this.world = world;
				if (world.HasOverworld && world.Overworld.regions.Count > 0) dimension = world.Overworld;
				else if (world.HasNether && world.Nether.regions.Count > 0) dimension = world.Nether;
				else if (world.HasTheEnd && world.TheEnd.regions.Count > 0) dimension = world.TheEnd;
				dimension ??= world.Overworld ?? world.Nether ?? world.TheEnd;
				cancellationTokenSource = new CancellationTokenSource();
				maps = new RegionMapCache();
			}

			public void Dispose(bool saveCache = true)
			{
				cancellationTokenSource?.Cancel();
				if (saveCache && Dirty) SaveRenderCache();
				maps = null!;
				world = null!;
				dimension = null!;
			}

			public void MarkDirty()
			{
				Dirty = true;
			}

			public DialogResult SaveIfRequiredAndConfirmedByUser()
			{
				if (Dirty)
				{
					var result = MessageBox.Show("Save modified render cache?", "Unsaved Render Cache", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
					if (result == DialogResult.Yes) SaveRenderCache();
					return result;
				}
				return DialogResult.None;
			}

			public void SaveRenderCache()
			{
				if (!IsFromDisk) throw new InvalidOperationException("Cannot save render cache for a newly created world.");
				var cachePath = Path.Combine(dimension.AbsoluteSourceDirectory, "region_map_cache.dat");
				if (!Directory.Exists(Path.GetDirectoryName(cachePath))) return;
				maps.Save(cachePath);
				Dirty = false;
			}

			public void LoadOrCreateRenderCache()
			{
				if (!IsFromDisk)
				{
					maps = new RegionMapCache();
					return;
				}
				var cachePath = Path.Combine(dimension.AbsoluteSourceDirectory, "region_map_cache.dat");
				maps = File.Exists(cachePath) ? RegionMapCache.Load(cachePath) : new RegionMapCache();
				Dirty = false;
			}

			public void ClearRenderCache(bool deleteCacheFile)
			{
				maps.Clear();
				if (IsFromDisk)
				{
					var cachePath = Path.Combine(dimension.AbsoluteSourceDirectory, "region_map_cache.dat");
					if (File.Exists(cachePath) && deleteCacheFile)
					{
						File.Delete(cachePath);
					}
				}
				Dirty = false;
			}

			public bool SwitchDimension(Dimension dim, bool saveMapCache = true)
			{
				if (saveMapCache)
				{
					if (SaveIfRequiredAndConfirmedByUser() == DialogResult.Cancel)
					{
						return false;
					}
				}
				cancellationTokenSource?.Cancel();
				cancellationTokenSource = new CancellationTokenSource();
				dimension = dim;
				currentRenders.Clear();
				LoadOrCreateRenderCache();
				return true;
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

		public static WorldViewer? Instance { get; private set; }

		private DimensionView? view;

		private BlockCoord2D? focusPosition;

		private BlockCoord2D? hoveredBlockPos;
		private PlayerData? hoveredPlayer;
		private RegionLocation? hoveredRegion;

		private List<RegionLocation> visibleUnrenderedRegions = new List<RegionLocation>();
		private int RunningRenderTasks => view?.currentRenders.Count ?? 0;
		private bool processNewRenders = true;

		private Brush voidBackgroundBrush = new HatchBrush(HatchStyle.BackwardDiagonal, Color.FromArgb(64, Color.Gray), Color.Transparent);
		private Brush regionBackgroundBrush = Brushes.Black;
		private Brush currentRenderBrush = new SolidBrush(Color.FromArgb(48, 128, 128, 128));
		private Pen spawnMarker = new Pen(Color.DarkOrange, 4);
		private Pen playerMarker = new Pen(Color.LightBlue, 3);
		private Pen missingRenderPen = new Pen(Color.FromArgb(128, 128, 128, 128), 1);
		private Brush darkenMapBrush = new SolidBrush(Color.FromArgb(160, Color.Black));
		private Brush outdatedMapBrush = new SolidBrush(Color.FromArgb(100, Color.Gray));

		private Pen regionGridPen = new Pen(Color.Gray, 1);
		private Pen chunkGridPen = new Pen(Color.FromArgb(128, Color.Gray), 1);

		private int _zoom = 4;

		private Timer statusLabelUpdater = new Timer()
		{
			Interval = 100
		};

		public static WorldViewer GetInstance(string? file = null)
		{
			if (Instance != null && !Instance.IsDisposed)
			{
				Instance.BringToFront();
				if (file != null) Instance.OpenWorld(file);
				return Instance;
			}
			else
			{
				var window = new WorldViewer(file);
				window.Show();
				return window;
			}
		}

		public WorldViewer(string? inputFile) : base(inputFile)
		{
			if (!Instance?.IsDisposed ?? false)
			{
				throw new InvalidOperationException("Only one instance of WorldViewer can be open at a time.");
			}
			Instance = this;
			InitializeComponent();
			viewport.UnitScale = 16;
			viewport.MouseUp += OnViewportMouseUp;
			viewport.MouseMove += OnViewportMouseMove;
			viewport.MouseLeave += OnViewportMouseLeave;
			viewport.DoubleClick += OnViewportDoubleClick;
			viewport.Cursor = Cursors.SizeAll;
			toggleGrid.Checked = true;
			statusLabelUpdater.Tick += UpdateStatusStrip;
			statusLabelUpdater.Start();
		}

		protected override void OnShown(EventArgs e)
		{
			if (string.IsNullOrEmpty(inputFileArg))
			{
				if (FileDialogUtility.OpenFileDialog("WorldViewer", out inputFileArg, FileDialogUtility.LEVEL_FILTER, FileDialogUtility.ALL_FILES_FILTER))
				{
					OpenWorld(inputFileArg);
				}
			}
			else
			{
				OpenWorld(inputFileArg);
			}
		}

		public void OpenWorld(string file)
		{
			if (!File.Exists(file) && !Directory.Exists(file))
			{
				MessageBox.Show("File or directory does not exist.");
				return;
			}
			bool isServer = false;
			if (Path.GetExtension(file).Equals(".properties", StringComparison.OrdinalIgnoreCase)
				|| Directory.Exists(file) && File.Exists(Path.Combine(file, "server.properties")))
			{
				isServer = true;
			}

			if (view?.world.SourceDirectory == file) return;
			view?.Dispose();
			view = new DimensionView(file, isServer);
			InitializeView();
		}

		public void OpenWorld(World world)
		{
			view?.Dispose();
			view = new DimensionView(world);
			InitializeView();
		}

		private void InitializeView()
		{
			float x;
			float z;
			if (focusPosition.HasValue)
			{
				x = focusPosition.Value.x;
				z = focusPosition.Value.z;
			}
			else
			{
				x = view.world.LevelData.spawnpoint.Position.x;
				z = view.world.LevelData.spawnpoint.Position.z;
			}
			viewport.Center = new System.Numerics.Vector2(x, z);
			dimensionSelector.DropDownItems.Clear();
			if (view.server != null)
			{
				foreach (var kvp in view.server.Worlds)
				{
					var name = kvp.Key;
					CreateDimensionMenuItem(kvp.Value.Dimensions.First().Value, name);
				}
			}
			else
			{
				foreach (var dim in view.world.Dimensions)
				{
					CreateDimensionMenuItem(dim.Value);
				}
			}
			Invalidate(true);
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			if (view?.SaveIfRequiredAndConfirmedByUser() == DialogResult.Cancel)
			{
				e.Cancel = true;
				return;
			}
			base.OnFormClosing(e);
			view?.Dispose(false);
		}

		public void GoToPosition(BlockCoord2D pos, int? zoomLevel = null)
		{
			focusPosition = pos;
			viewport.Center = new System.Numerics.Vector2(pos.x, pos.z);
			if (zoomLevel.HasValue) viewport.Zoom = zoomLevel.Value;
			viewport.Repaint();
		}

		private string GetDimensionName(Dimension dim)
		{
			string readableName = dim.dimensionID.ID.FullID.Replace("minecraft:", "");
			readableName = readableName.Substring(0, 1).ToUpper() + readableName.Substring(1);
			return readableName;
		}

		private void ShowDimension(Dimension dim)
		{
			if (view == null) return;
			view.SwitchDimension(dim);
			dimensionSelector.Text = GetDimensionName(view.dimension);
			visibleUnrenderedRegions.Clear();
			Invalidate(true);
		}

		private void CreateDimensionMenuItem(Dimension dim, string name = null)
		{
			if (dim == null) return;

			var button = new ToolStripMenuItem(name ?? GetDimensionName(dim))
			{
				Tag = dim
			};
			dimensionSelector.DropDownItems.Add(button);
		}

		private Point WorldToScreenPoint(BlockCoord2D pos) => viewport.MapToScreenPos(new System.Numerics.Vector2(pos.x, pos.z));

		private BlockCoord2D ScreenToBlockPos(Point screenPos, Rectangle clipRectangle)
		{
			var pos = viewport.ScreenToMapPos(screenPos);
			return new BlockCoord2D((int)pos.X, (int)pos.Y);
		}

		private void OnDraw(object sender, PaintEventArgs e)
		{
			var g = e.Graphics;
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.Clear(Color.Transparent);
			g.FillRectangle(voidBackgroundBrush, new Rectangle(0, 0, e.ClipRectangle.Width, e.ClipRectangle.Height));
			if (view?.dimension != null)
			{
				DrawDimension(e, g, view.dimension);
			}
			else
			{
				viewport.DrawDisabled(g, "No world loaded");
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
				var rect = GetRegionRectangle(r.Key);
				g.FillRectangle(regionBackgroundBrush, rect);
				if (!rect.IntersectsWith(e.ClipRectangle)) continue;
				var bmp = RequestSurfaceMap(r.Value, true, out var renderUpToDate);
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
						if (!renderUpToDate)
						{
							g.FillRectangle(outdatedMapBrush, rect);
							// Queue for re-rendering when outdated
							if (regenerateOutdatedMaps.Checked) visibleUnrenderedRegions.Add(r.Key);
						}
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
			}
			if (toggleGrid.Checked)
			{
				viewport.DrawGrid(g, regionGridPen, 512, true);
				if (viewport.Zoom >= 5) viewport.DrawGrid(g, chunkGridPen, 16);
				// Draw origin lines
				viewport.DrawHorizontalGuide(g, Pens.Red, 0);
				viewport.DrawVerticalGuide(g, Pens.Blue, 0);
				viewport.DrawMarker(g, regionGridPen, 0, 0, MapView.MarkerShape.Diamond, 6);
			}
			if (hoveredRegion.HasValue && hoveredPlayer == null)
			{
				g.DrawRectangle(hoverOutlinePen, GetRegionRectangle(hoveredRegion.Value));
			}
			var spawnPos = view.world.LevelData.spawnpoint.Position;
			viewport.DrawMarker(g, spawnMarker, spawnPos.x, spawnPos.z, MapView.MarkerShape.Cross, 8, "Spawn");
			if (togglePlayers.Checked)
			{
				foreach (var player in view.world.PlayerData.Values)
				{
					var username = view.GetPlayerAccountData(player.player.uuid).GetUsername();
					var avatar = view.GetPlayerAccountData(player.player.uuid).GetAvatar();
					bool hovered = hoveredPlayer == player;
					Pen pen = hovered ? Pens.Red : playerMarker;
					var playerPos = player.player.position.Block;
					viewport.DrawIcon(g, avatar, pen, playerPos.x, playerPos.z, 16, hovered, username);
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

		private Rectangle GetRegionRectangle(RegionLocation location)
		{
			var pos = WorldToScreenPoint(location.GetBlockCoord(0, 0, 0));
			var rect = new Rectangle(pos, new Size(32 * viewport.ZoomScale, 32 * viewport.ZoomScale));
			return rect;
		}

		private void ProcessRenderQueue()
		{
			if (!processNewRenders) return;
			int maxConcurrent = forceSingleMapRender.Checked ? 1 : Environment.ProcessorCount - 2;
			if (view != null && RunningRenderTasks < maxConcurrent)
			{
				foreach (var pos in visibleUnrenderedRegions.OrderBy(GetRenderPriority))
				{
					var region = view.dimension.GetRegion(pos);
					if (!view.maps.cache.TryGetValue(pos, out var entry))
					{
						entry = new RegionMapCache.Entry(null, DateTime.MinValue);
						view.maps.cache[pos] = entry;
					}
					entry.regionTimestamp = region.sourceFilePaths?.MainFileLastWriteTimeUtc ?? DateTime.MinValue;
					view.currentRenders.Add(pos);
					visibleUnrenderedRegions.Remove(pos);

					entry.normalRender = BeginRender(region, false);
					if (RunningRenderTasks >= maxConcurrent)
					{
						break;
					}
					viewport.Repaint();
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
					RenderRegionMap(region, wfBitmap.bitmap, res, token);
				},
				(r, token) =>
				{
					view.currentRenders.Remove(region.regionPos);
					if (token.IsCancellationRequested) return;
					r.renderComplete = true;
					view.MarkDirty();
					viewport.Repaint();
				},
				view.cancellationTokenSource.Token
			);
		}

		private void OnOpenWorldClick(object sender, EventArgs e)
		{
			if (FileDialogUtility.OpenFileDialog("WorldViewer", out var file, FileDialogUtility.LEVEL_FILTER, FileDialogUtility.ALL_FILES_FILTER))
			{
				OpenWorld(file);
			}
		}

		private void OnOpenServerWorldClick(object sender, EventArgs e)
		{
			if (FileDialogUtility.OpenWorldFolderDialog("ServerWorldViewer", out var folder))
			{
				OpenWorld(folder);
			}
		}

		private Bitmap? RequestSurfaceMap(Region region, bool allowHighQuality, out bool upToDate)
		{
			// Check if we have a cached version that is up to date
			if (view!.maps.TryGet(region.regionPos, out var entry))
			{
				upToDate = entry.regionTimestamp >= (region.sourceFilePaths?.MainFileLastWriteTimeUtc ?? DateTime.MinValue);
				if (allowHighQuality && (entry.highQualityRender?.renderComplete ?? false))
				{
					return entry.highQualityRender.bitmap;
				}
				return entry.normalRender.bitmap;
			}
			else
			{
				upToDate = false;
				return null;
			}
		}

		private int GetRenderPriority(RegionLocation location)
		{
			//Lower means higher
			var centerBlock = new BlockCoord2D((int)viewport.Center.X, (int)viewport.Center.Y);
			var diffX = Math.Abs(location.x - centerBlock.Region.x);
			var diffZ = Math.Abs(location.z - centerBlock.Region.z);
			return Math.Max(diffX, diffZ);
		}

		private void RenderRegionMap(Region region, Bitmap bitmap, int resolution, CancellationToken token)
		{
			try
			{
				int blocksPerPixel = Math.Max(1, 512 / resolution);
				Region loaded;
				if (region.IsLoaded)
				{
					loaded = region.Clone();
					foreach(var chunk in loaded.chunks)
					{
						if (chunk != null && !chunk.IsLoaded) chunk.Load(WorldForge.IO.ChunkLoadFlags.Blocks);
					}
				}
				else loaded = region.LoadClone(true, false, WorldForge.IO.ChunkLoadFlags.Blocks);
				bool fullRes = resolution == 512;
				var data = bitmap.LockBits(new Rectangle(0, 0, resolution, resolution), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
				Parallel.For(0, resolution, x =>
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
						var color = SurfaceMapGenerator.GetSurfaceMapColor(loaded, bx, bz, HeightmapType.AllBlocks, fullRes, MapColorPalette.Default);
						//bitmap.SetPixel(x, z, color);
						//Copy pixel data directly for better performance
						unsafe
						{
							byte* ptr = (byte*)data.Scan0 + z * data.Stride + x * 4;
							ptr[0] = color.b;
							ptr[1] = color.g;
							ptr[2] = color.r;
							ptr[3] = color.a;
						}
					}
				});
				bitmap.UnlockBits(data);
			}
			catch (Exception e)
			{
				throw;
			}
			viewport.Repaint();
		}

		private void OnViewportMouseUp(object? sender, MouseEventArgs e)
		{
			if (view == null) return;
			if (e.Button == MouseButtons.Right)
			{
				var blockPos = ScreenToBlockPos(e.Location, viewport.ClientRectangle);
				view.maps.TryGet(blockPos.Region, out var render);
				view.dimension.TryGetRegion(blockPos.Region, out var region);
				//Show context menu
				ContextMenuStrip menu = new ContextMenuStrip();
				if (region != null)
				{
					menu.Items.Add("Open Region").Click += (s, ev) =>
					{
						if (view.dimension.TryGetRegion(blockPos.Region, out var r))
						{
							var viewer = new RegionViewer(r.sourceFilePaths.mainPath);
							viewer.Show();
						}
					};
					menu.Items.Add("Regenerate").Click += (s, ev) =>
					{
						if (view.maps.cache.TryGetValue(blockPos.Region, out var entry) && entry.normalRender.renderComplete)
						{
							entry.normalRender = BeginRender(region, false);
							entry.regionTimestamp = region.sourceFilePaths?.MainFileLastWriteTimeUtc ?? DateTime.MinValue;
							view.currentRenders.Add(region.regionPos);
							viewport.Repaint();
						}
					};
					menu.Items.Add("Render High Resolution").Click += (s, ev) =>
					{
						if (view.maps.cache.TryGetValue(blockPos.Region, out var entry) && entry.normalRender.renderComplete)
						{
							entry.highQualityRender = BeginRender(region, true);
							viewport.Repaint();
						}
						else
						{
							MessageBox.Show("Normal render not complete yet. Please wait.");
						}
					};
					menu.Items.Add(new ToolStripSeparator());
					menu.Items.Add(new ToolStripLabel("Region Timestamp: " + region.sourceFilePaths?.MainFileLastWriteTimeUtc));
					menu.Items.Add(new ToolStripLabel("Render Timestamp: " + render?.regionTimestamp ?? "-"));
					menu.Items.Add(new ToolStripSeparator());
				}
				menu.Items.Add("Jump to Spawn").Click += (s, ev) =>
				{
					var x = view.world.LevelData.spawnpoint.Position.x;
					var z = view.world.LevelData.spawnpoint.Position.z;
					viewport.Center = new System.Numerics.Vector2(x, z);
					viewport.Repaint();
				};
				menu.Show(viewport, e.Location);
				viewport.Repaint();
			}
		}

		private void OnViewportMouseMove(object? sender, MouseEventArgs e)
		{
			if (view == null) return;
			hoveredBlockPos = ScreenToBlockPos(e.Location, viewport.ClientRectangle);
			if (!viewport.IsMouseDown)
			{
				var regPos = hoveredBlockPos.Value.Region;
				hoveredRegion = view.dimension.HasRegion(regPos) ? hoveredBlockPos.Value.Region : null;
				hoveredPlayer = null;
				if (togglePlayers.Checked)
				{
					foreach (var player in view!.world.PlayerData.Values)
					{
						var playerScreenPos = WorldToScreenPoint(player.player.position.Block.XZ);
						var distance = Math.Sqrt(Math.Pow(playerScreenPos.X - e.Location.X, 2) + Math.Pow(playerScreenPos.Y - e.Location.Y, 2));
						if (distance <= 16)
						{
							hoveredPlayer = player;
						}
					}
				}
			}
			viewport.Cursor = hoveredPlayer != null ? Cursors.Hand : Cursors.SizeAll;
			viewport.Repaint();
		}

		private void OnViewportMouseLeave(object? sender, EventArgs e)
		{
			hoveredBlockPos = null;
			hoveredRegion = null;
			hoveredPlayer = null;
			viewport.Repaint();
		}

		private void OnViewportDoubleClick(object? sender, EventArgs _)
		{
			if (view == null) return;
			if (hoveredPlayer != null)
			{
				try
				{
					var playerPath = Path.Combine(view.world.SourceDirectory, "playerdata", hoveredPlayer.player.uuid.ToString(true) + ".dat");
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
					if(r.sourceFilePaths != null && File.Exists(r.sourceFilePaths.mainPath))
					{
						var viewer = new RegionViewer(r.sourceFilePaths.mainPath);
						viewer.Show();
					}
					else
					{
						MessageBox.Show($"Region file for location {hoveredRegion.Value} not found.", "File not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			}
		}

		private void UpdateStatusStrip(object? sender, EventArgs e)
		{
			if (view == null) return;
			string text;
			if (hoveredPlayer != null) text = $"Player: {view.GetPlayerAccountData(hoveredPlayer.player.uuid).GetUsername()} {hoveredPlayer.player.position.Block}";
			else if (hoveredBlockPos != null) text = $"Block {hoveredBlockPos.Value} | Region {hoveredBlockPos.Value.Region}";
			else text = "";
			if (text != statusLabel.Text)
			{
				statusLabel.Text = text;
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
			viewport.Repaint();
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
			viewport.Zoom++;
			viewport.Repaint();
		}

		private void zoomOut_Click(object sender, EventArgs e)
		{
			viewport.Zoom--;
			viewport.Repaint();
		}

		private void jumpToSpawn_Click(object sender, EventArgs e)
		{
			if (view == null) return;
			var x = view.world.LevelData.spawnpoint.Position.x;
			var z = view.world.LevelData.spawnpoint.Position.z;
			viewport.Center = new System.Numerics.Vector2(x, z);
			viewport.Repaint();
		}

		private void toolboxButton_Click(object sender, EventArgs e)
		{
			Toolbox.Instance.Return();
			Close();
		}

		private void ToggleGrid(object sender, EventArgs e)
		{
			toggleGrid.Checked = !toggleGrid.Checked;
			viewport.Repaint();
		}

		private void TogglePlayers(object sender, EventArgs e)
		{
			togglePlayers.Checked = !togglePlayers.Checked;
			viewport.Repaint();
		}

		private void ToggleMapOpacity(object sender, EventArgs e)
		{
			toggleOpacity.Checked = !toggleOpacity.Checked;
			viewport.Repaint();
		}

		private void ToggleSingleRender(object sender, EventArgs e)
		{
			forceSingleMapRender.Checked = !forceSingleMapRender.Checked;
		}

		private void regenerateOutdatedMaps_Click(object sender, EventArgs e)
		{
			regenerateOutdatedMaps.Checked = !regenerateOutdatedMaps.Checked;
		}

		private void editLevelDat_Click(object sender, EventArgs e)
		{
			if(view.world != null)
			{
				string levelDatPath = Path.Combine(view.world.SourceDirectory, "level.dat");
				if(File.Exists(levelDatPath))
				{
					var viewer = new NBTViewer(levelDatPath);
					viewer.Show();
				}
				else
				{
					MessageBox.Show("No level.dat file was found.", "File not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}
	}
}

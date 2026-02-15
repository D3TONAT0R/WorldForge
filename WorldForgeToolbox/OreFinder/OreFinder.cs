using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using WorldForge.Coordinates;

namespace WorldForgeToolbox
{
	public partial class OreFinder : ToolboxForm
	{
		public static OreFinder Instance { get; private set; }

		private readonly Font textFont = new Font("Consolas", 8);
		PointF[] triangle =
		{
			new PointF( 0, -10),  // tip
			new PointF(-8, 10), // left
			new PointF(0, 5), // middle
			new PointF( 8, 10)  // right
		};
		StringFormat centerFormat = new StringFormat
		{
			Alignment = StringAlignment.Center,       // horizontal
			LineAlignment = StringAlignment.Center    // vertical
		};
		Pen prevPlayerPen = new Pen(Color.FromArgb(128, 128, 128, 128), 2);
		Pen pathPen = new Pen(Color.FromArgb(64, 128, 128, 128), 1);

		private string? regionDirectory;
		private Report? report;
		private List<BlockCoord> previousPlayerPositions = new List<BlockCoord>();

		private BlockCoord PlayerPos
		{
			get
			{
				return new BlockCoord(
					(int)playerXControl.Value,
					(int)playerYControl.Value,
					(int)playerZControl.Value
				);
			}
			set
			{
				playerXControl.Value = value.x;
				playerYControl.Value = value.y;
				playerZControl.Value = value.z;
			}
		}

		private float PlayerYaw
		{
			get => (float)playerYawControl.Value;
			set => playerYawControl.Value = (decimal)value;
		}

		private int YMin
		{
			get => (int)yMinControl.Value;
			set => yMinControl.Value = value;
		}
		private int YMax
		{
			get => (int)yMaxControl.Value;
			set => yMaxControl.Value = value;
		}

		private SearchProfile ActiveSearchProfile => profileSelector.SelectedItem as SearchProfile;

		public OreFinder()
		{
			InitializeComponent();
			profileSelector.Items.Add(SearchProfile.Netherite);
			profileSelector.Items.Add(SearchProfile.Diamond);
			profileSelector.SelectedIndex = 0;
			viewport.Cursor = Cursors.SizeAll;
			viewport.CheckInteractivity = () => !PlayerPos.IsZero;
		}

		private void OnOpenClick(object sender, EventArgs e)
		{
			if (PlayerPos.IsZero)
			{
				MessageBox.Show("No player position data. Copy your nether coordinates to the clipboard first.", "No Position Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			using (var dialog = new FolderBrowserDialog())
			{
				dialog.Description = "Select nether region cache folder";

				if (dialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
				{
					regionDirectory = dialog.SelectedPath;
					GenerateReport();
				}
			}
		}

		private void OnReloadClick(object sender, EventArgs e)
		{
			if (regionDirectory != null && PlayerPos.IsZero == false)
			{
				GenerateReport();
			}
		}

		private void GenerateReport()
		{
			report = Report.Create(regionDirectory, PlayerPos.x, PlayerPos.z, 256, ActiveSearchProfile);
			report.SortVeinsByDistance(PlayerPos.x, PlayerPos.z);
			Invalidate(true);
		}

		private void UpdatePosition(float x, float y, float z, float yaw)
		{
			if (!PlayerPos.IsZero)
			{
				previousPlayerPositions.Add(PlayerPos);
				if (previousPlayerPositions.Count > 50)
				{
					previousPlayerPositions.RemoveAt(0);
				}
			}
			PlayerPos = new BlockCoord((int)x, (int)y, (int)z);
			viewport.SetCenter(PlayerPos.x, PlayerPos.z);
			yaw = (yaw % 360f) % 360f;
			PlayerYaw = yaw;
			report?.SortVeinsByDistance(PlayerPos.x, PlayerPos.z);
			Invalidate(true);
		}

		private void OnDraw(object? sender, PaintEventArgs e)
		{
			var g = e.Graphics;
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.Clear(Color.FromArgb(20, 20, 20));

			if (PlayerPos.IsZero)
			{
				viewport.DrawDisabled(g, "No position data");
				return;
			}

			//Draw grid
			viewport.DrawGrid(g, Pens.DimGray, 16, true);
			var playerChunkPos = PlayerPos.Chunk.BlockCoord;

			//Previous player positions
			for (var i = 0; i < previousPlayerPositions.Count; i++)
			{
				var prev = previousPlayerPositions[i];
				var next = (i < previousPlayerPositions.Count - 1) ? previousPlayerPositions[i + 1] : PlayerPos;
				viewport.DrawLine(g, pathPen, prev.x, prev.z, next.x, next.z);
				viewport.DrawMarker(g, prevPlayerPen, prev.x, prev.z, MapView.MarkerShape.Cross, 5);
			}

			//Draw veins
			if (report != null)
			{
				foreach (var vein in report.veins)
				{
					if (vein.pos.y < YMin || vein.pos.y > YMax)
					{
						continue;
					}
					var screenPos = viewport.MapToScreenPos(vein.pos.x, vein.pos.z);
					if (screenPos.X < 0 || screenPos.X > e.ClipRectangle.Width || screenPos.Y < 0 || screenPos.Y > e.ClipRectangle.Height)
					{
						//Outside of view
						continue;
					}
					int r = 4 + Math.Min(vein.count * 2, 5);
					g.FillEllipse(ActiveSearchProfile.veinBrush, screenPos.X - r, screenPos.Y - r, r + r, r + r);
					g.DrawString(vein.count.ToString(), textFont, Brushes.Black, screenPos.X, screenPos.Y, centerFormat);
					PrintCoordinates(e, vein.pos, 8, 8);
				}
				g.DrawString("Veins found: " + report.veins.Count, textFont, Brushes.Gray, 30, 30);
				g.DrawString("Nearest vein: " + report.veins[0], textFont, Brushes.Gray, 30, 45);
			}
			else
			{
				g.DrawString("No Vein Data", textFont, Brushes.OrangeRed, 30, 30);
			}

			//Current player pos
			var playerScreenPos = viewport.MapToScreenPos(PlayerPos.x, PlayerPos.z);
			g.TranslateTransform(playerScreenPos.X, playerScreenPos.Y);
			g.RotateTransform(PlayerYaw + 180);
			g.DrawPolygon(Pens.White, triangle);
			g.ResetTransform();
			// PrintCoordinates(e, playerPos, 0, 15);
		}

		private void PrintCoordinates(PaintEventArgs e, BlockCoord pos, float offX, int offY)
		{
			var screen = viewport.MapToScreenPos(pos.x, pos.z);
			e.Graphics.DrawString($"{pos.x}, {pos.z}\nY {pos.y}", textFont, Brushes.White, screen.X + offX, screen.Y + offY);
		}

		#region Clipboard
		private const int WM_CLIPBOARDUPDATE = 0x031D;

		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool AddClipboardFormatListener(IntPtr hwnd);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			AddClipboardFormatListener(Handle);
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			RemoveClipboardFormatListener(Handle);
			base.OnHandleDestroyed(e);
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == WM_CLIPBOARDUPDATE)
			{
				OnClipboardChanged();
			}

			base.WndProc(ref m);
		}

		private void OnClipboardChanged()
		{
			// Keep this fast; clipboard can change frequently.
			if (Clipboard.ContainsText())
			{
				// /execute in minecraft:the_nether run tp @s 6.94 62.00 0.47 188.29 9.75
				string text = Clipboard.GetText().ToLower();
				string prefix = $"/execute in {ActiveSearchProfile.dimensionId} run tp @s ";
				if (text.StartsWith(prefix))
				{
					var parts = text.Substring(prefix.Length).Split(' ');
					if (parts.Length >= 3)
					{
						if (float.TryParse(parts[0], out float x) && float.TryParse(parts[1], out float y) && float.TryParse(parts[2], out float z))
						{
							float yaw = 0;
							if (parts.Length >= 4) float.TryParse(parts[3], out yaw);
							UpdatePosition(x, y, z, yaw);
						}
					}
				}
			}
		}
		#endregion

		private void zoomIn_Click(object sender, EventArgs e)
		{
			viewport.Zoom++;
		}

		private void zoomOut_Click(object sender, EventArgs e)
		{
			viewport.Zoom--;
		}

		private void FocusStrip(object sender, EventArgs e)
		{
			toolStrip1.Focus();
		}

		private void OnProfileChanged(object sender, EventArgs e)
		{
			if (report != null && !PlayerPos.IsZero)
			{
				GenerateReport();
			}
			YMin = ActiveSearchProfile.displayYMin;
			YMax = ActiveSearchProfile.displayYMax;
		}

		private void recenter_Click(object sender, EventArgs e)
		{
			viewport.SetCenter(PlayerPos.x, PlayerPos.z);
		}

		private void returnToToolbox_Click(object sender, EventArgs e)
		{
			ReturnToToolbox();
		}
	}
}
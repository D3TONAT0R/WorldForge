using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using WorldForge.Coordinates;

namespace NetheriteFinder
{
	public partial class MainForm : Form
	{
		private readonly Font textFont = new Font("Consolas", 8);
		PointF[] triangle =
		{
			new PointF( 0, -10),  // tip
			new PointF(-8, 10), // left
			new  PointF(0, 5), // middle
			new PointF( 8, 10)  // right
		};
		StringFormat centerFormat = new StringFormat
		{
			Alignment = StringAlignment.Center,       // horizontal
			LineAlignment = StringAlignment.Center    // vertical
		};
		Pen prevPlayerPen = new Pen(Color.FromArgb(128, 128, 128, 128), 2);
		Pen pathPen = new Pen(Color.FromArgb(64, 128, 128, 128), 1);

		private Report? report;
		private BlockCoord playerPos;
		private float playerYaw;
		private List<BlockCoord> previousPlayerPositions = new List<BlockCoord>();

		private int yMin = 8;
		private int yMax = 18;
		private int zoom = 5;

		public MainForm()
		{
			InitializeComponent();
		}

		private void OnOpenDialog(object sender, EventArgs e)
		{
			if(playerPos.x == 0 && playerPos.y == 0 && playerPos.z == 0)
			{
				MessageBox.Show("No player position data. Copy your nether coordinates to the clipboard first.", "No Position Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			using (var dialog = new FolderBrowserDialog())
			{
				dialog.Description = "Select nether region cache folder";

				if (dialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
				{
					report = Report.Create(dialog.SelectedPath, playerPos.x, playerPos.z, 256);
					report.SortVeinsByDistance(playerPos.x, playerPos.z);
					canvas.Invalidate();
				}
			}
		}

		private void OnExitDialog(object sender, EventArgs e)
		{
			Application.Exit();
		}

		private void UpdatePosition(float x, float y, float z, float yaw)
		{
			if(playerPos.x != 0 && playerPos.y != 0 && playerPos.z != 0)
			{
				previousPlayerPositions.Add(playerPos);
				if(previousPlayerPositions.Count > 50)
				{
					previousPlayerPositions.RemoveAt(0);
				}
			}
			playerPos = new BlockCoord((int)x, (int)y, (int)z);
			playerYaw = yaw;
			report?.SortVeinsByDistance(playerPos.x, playerPos.z);
		}

		private void OnDraw(object? sender, PaintEventArgs e)
		{
			var g = e.Graphics;
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.Clear(Color.FromArgb(20, 20, 20));
			
			if(playerPos.x == 0 && playerPos.z == 0)
			{
				g.DrawString("No position data", textFont, Brushes.White, e.ClipRectangle.Width / 2f, e.ClipRectangle.Height / 2f, centerFormat);
				return;
			}
			
			//Draw grid
			var playerChunkPos = playerPos.Chunk.BlockCoord;
			//Vertical lines
			for (int i = -20; i <= 20; i++)
			{
				var pos = playerChunkPos + new BlockCoord(i * 16, 0, 0);
				BlockToCanvasCoord(e.ClipRectangle, pos, out var cx, out _);
				g.DrawLine(Pens.DimGray, cx, 0, cx, e.ClipRectangle.Height);
				g.DrawString(pos.x.ToString(), textFont, Brushes.DimGray, cx + 4, 4);
			}
			//Horizontal lines
			for (int i = -20; i <= 20; i++)
			{
				var pos = playerChunkPos + new BlockCoord(0, 0, i * 16);
				BlockToCanvasCoord(e.ClipRectangle, pos, out _, out var cy);
				g.DrawLine(Pens.DimGray, 0, cy, e.ClipRectangle.Width, cy);
				g.DrawString(pos.z.ToString(), textFont, Brushes.DimGray, 4, cy + 4);
			}
			
			//Previous player positions
			for (var i = 0; i < previousPlayerPositions.Count; i++)
			{
				var prev = previousPlayerPositions[i];
				BlockToCanvasCoord(e.ClipRectangle, prev, out var ppx, out var ppy);
				g.DrawLine(prevPlayerPen, ppx - 5, ppy - 5, ppx + 5, ppy + 5);
				g.DrawLine(prevPlayerPen, ppx - 5, ppy + 5, ppx + 5, ppy - 5);
				var next = (i < previousPlayerPositions.Count - 1) ? previousPlayerPositions[i + 1] : playerPos;
				BlockToCanvasCoord(e.ClipRectangle, next, out var npx, out var npy);
				g.DrawLine(pathPen, ppx, ppy, npx, npy);
			}
			
			//Draw veins
			if (report != null)
			{
				foreach (var vein in report.veins)
				{
					if(vein.pos.y < yMin || vein.pos.y > yMax)
					{
						continue;
					}
					BlockToCanvasCoord(e.ClipRectangle, vein.pos, out var vx, out var vz);
					if(vx < 0 || vx > e.ClipRectangle.Width || vz < 0 || vz > e.ClipRectangle.Height)
					{
						//Outside of view
						continue;
					}
					g.FillEllipse(Brushes.DarkOrange, vx - 6, vz - 6, 12, 12);
					g.DrawString(vein.count.ToString(), textFont, Brushes.Black, vx, vz, centerFormat);
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
			BlockToCanvasCoord(e.ClipRectangle, playerPos, out var px, out var py);
			g.TranslateTransform(px, py);
			g.RotateTransform(playerYaw + 180);
			g.DrawPolygon(Pens.White, triangle);
			g.ResetTransform();
			// PrintCoordinates(e, playerPos, 0, 15);
		}
		
		private void BlockToCanvasCoord(Rectangle rect, BlockCoord pos, out float x, out float y)
		{
			x = pos.x - playerPos.x;
			y = pos.z - playerPos.z;
			x *= zoom;
			y *= zoom;
			x += rect.Width * 0.5f;
			y += rect.Height * 0.5f;
		}

		private void PrintCoordinates(PaintEventArgs e, BlockCoord pos, float offX, int offY)
		{
			BlockToCanvasCoord(e.ClipRectangle, pos, out var x, out var y);
			e.Graphics.DrawString($"{pos.x}, {pos.z}\nY {pos.y}", textFont, Brushes.White, x + offX, y + offY);
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
				const string prefix = "/execute in minecraft:the_nether run tp @s ";
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
							canvas.Invalidate();
						}
					}
				}
				// TODO: use text (update UI, parse, etc.)
			}
		}

		#endregion

		private void YMin_ValueChanged(object sender, EventArgs e)
		{
			yMin = (int)yMinControl.Value;
			canvas.Invalidate();
		}

		private void YMax_ValueChanged(object sender, EventArgs e)
		{
			yMax = (int)yMaxControl.Value;
			canvas.Invalidate();
		}

		private void Zoom_ValueChanged(object sender, EventArgs e)
		{
			zoom = (int)zoomControl.Value;
			canvas.Invalidate();
		}
	}
}
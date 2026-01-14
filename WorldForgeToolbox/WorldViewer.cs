using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
		private Dictionary<Region, Bitmap> regionBitmaps = new Dictionary<Region, Bitmap>();

		private BlockCoord center;

		private int zoom = 1;

		public WorldViewer(string file)
		{
			InitializeComponent();
			fileName = file;
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
			pos.x -= spawn.spawnX;
			pos.z -= spawn.spawnZ;
			var x = pos.x / 8;
			var y = pos.z / 8;
			x += clipRectangle.Width / 2;
			y += clipRectangle.Height / 2;
			return new Point(x, y);
		}

		private void OnDraw(object sender, PaintEventArgs e)
		{
			var g = e.Graphics;
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.Clear(Color.Transparent);
			var overworld = world.Overworld;
			if(overworld != null)
			{
				var spawnPos = WorldToScreenPoint(world.LevelData.spawnpoint.Position, e.ClipRectangle);
				g.DrawLine(Pens.Red, spawnPos.X - 4, spawnPos.Y - 4, spawnPos.X + 4, spawnPos.Y + 4);
				g.DrawLine(Pens.Red, spawnPos.X - 4, spawnPos.Y + 4, spawnPos.X + 4, spawnPos.Y - 4);
				foreach(var r in overworld.regions)
				{
					var pos = WorldToScreenPoint(r.Key.GetBlockCoord(0, 0, 0), e.ClipRectangle);
					var rect = new Rectangle(pos, new Size(64, 64));
					var bmp = RequestSurfaceMap(r.Value);
					g.DrawImage(bmp, rect);
					g.DrawRectangle(Pens.DarkGray, rect);

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
			if(regionBitmaps.TryGetValue(region, out var bmp))
			{
				return bmp;
			}
			else
			{
				var bitmap = Bitmaps.Create(64, 64);
				regionBitmaps[region] = ((WinformsBitmap)bitmap).bitmap;
				var task = new Task(() => RenderRegionMap(region, bitmap));
				task.Start();
				return regionBitmaps[region];
			}
		}

		private void RenderRegionMap(WorldForge.Regions.Region region, IBitmap bitmap)
		{
			for(int x = 0; x < 64; x++)
			{
				for(int z = 0; z < 64; z++)
				{
					int bx = x * 8;
					int bz = z * 8;
					var chunk = region.GetChunkAtBlock(new BlockCoord(bx, 0, bz), false);
					if(chunk != null)
					{
						var color = SurfaceMapGenerator.GetSurfaceMapColor(chunk, bx & 15, bz & 15, HeightmapType.AllBlocks, MapColorPalette.Modern);
						bitmap.SetPixel(x, z, color);
					}
				}
			}
			canvas.Invalidate();
		}

		private void OnCanvasScroll(object sender, ScrollEventArgs e)
		{
			MessageBox.Show("Test");
		}
	}
}

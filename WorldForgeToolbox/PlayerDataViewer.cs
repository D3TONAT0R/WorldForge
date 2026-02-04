using System.Net;
using WorldForge;
using WorldForge.IO;
using WorldForge.Items;
using WorldForge.NBT;

namespace WorldForgeToolbox
{
	public partial class PlayerDataViewer : ToolboxForm
	{
		private NBTCompound? data;
		private Player? playerData;
		private Image avatar;

		private StringFormat centerFormat = new StringFormat()
		{
			Alignment = StringAlignment.Center,
			LineAlignment = StringAlignment.Center
		};
		private StringFormat bottomRightFormat = new StringFormat()
		{
			Alignment = StringAlignment.Far,
			LineAlignment = StringAlignment.Far
		};

		public PlayerDataViewer(string? inputFile) : base(inputFile)
		{
			InitializeComponent();
			canvas.Paint += OnDraw;
		}

		private void OnDraw(object? sender, PaintEventArgs e)
		{
			var g = e.Graphics;
			g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
			g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
			g.Clear(Color.Gray);
			if (data == null || playerData == null)
			{
				g.DrawString("No player data loaded.", Font, Brushes.Black, e.ClipRectangle, centerFormat);
				return;
			}
			int w = e.ClipRectangle.Width / 10;
			for (int iy = 0; iy < 4; iy++)
			{
				int y = (int)((3.5f - iy) * w);
				if (iy == 0) y += w / 2;
				for (int ix = 0; ix < 9; ix++)
				{
					sbyte slot = (sbyte)(ix + iy * 9);
					int x = ix * w + w / 2;
					DrawSlot(g, x, y, w, playerData.inventory.GetItem(slot));
				}
			}
			if(avatar != null)
			{
				g.DrawImage(avatar, 16, e.ClipRectangle.Height - 80, 64, 64);
			}
			else
			{
				g.DrawRectangle(Pens.Red, 16, e.ClipRectangle.Height - 80, 64, 64);
			}
		}

		private void DrawSlot(Graphics g, int x, int y, int size, ItemStack? stack)
		{
			g.DrawRectangle(Pens.Black, x, y, size, size);
			if (stack != null && !stack.IsNull && stack.item.id != null)
			{
				string id = stack.item.id.ID.id;
				g.DrawString(id, Font, Brushes.Black, x + size / 2, y + size / 2, centerFormat);
				g.DrawString("x" + stack.count, Font, Brushes.Black, x + size - 2, y + size - 2, bottomRightFormat);
			}
		}

		protected override void OnShown(EventArgs e)
		{
			if (data != null) return;
			if (string.IsNullOrEmpty(inputFileArg))
			{
				OpenFileUtility.OpenFileDialog(out inputFileArg, OpenFileUtility.NBT_DATA_FILTER, OpenFileUtility.ALL_FILES_FILTER);
			}
			if (inputFileArg != null)
			{
				OpenFile(inputFileArg);
			}
		}

		private void OpenFile(string file)
		{
			var extension = Path.GetExtension(file).ToLower();
			Text = Path.GetFileName(file);
			var nbt = new NBTFile(file);
			data = nbt.contents;
			playerData = new Player(data, GameVersion.DefaultVersion);
			nbtView.DisplayContent(data, "");
			string uuid = Path.GetFileNameWithoutExtension(file);
			string url = $"https://mc-heads.net/avatar/{uuid}/8";
			avatar = null;
			try
			{
				var request = WebRequest.Create(url);
				using var response = request.GetResponse();
				using var stream = response.GetResponseStream();
				avatar = Bitmap.FromStream(stream);
			}
			catch(Exception e)
			{
				avatar = null;
				MessageBox.Show(url + "\n" + e.ToString());
			}
			Invalidate(true);
		}

		private void toolboxButton_Click(object sender, EventArgs e)
		{
			ReturnToToolbox();
		}

		private void openFileButton_Click(object sender, EventArgs e)
		{
			if(OpenFileUtility.OpenFileDialog(out inputFileArg, OpenFileUtility.NBT_DATA_FILTER, OpenFileUtility.ALL_FILES_FILTER))
			{
				OpenFile(inputFileArg);
			}
		}
	}
}

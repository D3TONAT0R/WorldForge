using WorldForge;
using WorldForge.Items;
using WorldForge.NBT;

namespace WorldForgeToolbox
{
	public partial class PlayerDataViewer : ToolboxForm
	{
		private string filename;
		private NBTCompound? data;
		private PlayerData? playerData;
		private PlayerAccountData? accountData;

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
		private StringFormat bottomLeftFormat = new StringFormat()
		{
			Alignment = StringAlignment.Near,
			LineAlignment = StringAlignment.Far
		};
		private StringFormat centerLeftFormat = new StringFormat()
		{
			Alignment = StringAlignment.Near,
			LineAlignment = StringAlignment.Center
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
			int w = (int)(e.ClipRectangle.Width / 11.5f);
			for (int iy = 0; iy < 4; iy++)
			{
				var y = 4f - iy;
				if (iy == 0) y += 0.5f;
				for (int ix = 0; ix < 9; ix++)
				{
					sbyte slot = (sbyte)(ix + iy * 9);
					var x = 1.5f + ix;
					DrawSlot(g, x, y, w, playerData.player.inventory.GetItem(slot));
				}
			}
			DrawSlot(g, 0, 0, w, playerData.player.equipmentSlots.head);
			DrawSlot(g, 0, 1, w, playerData.player.equipmentSlots.chest);
			DrawSlot(g, 0, 2, w, playerData.player.equipmentSlots.legs);
			DrawSlot(g, 0, 3, w, playerData.player.equipmentSlots.feet);
			DrawSlot(g, 0, 4.5f, w, playerData.player.equipmentSlots.offhand);
			var avatar = accountData?.GetAvatar();
			if (avatar != null)
			{
				g.DrawImage(avatar, 16, e.ClipRectangle.Height - 80, 64, 64);
			}
			else
			{
				g.DrawRectangle(Pens.Red, 16, e.ClipRectangle.Height - 80, 64, 64);
			}
			g.DrawString(accountData?.GetUsername() ?? "Loading...", Font, Brushes.Black, e.ClipRectangle.Width / 2f, w, centerFormat);
		}

		private void DrawSlot(Graphics g, float x, float y, int size, ItemStack? stack)
		{
			x += 0.5f;
			y += 0.5f;
			x *= size;
			y *= size;
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
			filename = file;
			Text = Path.GetFileName(file);
			var nbt = new NBTFile(file);
			data = nbt.contents;
			var rootDir = Path.Combine(Path.GetDirectoryName(file), "..");
			var uuid = GetUUID(file);
			playerData = new PlayerData(rootDir, uuid, GameVersion.DefaultVersion);
			accountData = new PlayerAccountData(playerData.uuid);
			nbtView.DisplayContent(data, "");
			Invalidate(true);
			if (uuid != null)
			{
				Task.Run(async () =>
				{
					var username = await accountData.GetUsernameAsync();
					if (username != null) Invoke(() => Text += " - " + username);
				});
				Task.Run(async () =>
				{
					await accountData.GetAvatarAsync();
					Invalidate(true);
				});
			}
		}

		private UUID? GetUUID(string file)
		{
			string s = Path.GetFileNameWithoutExtension(file);
			try
			{
				return new UUID(s);
			}
			catch
			{
				return null;
			}
		}

		private void toolboxButton_Click(object sender, EventArgs e)
		{
			ReturnToToolbox();
		}

		private void openFileButton_Click(object sender, EventArgs e)
		{
			if (OpenFileUtility.OpenFileDialog(out inputFileArg, OpenFileUtility.NBT_DATA_FILTER, OpenFileUtility.ALL_FILES_FILTER))
			{
				OpenFile(inputFileArg);
			}
		}

		private void showGeneralStatistics_Click(object sender, EventArgs e)
		{
			if (playerData.stats != null)
			{
				int totalPlayTime = playerData.stats.data.GetAsCompound("minecraft:custom")?.Get<int>("minecraft:play_time") ?? 0;
				int seconds = totalPlayTime / 20;
				var timespan = TimeSpan.FromSeconds(seconds);
				MessageBox.Show($"Total play time: {(int)timespan.TotalHours}h {timespan.Minutes}m {timespan.Seconds}s");
			}
			else
			{
				MessageBox.Show("No stats data found for this player.");
			}
		}

		private void showInWorldViewer_Click(object sender, EventArgs e)
		{
			if (playerData == null) return;
			string worldDir = Path.Combine(Path.GetDirectoryName(filename) ?? "", "..");
			string levelDat = Path.Combine(worldDir, "level.dat");
			if (File.Exists(levelDat))
			{
				var viewer = WorldViewer.GetInstance(levelDat);
				viewer.GoToPosition(playerData.player.position.Block.XZ);
				viewer.togglePlayers.Checked = true;
				viewer.Show();
			}
			else
			{
				MessageBox.Show("No world was found at this location", "World not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}
}

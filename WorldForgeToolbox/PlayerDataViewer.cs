using WorldForge;
using WorldForge.Items;
using WorldForge.Maps;
using WorldForge.NBT;
using WorldForge.Textures;

namespace WorldForgeToolbox
{
	public partial class PlayerDataViewer : ToolboxForm
	{
		private class SlotDisplay
		{
			public readonly float x;
			public readonly float y;
			public readonly string slotInfo;
			public readonly Func<Player, ItemStack> contentGetter;
			public readonly Func<TreeNode> nodeGetter;

			public SlotDisplay(float x, float y, string slotInfo, Func<Player, ItemStack> contentGetter, Func<TreeNode> nodeGetter)
			{
				this.x = x;
				this.y = y;
				this.slotInfo = slotInfo;
				this.contentGetter = contentGetter;
				this.nodeGetter = nodeGetter;
			}

			public bool DrawAndCheckHover(Graphics g, Player? player, int size, Font font)
			{
				var px = (x + 0.5f) * size;
				var py = (y + 0.5f) * size;
				ItemStack stack = null;
				if (player != null) stack = contentGetter(player);
				DrawBeveledRectangle(g, px, py, size, size, size / 16f, true);
				if (stack != null && !stack.IsNull && stack.item.id != null)
				{
					var id = stack.item.id;
					if (textures?.TryGetTexture(id.ID, out var img) ?? false)
					{
						var bmp = ((WinformsBitmap)img).bitmap;
						int iconSize = (int)(size * 0.8f);
						iconSize -= iconSize % 16;
						int cx = (int)(px + size / 2);
						int cy = (int)(py + size / 2);
						g.DrawImage(bmp, cx - iconSize / 2, cy - iconSize / 2, iconSize, iconSize);
					}
					else
					{
						g.FillRectangle(missingTextureBrush, px + size * 0.15f, py + size * 0.15f, size * 0.7f, size * 0.7f);
						g.DrawString(id.ID.id, font, Brushes.Black, px + size / 2, py + size / 2, centerFormat);
					}
					if (stack.count != 1)
					{
						string count = "x" + stack.count;
						g.DrawString(count, labelFont, Brushes.Black, px + size - size / 16f + 2, py + size - size / 16f + 2, bottomRightFormat);
						g.DrawString(count, labelFont, Brushes.White, px + size - size / 16f, py + size - size / 16f, bottomRightFormat);
					}
				}
				var rect = new RectangleF(px, py, size, size);
				if (rect.Contains(mousePos))
				{
					g.FillRectangle(hoverBrush, rect);
					// Tooltip box
					tooltipText = slotInfo + "\n";
					if (stack == null || stack.IsNull) tooltipText += "(Empty)";
					else tooltipText += $"{stack.item.id?.ID} x{stack.count}";
					return true;
				}
				return false;
			}
		}

		private string filename;
		private NBTCompound? data;
		private PlayerData? playerData;
		private PlayerAccountData? accountData;

		private static ItemTextures? textures;

		private static Brush lightBevelBrush = new SolidBrush(Color.FromArgb(128, Color.White));
		private static Brush hoverBrush = new SolidBrush(Color.FromArgb(64, Color.White));
		private static Brush darkBevelBrush = new SolidBrush(Color.FromArgb(128, Color.Black));
		private static Brush grayLabelBrush = new SolidBrush(Color.FromArgb(64, 64, 64));
		private static Brush missingTextureBrush = new SolidBrush(Color.FromArgb(100, 100, 100));
		private static PointF[] bevelShapePoints = new PointF[6];
		private static StringFormat centerFormat = new StringFormat()
		{
			Alignment = StringAlignment.Center,
			LineAlignment = StringAlignment.Center
		};
		private static StringFormat bottomRightFormat = new StringFormat()
		{
			Alignment = StringAlignment.Far,
			LineAlignment = StringAlignment.Far
		};
		private static StringFormat bottomLeftFormat = new StringFormat()
		{
			Alignment = StringAlignment.Near,
			LineAlignment = StringAlignment.Far
		};
		private static StringFormat centerLeftFormat = new StringFormat()
		{
			Alignment = StringAlignment.Near,
			LineAlignment = StringAlignment.Center
		};
		private static Font labelFont = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Bold);

		private List<SlotDisplay> slotDisplays;

		private static Point mousePos;
		private static SlotDisplay hoveredSlot;
		private static string tooltipText = null;

		public PlayerDataViewer(string? inputFile) : base(inputFile)
		{
			InitializeComponent();
			canvas.Paint += OnDraw;
			slotDisplays = new List<SlotDisplay>();
			// Hotbar
			for (int i = 0; i < 9; i++)
			{
				sbyte slot = (sbyte)i;
				slotDisplays.Add(new SlotDisplay(1.5f + i, 4.5f, "Slot " + slot, p => p.inventory.GetItem(slot), () => GetInventoryNode(slot)));
			}
			// Main inventory
			for (int iy = 0; iy < 3; iy++)
			{
				var y = 3f - iy;
				for (int ix = 0; ix < 9; ix++)
				{
					sbyte slot = (sbyte)(9 + ix + iy * 9);
					slotDisplays.Add(new SlotDisplay(1.5f + ix, y, "Slot " + slot, p => p.inventory.GetItem(slot), () => GetInventoryNode(slot)));
				}
			}
			// Armor and offhand
			slotDisplays.Add(new SlotDisplay(0, 0, "Head Slot", p => p.equipmentSlots.head, () => GetEquipmentNode("head")));
			slotDisplays.Add(new SlotDisplay(0, 1, "Chest Slot", p => p.equipmentSlots.chest, () => GetEquipmentNode("chest")));
			slotDisplays.Add(new SlotDisplay(0, 2, "Legs Slot", p => p.equipmentSlots.legs, () => GetEquipmentNode("legs")));
			slotDisplays.Add(new SlotDisplay(0, 3, "Feet Slot", p => p.equipmentSlots.feet, () => GetEquipmentNode("feet")));
			slotDisplays.Add(new SlotDisplay(0, 4.5f, "Offhand Slot", p => p.equipmentSlots.offhand, () => GetEquipmentNode("offhand")));
		}

		private void OnDraw(object? sender, PaintEventArgs e)
		{
			int slotSize = (int)(e.ClipRectangle.Width / 11.5f);
			int fs = Math.Max(8, slotSize / 6);
			if (labelFont.Size != fs)
			{
				labelFont.Dispose();
				labelFont = new Font(FontFamily.GenericSansSerif, fs, FontStyle.Bold);
			}
			var g = e.Graphics;
			g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
			g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
			g.Clear(Color.Gray);
			tooltipText = null;
			hoveredSlot = null;
			if (data == null || playerData == null)
			{
				g.DrawString("No player data loaded.", Font, Brushes.Black, e.ClipRectangle, centerFormat);
				return;
			}
			foreach (var slot in slotDisplays)
			{
				if (slot.DrawAndCheckHover(g, playerData?.player, slotSize, Font))
				{
					hoveredSlot = slot;
				}
			}
			var avatar = accountData?.GetAvatar();
			if (avatar != null)
			{
				g.DrawImage(avatar, 16, e.ClipRectangle.Height - 80, 64, 64);
			}
			else
			{
				g.DrawRectangle(Pens.Red, 16, e.ClipRectangle.Height - 80, 64, 64);
			}
			g.DrawString(accountData?.GetUsername() ?? "Loading...", labelFont, grayLabelBrush, slotSize * 6.5f, slotSize - 4, centerFormat);
			if (tooltipText != null)
			{
				var tooltipSize = g.MeasureString(tooltipText, Font);
				tooltipSize.Width += 4;
				tooltipSize.Height += 4;
				var tooltipRect = new RectangleF(mousePos.X + 4, mousePos.Y - tooltipSize.Height - 4, tooltipSize.Width, tooltipSize.Height);
				if (tooltipRect.Right > g.VisibleClipBounds.Right) tooltipRect.X = g.VisibleClipBounds.Right - tooltipRect.Width;
				if (tooltipRect.Top < g.VisibleClipBounds.Top) tooltipRect.Y = g.VisibleClipBounds.Top;
				g.FillRectangle(Brushes.LightYellow, tooltipRect);
				g.DrawRectangle(Pens.Black, tooltipRect.X, tooltipRect.Y, tooltipRect.Width, tooltipRect.Height);
				tooltipRect.Inflate(-2, -2);
				g.DrawString(tooltipText, Font, Brushes.Black, tooltipRect, centerLeftFormat);
			}
		}

		private static void DrawBeveledRectangle(Graphics g, float x, float y, float width, float height, float bevelWidth, bool down)
		{
			bevelShapePoints[0] = new PointF(x, y);
			bevelShapePoints[1] = new PointF(x + width, y);
			bevelShapePoints[2] = new PointF(x + width - bevelWidth, y + bevelWidth);
			bevelShapePoints[3] = new PointF(x + bevelWidth, y + bevelWidth);
			bevelShapePoints[4] = new PointF(x + bevelWidth, y + height - bevelWidth);
			bevelShapePoints[5] = new PointF(x, y + height);
			g.FillPolygon(down ? darkBevelBrush : lightBevelBrush, bevelShapePoints);
			bevelShapePoints[0] = new PointF(x + width, y);
			bevelShapePoints[1] = new PointF(x + width, y + height);
			bevelShapePoints[2] = new PointF(x, y + height);
			bevelShapePoints[3] = new PointF(x + bevelWidth, y + height - bevelWidth);
			bevelShapePoints[4] = new PointF(x + width - bevelWidth, y + height - bevelWidth);
			bevelShapePoints[5] = new PointF(x + width - bevelWidth, y + bevelWidth);
			g.FillPolygon(down ? lightBevelBrush : darkBevelBrush, bevelShapePoints);
		}

		protected override void OnShown(EventArgs e)
		{
			if (data != null) return;
			if (string.IsNullOrEmpty(inputFileArg))
			{
				FileDialogUtility.OpenFileDialog("PlayerData", out inputFileArg, FileDialogUtility.NBT_DATA_FILTER, FileDialogUtility.ALL_FILES_FILTER);
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
					Invalidate(true);
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
			if (FileDialogUtility.OpenFileDialog("PlayerData", out inputFileArg, FileDialogUtility.NBT_DATA_FILTER, FileDialogUtility.ALL_FILES_FILTER))
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

		private void loadTexturesFromJar_Click(object sender, EventArgs e)
		{
			if (FileDialogUtility.OpenFileDialog("MinecraftJar", out string jarPath, "Minecraft JAR files (*.jar)|*.jar"))
			{
				textures = ItemTextures.CreateFomMinecraftJar(jarPath, BlockList.allBlocks.Values, ItemList.allItems.Values);
			}
			canvas.Refresh();
		}

		private void OnCanvasMouseMove(object sender, MouseEventArgs e)
		{
			mousePos = e.Location;
			canvas.Refresh();
		}

		private void OnCanvasMouseLeave(object sender, EventArgs e)
		{
			mousePos = Point.Empty;
			canvas.Refresh();
		}

		private void OnCanvasClick(object sender, EventArgs e)
		{
			if(hoveredSlot != null)
			{
				var node = hoveredSlot.nodeGetter();
				nbtView.treeView.SelectedNode = node;
				node.Expand();
				node.EnsureVisible();
			}
		}

		private TreeNode GetInventoryNode(int index)
		{
			var inv = GetNode(nbtView.treeView.Nodes[0], nbtView.Data.GetAsList("Inventory"));
			for (var i = 0; i < inv.Nodes.Count; i++)
			{
				var node = inv.Nodes[i];
				var nbt = (NBTCompound)node.Tag;
				if (nbt.TryGet("Slot", out sbyte slot) && slot == index) return node;
			}
			return null;
		}

		private TreeNode GetEquipmentNode(string slotName)
		{
			var equipmentList = nbtView.Data.GetAsCompound("equipment");
			var eq = GetNode(nbtView.treeView.Nodes[0], equipmentList);
			return GetNode(eq, equipmentList.GetAsCompound(slotName));
		}

		private TreeNode FindNode(string path)
		{
			var node = nbtView.treeView.Nodes[0];
			var parts = path.Split('/');
			foreach(var part in parts)
			{
				var next = GetNode(node, part);
				if(next == null)
				{
					return node;
				}
				node = next;
			}
			return node;
		}

		private TreeNode GetNode(TreeNode parent, object tag)
		{
			foreach(TreeNode node in parent.Nodes)
			{
				if(node.Tag == tag) return node;
			}
			return null;
		}
	}
}

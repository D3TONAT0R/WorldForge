using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WorldForge.Maps;

namespace WorldForgeToolbox
{
	public partial class MapColorPaletteWizard : ToolboxForm
	{
		private MapColorPalette palette;

		public MapColorPaletteWizard()
		{
			InitializeComponent();
			dataGridView1.Columns.Add("ID", "ID");
			dataGridView1.Columns[0].Width = 30;
			dataGridView1.Columns.Add("Color", "Color");
			dataGridView1.Columns[1].Width = 100;
			dataGridView1.Columns.Add("Blocks", "Blocks");
			dataGridView1.Columns[2].Width = 300;
			palette = MapColorPalette.Default;
			UpdateGridView();
		}

		private void UpdateGridView()
		{
			dataGridView1.Rows.Clear();
			if (palette == null) return;
			for (var i = 0; i < palette.mapColorPalette.Count; i++)
			{
				var color = palette.mapColorPalette[i];
				string blockList = string.Join(", ", palette.mappings.Where(kvp => kvp.Value == i).Select(kvp => kvp.Key.id));
				int index = dataGridView1.Rows.Add(i, color.baseColor, blockList);
				dataGridView1.Rows[index].Cells[1].Style.BackColor = Color.FromArgb(color.baseColor.r, color.baseColor.g, color.baseColor.b);
			}

			dataGridView1.AutoResizeColumns();
			dataGridView1.AutoResizeRows();
			dataGridView1.Refresh();
		}

		private void returnButton_Click(object sender, EventArgs e)
		{
			ReturnToToolbox();
		}

		private void generateFromJar_Click(object sender, EventArgs e)
		{
			if(FileDialogUtility.OpenFileDialog(out string jarPath, "Minecraft JAR files (*.jar)|*.jar"))
			{
				palette = MapColorPaletteGenerator.CreateFromMinecraftJar(jarPath, MapColorPalette.Default);
			}
			save.Enabled = palette != null;
			UpdateGridView();
		}

		private void save_Click(object sender, EventArgs e)
		{
			if (palette == null) return;
			if (FileDialogUtility.SaveFileDialog(out string savePath, FileDialogUtility.CSV_PALETTE_FILTER))
			{
				palette.Save(savePath);
				MessageBox.Show("Palette saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}
	}
}

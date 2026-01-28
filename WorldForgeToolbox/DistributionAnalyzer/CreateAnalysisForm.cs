using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WorldForge;
using WorldForge.Utilities.BlockDistributionAnalysis;
using WorldForgeToolbox;

namespace RegionViewer.DistributionAnalyzer
{
	public partial class CreateAnalysisForm : Form
	{
		public enum ScanMode
		{
			FullWorld = 0,
			CustomArea = 1,
			Region = 2
		}



		public CreateAnalysisForm()
		{
			InitializeComponent();
			scanModeComboBox.Enabled = false;
			scanAreaGroupBox.Enabled = false;
			scanModeComboBox.SelectedValueChanged += OnScanModeChanged;
			scanTypesList.Items.Clear();
			scanTypesList.Items.Add(AnalysisConfigurations.OverworldOres);
			scanTypesList.Items.Add(AnalysisConfigurations.NetherOres);
			scanTypesList.Items.Add(AnalysisConfigurations.Liquids);
			scanTypesList.Items.Add(AnalysisConfigurations.Stones);
			scanTypesList.Items.Add(AnalysisConfigurations.TerrainBlocks);
			scanTypesList.ItemCheck += OnScanTypeListChanged;
			filePathTextBox.TextChanged += OnFilePathChanged;
		}

		private void OnScanModeChanged(object? sender, EventArgs e)
		{
			scanAreaGroupBox.Enabled = scanModeComboBox.SelectedItem?.Equals(ScanMode.CustomArea) ?? false;
		}

		private void OnScanTypeListChanged(object? sender, ItemCheckEventArgs e)
		{

		}

		private void browseButton_Click(object sender, EventArgs e)
		{
			if (OpenFileUtility.OpenRegionOrLevelDialog(out var file))
			{
				filePathTextBox.Text = file;
			}
		}

		private void OnFilePathChanged(object? sender, EventArgs e)
		{
			var filePath = filePathTextBox.Text;
			if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
			{
				if (Path.GetExtension(filePath).ToLower() == ".dat")
				{
					//Level file
					scanModeComboBox.Items.Clear();
					scanModeComboBox.Items.Add(ScanMode.FullWorld);
					scanModeComboBox.Items.Add(ScanMode.CustomArea);
					if (scanModeComboBox.SelectedItem?.Equals(ScanMode.Region) ?? true)
					{
						scanModeComboBox.SelectedItem = ScanMode.FullWorld;
					}
					scanModeComboBox.Enabled = true;
					var worldRoot = Path.GetDirectoryName(filePath);
					dimensionComboBox.Items.Clear();
					if (Directory.Exists(Path.Combine(worldRoot, "region")))
					{
						dimensionComboBox.Items.Add(DimensionID.Overworld);
					}
					if(Directory.Exists(Path.Combine(worldRoot, "DIM-1", "region")))
					{
						dimensionComboBox.Items.Add(DimensionID.Nether);
					}
					if(Directory.Exists(Path.Combine(worldRoot, "DIM1", "region")))
					{
						dimensionComboBox.Items.Add(DimensionID.TheEnd);
					}
					dimensionComboBox.SelectedIndex = 0;
				}
				else
				{
					//Region file
					scanModeComboBox.Items.Clear();
					scanModeComboBox.Items.Add(ScanMode.Region);
					scanModeComboBox.SelectedItem = ScanMode.Region;
					scanModeComboBox.Enabled = false;
					dimensionComboBox.Items.Clear();
					dimensionComboBox.SelectedIndex = -1;
				}
			}
		}
	}
}

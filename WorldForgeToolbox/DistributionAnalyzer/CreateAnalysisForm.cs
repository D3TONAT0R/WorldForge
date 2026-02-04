using WorldForge;
using WorldForge.Coordinates;
using WorldForge.Utilities.BlockDistributionAnalysis;

namespace WorldForgeToolbox
{
	public partial class CreateAnalysisForm : Form
	{
		public enum ScanMode
		{
			FullWorld = 0,
			CustomArea = 1,
			Region = 2
		}

		public string? FilePath => !string.IsNullOrWhiteSpace(filePathTextBox.Text) ? filePathTextBox.Text : null;

		public DimensionID Dimension => (DimensionID)dimensionComboBox.SelectedItem;

		public ScanMode WorldScanMode => (ScanMode)scanModeComboBox.SelectedItem;

		public List<AnalysisConfiguration> ScanConfigurations
		{
			get
			{
				List<AnalysisConfiguration> configurations = new List<AnalysisConfiguration>();
				foreach (var item in scanTypesList.CheckedItems)
				{
					configurations.Add((AnalysisConfiguration)item);
				}
				return configurations;
			}
		}

		public BlockCoord2D ScanOrigin => new BlockCoord2D((int)centerXNumeric.Value, (int)centerZNumeric.Value);

		public int ScanChunkRadius => (int)scanRadius.Value;

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
			if (OpenFileUtility.OpenFileDialog(out var file, OpenFileUtility.ALL_FILES_FILTER, OpenFileUtility.REGION_FILTER, OpenFileUtility.LEVEL_FILTER))
			{
				filePathTextBox.Text = file;
			}
		}

		private void OnFilePathChanged(object? sender, EventArgs e)
		{
			var filePath = filePathTextBox.Text;
			if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
			{
				createButton.Enabled = true;
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
					if (Directory.Exists(Path.Combine(worldRoot, "DIM-1", "region")))
					{
						dimensionComboBox.Items.Add(DimensionID.Nether);
					}
					if (Directory.Exists(Path.Combine(worldRoot, "DIM1", "region")))
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
			else
			{
				createButton.Enabled = false;
			}
		}

		private void cancelButton_Click(object sender, EventArgs e)
		{
			filePathTextBox.Text = null;
		}

		private void createButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}

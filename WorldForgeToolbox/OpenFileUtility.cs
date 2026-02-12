namespace WorldForgeToolbox;

public class OpenFileUtility
{
	public const string REGION_FILTER = "Region files (*.mca;*.mcr)|*.mca;*.mcr";
	public const string LEVEL_FILTER = "Level files (*.dat)|*.dat";
	public const string NBT_DATA_FILTER = "NBT files (*.dat)|*.dat";
	public const string ALL_FILES_FILTER = "All files (*.*)|*.*";

	public static bool OpenRegionDialog(string? initialDirectory, out string filePath)
	{
		using OpenFileDialog openFileDialog = new OpenFileDialog();
		if(initialDirectory != null) openFileDialog.InitialDirectory = initialDirectory;
		openFileDialog.Title = "Select a region file";
		openFileDialog.Filter = "Region files (*.mca;*.mcr)|*.mca;*.mcr|All files (*.*)|*.*";
		openFileDialog.Multiselect = false;
		DialogResult result = openFileDialog.ShowDialog();
		if (result == DialogResult.OK)
		{
			filePath = openFileDialog.FileName;
			// Handle the selected file path as needed
			return true;
		}
		filePath = null!;
		return false;
	}

	public static bool OpenRegionDialog(out string filePath) => OpenRegionDialog(null, out filePath);

	public static bool OpenWorldFolderDialog(string? initialDirectory, out string folderPath)
	{
		using FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
		if(initialDirectory != null) folderBrowserDialog.InitialDirectory = initialDirectory;
		folderBrowserDialog.Description = "Select a world folder";
		DialogResult result = folderBrowserDialog.ShowDialog();
		if (result == DialogResult.OK)
		{
			folderPath = folderBrowserDialog.SelectedPath;
			// Handle the selected folder path as needed
			return true;
		}
		folderPath = null!;
		return false;
	}

	public static bool OpenWorldFolderDialog(out string folderPath) => OpenWorldFolderDialog(null, out folderPath);

	public static bool OpenFileDialog(string? initialDirectory, out string filePath, params string[] fileFilters)
	{
		using OpenFileDialog openFileDialog = new OpenFileDialog();
		if (initialDirectory != null) openFileDialog.InitialDirectory = initialDirectory;
		openFileDialog.Title = "Select a file";
		if (fileFilters.Length == 0) openFileDialog.Filter = ALL_FILES_FILTER;
		else openFileDialog.Filter = string.Join("|", fileFilters);
		openFileDialog.Multiselect = false;
		DialogResult result = openFileDialog.ShowDialog();
		if (result == DialogResult.OK)
		{
			filePath = openFileDialog.FileName;
			// Handle the selected file path as needed
			return true;
		}
		filePath = null!;
		return false;
	}

	public static bool OpenFileDialog(out string filePath, params string[] fileFilters) => OpenFileDialog(null, out filePath, fileFilters);
}
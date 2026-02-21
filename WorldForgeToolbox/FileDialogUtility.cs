using Microsoft.Win32;

namespace WorldForgeToolbox;

public class FileDialogUtility
{
	public const string REGION_FILTER = "Region files (*.mca;*.mcr)|*.mca;*.mcr";
	public const string LEVEL_FILTER = "Level files (*.dat)|*.dat";
	public const string NBT_DATA_FILTER = "NBT files (*.dat)|*.dat";
	public const string CSV_PALETTE_FILTER = "Color palette files (*.csv)|*.csv";
	public const string JAR_FILES_FILTER = "JAR files (*.jar)|*.jar";
	public const string ALL_FILES_FILTER = "All files (*.*)|*.*";

	private const string REGISTRY_ROOT = @"HKEY_CURRENT_USER\SOFTWARE\WorldForgeToolbox\LastDirectoryPaths";

	public static bool OpenRegionDialogAt(string? initialDirectory, string? id, out string filePath)
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
			SetLastPath(id, Path.GetDirectoryName(filePath));
			return true;
		}
		filePath = null!;
		return false;
	}

	public static bool OpenRegionDialog(string? id, out string filePath) => OpenRegionDialogAt(GetLastPath(id), id, out filePath);

	public static bool OpenWorldFolderDialogAt(string? initialDirectory, string? id, out string folderPath)
	{
		using FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
		if(initialDirectory != null) folderBrowserDialog.InitialDirectory = initialDirectory;
		folderBrowserDialog.Description = "Select a world folder";
		DialogResult result = folderBrowserDialog.ShowDialog();
		if (result == DialogResult.OK)
		{
			folderPath = folderBrowserDialog.SelectedPath;
			SetLastPath(id, folderPath);
			return true;
		}
		folderPath = null!;
		return false;
	}

	public static bool OpenWorldFolderDialog(string? id, out string folderPath) => OpenWorldFolderDialogAt(GetLastPath(id), id, out folderPath);

	public static bool OpenFileDialogAt(string? initialDirectory, string? id, out string filePath, params string[] fileFilters)
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
			SetLastPath(id, Path.GetDirectoryName(filePath));
			return true;
		}
		filePath = null!;
		return false;
	}

	public static bool OpenFileDialog(string? id, out string filePath, params string[] fileFilters) => OpenFileDialogAt(GetLastPath(id), id, out filePath, fileFilters);

	public static bool SaveFileDialogAt(string? initialDirectory, string? id, out string filePath, params string[] fileFilters)
	{
		using SaveFileDialog saveFileDialog = new SaveFileDialog();
		saveFileDialog.InitialDirectory = initialDirectory;
		saveFileDialog.Title = "Save file";
		if (fileFilters.Length == 0) saveFileDialog.Filter = ALL_FILES_FILTER;
		else saveFileDialog.Filter = string.Join("|", fileFilters);
		DialogResult result = saveFileDialog.ShowDialog();
		if (result == DialogResult.OK)
		{
			filePath = saveFileDialog.FileName;
			SetLastPath(id, Path.GetDirectoryName(filePath));
			return true;
		}
		filePath = null!;
		return false;
	}

	public static bool SaveFileDialog(string? id, out string filePath, params string[] fileFilters) => SaveFileDialogAt(GetLastPath(id), id, out filePath, fileFilters);

	private static string? GetLastPath(string? id)
	{
		if(string.IsNullOrEmpty(id)) return null;
		return Registry.GetValue(REGISTRY_ROOT, id, null) as string;
	}

	private static void SetLastPath(string? id, string? path)
	{
		if(string.IsNullOrEmpty(id) || string.IsNullOrEmpty(path)) return;
		Registry.SetValue(REGISTRY_ROOT, id, path);
	}
}
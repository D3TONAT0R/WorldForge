namespace WorldForgeToolbox;

public class OpenFileUtility
{
	public static bool OpenRegionDialog(out string filePath, string? initialDirectory = null)
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
		filePath = null;
		return false;
	}

    public static bool OpenRegionOrLevelDialog(out string filePath, string? initialDirectory = null)
    {
        using OpenFileDialog openFileDialog = new OpenFileDialog();
        if (initialDirectory != null) openFileDialog.InitialDirectory = initialDirectory;
        openFileDialog.Title = "Select a region or level.dat file";
        openFileDialog.Filter = "Region files (*.mca;*.mcr)|*.mca;*.mcr|Level files (*.dat)|*.dat|All files (*.*)|*.*";
        openFileDialog.Multiselect = false;
        DialogResult result = openFileDialog.ShowDialog();
        if (result == DialogResult.OK)
        {
            filePath = openFileDialog.FileName;
            // Handle the selected file path as needed
            return true;
        }
        filePath = null;
        return false;
    }

    public static bool OpenWorldFolderDialog(out string? folderPath, string? initialDirectory = null)
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
		folderPath = null;
		return false;
	}
}
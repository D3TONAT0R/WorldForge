namespace WorldForgeToolbox;

public class ToolboxForm : Form
{
	protected string? inputFileArg;

	public ToolboxForm()
	{
		
	}

	public ToolboxForm(string? inputFile)
	{
		inputFileArg = inputFile;
	}

	public void ReturnToToolbox()
	{
		Toolbox.Instance.Show();
		this.Close();
	}
}
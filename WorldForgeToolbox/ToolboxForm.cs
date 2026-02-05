namespace WorldForgeToolbox;

public class ToolboxForm : Form
{
	protected string? inputFileArg;

	public static Pen hoverOutlinePen = new Pen(Color.Red, 2f);

	public ToolboxForm()
	{
		
	}

	public ToolboxForm(string? inputFile)
	{
		inputFileArg = inputFile;
	}

	public void ReturnToToolbox()
	{
		Toolbox.Instance.Return();
		Close();
	}
}
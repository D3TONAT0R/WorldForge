namespace WorldForgeToolbox;

public class ToolboxForm : Form
{
	protected string? inputFileArg;
	public static Pen hoverOutlinePen = new Pen(Color.Red, 2f);

	public ToolboxForm()
	{
		InitializeComponent();
	}

	public ToolboxForm(string? inputFile) : base()
	{
		inputFileArg = inputFile;
	}

	public void ReturnToToolbox()
	{
		Toolbox.Instance.Return();
		Close();
	}

	private void returnToToolboxButton_Click(object sender, EventArgs e)
	{
		ReturnToToolbox();
	}

	private void InitializeComponent()
	{
		SuspendLayout();
		// 
		// ToolboxForm
		// 
		ClientSize = new Size(284, 261);
		Name = "ToolboxForm";
		ResumeLayout(false);

	}
}
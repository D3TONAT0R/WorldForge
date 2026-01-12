namespace WorldForgeToolbox
{
	public partial class Toolbox : Form
	{
		public Toolbox()
		{
			InitializeComponent();
		}

		private void OnNBTViewerClick(object sender, EventArgs e)
		{
			Application.Run(new NBTViewer(null));
			Close();
		}

		private void OnRegionViewerClick(object sender, EventArgs e)
		{
			Application.Run(new RegionViewer(null));
			Close();
		}
	}
}
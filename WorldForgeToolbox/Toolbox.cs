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
			Show(new NBTViewer(null));
		}

		private void OnRegionViewerClick(object sender, EventArgs e)
		{
			Show(new RegionViewer(null));
		}

		private void OnWorldViewerClick(object sender, EventArgs e)
		{
			Show(new WorldViewer(null));
        }

        private void Show(Form form)
        {
            Hide();
            form.Closed += (s, args) => Close();
            if (!form.IsDisposed) form.Show();
        }
    }
}
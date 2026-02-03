using RegionViewer.DistributionAnalyzer;
using WorldForge.ConsoleTools;

namespace WorldForgeToolbox
{
    public partial class Toolbox : Form
    {
        private readonly string? fileNameArg;

		private bool returning = false;

        public Toolbox(string? fileNameArg)
        {
			Instance = this;
            this.fileNameArg = fileNameArg;
            InitializeComponent();
		}

        public static Toolbox Instance { get; private set; }

        protected override void OnShown(EventArgs _)
		{
			try
			{
				if (!string.IsNullOrEmpty(fileNameArg))
				{
					if (File.Exists(fileNameArg))
					{
						string ext = Path.GetExtension(fileNameArg).ToLowerInvariant();
						if (ext == ".mca" || ext == ".mcr")
						{
							Show(new RegionViewer(fileNameArg));
						}
						else if (ext == ".dat" || ext == ".nbt")
						{
							if (Path.GetFileName(fileNameArg).ToLowerInvariant() == "level.dat")
							{
								Show(new WorldViewer(fileNameArg));
							}
							else
							{
								Show(new NBTViewer(fileNameArg));
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				// Show exception as a message box
				MessageBox.Show($"{e.Message}\n\n{e.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
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

        private void OnBlockDistributionClick(object sender, EventArgs e)
        {
            Show(new DistributionViewer());
        }

        private void Show(Form form)
        {
			returning = false;
            Hide();
			form.Closed += (s, args) =>
			{
				if (returning) Show();
				else Close();
			};
            if (!form.IsDisposed) form.Show();
        }

        public void Return()
        {
			returning = true;
        }
    }
}
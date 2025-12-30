using WorldForge;

namespace NetheriteFinder
{
	public static class Program
	{
		[STAThread]
		public static void Main()
		{
			WorldForgeManager.Initialize();
			ApplicationConfiguration.Initialize();
			Application.Run(new MainForm());
		}
	}
}
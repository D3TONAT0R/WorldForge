using WorldForge;

namespace WorldForgeToolbox;

static class Program
{
	/// <summary>
	///  The main entry point for the application.
	/// </summary>
	[STAThread]
	static void Main()
	{
		// To customize application configuration such as set high DPI settings or default font,
		// see https://aka.ms/applicationconfiguration.
		ApplicationConfiguration.Initialize();
		WorldForgeManager.Initialize();
		Bitmaps.BitmapFactory = new WinformsBitmapFactory();
		//Get file name from command line args
		string[] args = Environment.GetCommandLineArgs();
		string fileName = args.Length > 1 ? args[1] : null;
		Application.Run(new Toolbox());
	}
}
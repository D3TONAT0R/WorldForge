using System.Diagnostics;
using WorldForge;

namespace WorldForgeToolbox;

static class Program
{
	public class WinformsLogger : ILogHandler
	{
        public void Verbose(string message)
        {
            Debug.WriteLine(message);
        }

        public void Info(string message)
        {
            Debug.WriteLine(message);
        }

        public void Warning(string message)
        {
            Debug.WriteLine(message);
        }

        public void Error(string message)
        {
            Debug.WriteLine(message);
        }

        public void Exception(string message, Exception e)
        {
            Debug.WriteLine(e.Message);
            Debug.WriteLine(e.StackTrace);
        }
    }

	/// <summary>
	///  The main entry point for the application.
	/// </summary>
	[STAThread]
	static void Main()
	{
		// To customize application configuration such as set high DPI settings or default font,
		// see https://aka.ms/applicationconfiguration.
		ApplicationConfiguration.Initialize();
        Logger.LogHandler = new WinformsLogger();
        WorldForgeManager.Initialize();
		Bitmaps.BitmapFactory = new WinformsBitmapFactory();
		//Get file name from command line args
		string[] args = Environment.GetCommandLineArgs();
		string fileName = args.Length > 1 ? args[1] : null;
		Application.Run(new Toolbox());
	}
}
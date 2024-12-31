using WorldForge;

namespace WorldForgeConsole
{
	internal class Program
	{
		static void Main(string[] args)
		{
			WorldForgeManager.Initialize();
			WorldForge.WorldForgeConsole.Start(args);
		}
	}
}

using WorldForge;

namespace WorldForgeConsole
{
	internal class Program
	{
		static void Main(string[] args)
		{
			WorldForgeManager.Initialize(new WFBitmapFactory());
			WorldForge.WorldForgeConsole.Start(args);
		}
	}
}

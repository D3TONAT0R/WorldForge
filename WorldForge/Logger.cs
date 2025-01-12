using System;

namespace WorldForge
{
	public interface ILogHandler
	{
		void Log(string message);

		void Warning(string message);

		void Error(string message);
	}

	public class ConsoleLogHandler : ILogHandler
	{
		public void Log(string message)
		{
			Console.WriteLine(message);
		}

		public void Warning(string message)
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine(message);
			Console.ResetColor();
		}

		public void Error(string message)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(message);
			Console.ResetColor();
		}
	}

	public static class Logger
	{
		public static ILogHandler LogHandler { get; set; };

		private static ConsoleLogHandler defaultHandler = new ConsoleLogHandler();

		public static void Log(string message) => (LogHandler ?? defaultHandler).Log(message);

		public static void Warning(string message) => (LogHandler ?? defaultHandler).Warning(message);

		public static void Error(string message) => (LogHandler ?? defaultHandler).Error(message);
	}
}
using System;

namespace WorldForge
{
	public interface ILogHandler
	{
		void Log(string message);

		void Warning(string message);

		void Error(string message);

		void Exception(string message, Exception e);
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

		public void Exception(string message, Exception e)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(message);
			Console.WriteLine(e);
			Console.ResetColor();
		}
	}

	public static class Logger
	{
		public static ILogHandler LogHandler { get; set; };

		private static ConsoleLogHandler defaultHandler = new ConsoleLogHandler();

		public static void Info(string message) => (LogHandler ?? defaultHandler).Log(message);

		public static void Warning(string message) => (LogHandler ?? defaultHandler).Warning(message);

		public static void Error(string message) => (LogHandler ?? defaultHandler).Error(message);

		public static void Exception(string message, Exception e) => (LogHandler ?? defaultHandler).Exception(message, e);
	}
}
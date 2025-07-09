using System;

namespace WorldForge
{
	public enum LogLevel
	{
		Verbose = 0,
		Info = 1,
		Warning = 2,
		Error = 3
	}

	public interface ILogHandler
	{
		void Verbose(string message);

		void Info(string message);

		void Warning(string message);

		void Error(string message);

		void Exception(string message, Exception e);
	}

	public class ConsoleLogHandler : ILogHandler
	{
		public LogLevel LogLevel { get; set; } = LogLevel.Info;

		public void Verbose(string message)
		{
			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.WriteLine(message);
			Console.ResetColor();
		}

		public void Info(string message)
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
		public static ILogHandler LogHandler { get; set; }
		public static ILogHandler ActiveLogHandler => LogHandler ?? defaultHandler;

		public static LogLevel Level { get; set; } = LogLevel.Info;

		private static ConsoleLogHandler defaultHandler = new ConsoleLogHandler();

		public static void Verbose(string message)
		{
			if(CheckLogLevel(LogLevel.Verbose)) ActiveLogHandler.Verbose(message);
		}

		public static void Info(string message)
		{
			if(CheckLogLevel(LogLevel.Info)) ActiveLogHandler.Info(message);
		}

		public static void Warning(string message)
		{
			if(CheckLogLevel(LogLevel.Warning)) ActiveLogHandler.Warning(message);
		}

		public static void Error(string message)
		{
			if(CheckLogLevel(LogLevel.Error)) ActiveLogHandler.Error(message);
		}

		public static void Exception(string message, Exception e)
		{
			ActiveLogHandler.Exception(message, e);
		}

		private static bool CheckLogLevel(LogLevel level)
		{
			return Level <= level;
		}
	}
}
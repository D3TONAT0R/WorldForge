using System;
using System.Collections.Generic;
using System.IO;

namespace MCUtils {
	class Program {

		static string progressString;

		static void Main(string[] args) {
			Console.ResetColor();
			Console.Clear();
			WriteLine("--------------");
			WriteLine("MinecraftUtils");
			WriteLine("--------------");
			string fname = null;
			if(args.Length > 0) {
				if(File.Exists(args[0])) {
					var v = new NBTViewer(args[0]);
					v.Run();
				}
			} else {
				Console.WriteLine("File '" + fname + "' does not exist!");
			}
			string input = "";
			while(input != "exit") {
				WriteLine("Chose an operation to perform:");
				WriteLine("- mergeregions       Merges region files based on an input map.");
				input = GetInput();
				if(input.StartsWith("mergeregions")) {
					var m = new RegionMerger();
					m.Run();
				}
				if(input.StartsWith("view ")) {
					var v = new NBTViewer(input.Substring(5).Replace("\"", ""));
					v.Run();
				}
			}
		}
		
		public static string GetInput() {
			Console.CursorVisible = true;
			string s;
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Write("> ");
			s = Console.ReadLine();
			Console.ResetColor();
			return s;
		}

		static void WriteConsoleLine(string str) {
			Console.CursorVisible = false;
			if(progressString != null) {
				WriteProgress("", -1);
				progressString = null;
				Console.SetCursorPosition(0, Console.CursorTop);
			}
			Console.WriteLine(str);
			Console.ResetColor();
		}

		public static void WriteLine(string str) {
			WriteConsoleLine(str);
		}

		public static void WriteSuccess(string str) {
			Console.ForegroundColor = ConsoleColor.Green;
			WriteConsoleLine(str);
		}

		public static void WriteLineSpecial(string str) {
			Console.ForegroundColor = ConsoleColor.Cyan;
			WriteConsoleLine(str);
		}

		public static void WriteLine(string str, params Object[] args) {
			Console.WriteLine(str, args);
		}

		public static void WriteWarning(string str) {
			Console.ForegroundColor = ConsoleColor.DarkYellow;
			WriteConsoleLine(str);
		}

		public static void WriteError(string str) {
			Console.BackgroundColor = ConsoleColor.DarkRed;
			WriteConsoleLine(str);
		}

		public static void WriteProgress(string str, float progressbar) {
			Console.CursorVisible = false;
			int lastLength = 0;
			if(progressString != null) {
				lastLength = progressString.Length;
				Console.SetCursorPosition(0, Console.CursorTop);
			}
			progressString = str;
			if(progressbar >= 0) progressString += " "+GetProgressBar(progressbar)+" "+(int)Math.Round(progressbar*100)+"%";
			if(lastLength > 0) progressString = progressString.PadRight(lastLength, ' ');
			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.Write(progressString);
			Console.ResetColor();
		}

		static string GetProgressBar(float prog) {
			string s = "";
			for(int i = 0; i < 20; i++) {
				s += (prog >= (i+1)/20f) ? "█" : "░";
			}
			s += "";
			return s;
		}
	}
}

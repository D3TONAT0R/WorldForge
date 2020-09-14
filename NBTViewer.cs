using Ionic.Zlib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static MCUtils.NBTContent;

namespace MCUtils {
	class NBTViewer : IUtilTask {

		const char sep = ';';

		string filename;
		NBTContent content;

		string cursorParent = "";
		int cursorChildPos = 0;
		List<int> indexTree = new List<int>();

		Container currentContainer;
		string[] currentContentKeys;

		int cursorBufferLoc = 0;
		string buffer;
		int overflow = -1;

		public NBTViewer(string path) {
			content = new NBTContent(GZipStream.UncompressBuffer(File.ReadAllBytes(path)), false);
			filename = Path.GetFileName(path);
			filename = "root";
			var root = new CompoundContainer();
			foreach(var k in content.contents.GetContentKeys("")) {
				root.Add(k, content.contents.Get(k));
			}
			content.contents.cont.Clear();
			content.contents.Add(filename, root);
		}

		public void Run() {
			currentContainer = content.contents;
			UpdateContentKeys();
			while(true) {
				overflow = -1;
				buffer = "";
				Draw("", "", content.contents, 0, false);
				Console.Write(buffer);
				while(cursorBufferLoc - Console.WindowTop < 1) {
					Console.WindowTop--;
				}
				while(cursorBufferLoc - Console.WindowTop + Console.WindowHeight < 3) {
					Console.WindowTop++;
				}
				var key = Console.ReadKey().Key;
				if(key == ConsoleKey.Escape) break;
				else if(key == ConsoleKey.DownArrow) {
					cursorChildPos++;
				} else if(key == ConsoleKey.UpArrow) {
					cursorChildPos--;
				} else if(key == ConsoleKey.RightArrow) {
					EnterContainer(currentContentKeys[cursorChildPos]);
				} else if(key == ConsoleKey.LeftArrow) {
					ExitContainer();
				}
				cursorChildPos = Math.Max(0, Math.Min(currentContentKeys.Length - 1, cursorChildPos));
				Console.Clear();
			}
		}

		void Draw(string tree, string name, object obj, int indent, bool drawAll) {
			if(overflow > 15) return;
			if(!string.IsNullOrEmpty(tree)) tree += sep;
			tree += name;
			string s = "";
			for(int i = 0; i < indent; i++) s += "| ";
			bool enterContainer = cursorParent.StartsWith(tree/* + (string.IsNullOrEmpty(tree) ? "" : sep.ToString())*/);
			if(IsContainer(obj)) s += enterContainer ? "▼ " : "► ";
			s += name + "[" + GetTag(obj) + "] = " + obj;
			if(cursorChildPos < currentContentKeys.Length && currentContentKeys[cursorChildPos] == name) {
				overflow = 0;
				Console.Write(buffer);
				buffer = "";
				cursorBufferLoc = Console.CursorTop;
				Program.WriteSuccess(s);
			} else {
				if(overflow >= 0) overflow++;
				if(overflow > 15) {
					buffer += "[...]";
					return;
				}
				buffer += s + "\n";
			}
			if(obj is CompoundContainer) {
				var content = ((CompoundContainer)obj).cont;
				if(enterContainer) {
					foreach(var k in content.Keys) {
						Draw(tree, k, content[k], indent + 1, drawAll);
					}
				}
			} else if(obj is ListContainer) {
				var content = ((ListContainer)obj).cont;
				if(enterContainer) {
					for(int i = 0; i < content.Count; i++) {
						Draw(tree, i.ToString(), content[i], indent + 1, drawAll);
					}
				}
			}
		}

		bool IsContainer(object obj) {
			return obj is Container;
		}

		NBTTag GetTag(object obj) {
			return NBTTagDictionary[obj.GetType()];
		}

		void EnterContainer(string enter) {
			string lastParent = cursorParent;
			cursorParent += sep + enter;
			int lastIndex = cursorChildPos;
			bool b = UpdateContentKeys();
			if(currentContentKeys.Length == 0 || !b) {
				cursorParent = lastParent;
				UpdateContentKeys();
				cursorChildPos = lastIndex;
			} else {
				indexTree.Add(lastIndex);
			}
		}

		void ExitContainer() {
			if(!cursorParent.Contains(sep)) return;
			var split = cursorParent.Split(sep);
			cursorParent = cursorParent.Substring(0, cursorParent.Length - split[split.Length - 1].Length - 1);
			UpdateContentKeys();
			cursorChildPos = indexTree[indexTree.Count - 1];
			indexTree.RemoveAt(indexTree.Count - 1);
		}

		bool UpdateContentKeys() {
			try {
				if(cursorParent.StartsWith(sep)) cursorParent = cursorParent.Substring(1);
				cursorChildPos = 0;
				if(string.IsNullOrEmpty(cursorParent)) {
					currentContainer = content.contents;
				} else {
					string[] split = cursorParent.Split(sep);
					currentContainer = content.contents;
					foreach(var s in split) {
						currentContainer = (Container)currentContainer.Get(s);
					}
				}
				currentContentKeys = currentContainer.GetContentKeys("");
				return true;
			}
			catch {
				return false;
			}
		}
	}
}

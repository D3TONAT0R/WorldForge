using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WorldForge.IO;
using WorldForge.NBT;
using WorldForge.Regions;

namespace WorldForge.ConsoleTools
{
	class NBTViewer : IConsoleTool
	{

		public class ChangeState
		{

			public enum ChangeType
			{
				None, Modification, Addition, Removal
			}

			public object oldValue = null;
			public ChangeType changeType = ChangeType.None;

			public ConsoleColor GetColor(bool bg)
			{
				switch(changeType)
				{
					case ChangeType.Modification: return bg ? ConsoleColor.DarkCyan : ConsoleColor.Cyan;
					case ChangeType.Addition: return bg ? ConsoleColor.DarkGreen : ConsoleColor.Green;
					case ChangeType.Removal: return bg ? ConsoleColor.DarkRed : ConsoleColor.Red;
					default: return bg ? ConsoleColor.DarkBlue : ConsoleColor.Gray;
				}
			}
		}

		public static readonly ChangeState defaultState = new ChangeState();
		const char sep = ';';

		string filename;
		NBTFile content;
		Dictionary<string, ChangeState> changes = new Dictionary<string, ChangeState>();

		string cursorParent = "";
		int cursorChildPos = 0;
		List<int> indexTree = new List<int>();

		INBTContainer currentContainer;
		string[] currentContentKeys;

		int cursorBufferLoc = 0;
		//string buffer;
		int overflow = -1;
		int moreItems = 0;

		public NBTViewer(string path)
		{
			filename = Path.GetFileName(path);
			if(path.EndsWith(".mca"))
			{
				content = new NBTFile();
				Region r = RegionDeserializer.LoadRegion(new RegionFilePaths(path, null, null), null);
				for(int z = 0; z < 32; z++)
				{
					for(int x = 0; x < 32; x++)
					{
						if(r.chunks[x, z] != null)
						{
							content.contents.Add($"Chunk [{x},{z}]", r.chunks[x, z].SourceData.main.contents);
						}
					}
				}
			}
			else
			{
				content = new NBTFile(path);
			}
			//filename = "root";
			//var root = new CompoundContainer();
			//foreach(var k in content.contents.GetContentKeys("")) {
			//	root.Add(k, content.contents.Get(k));
			//}
			//content.contents.cont.Clear();
			//content.contents.Add(filename, root);
		}

		public NBTViewer(NBTFile data)
		{
			content = data;
		}

		public void Run(string[] args)
		{
			currentContainer = content.contents;
			UpdateContentKeys();
			while(true)
			{
				overflow = -1;
				//buffer = "";
				moreItems = 0;
				Draw("", "", content.contents, 0, false, false);
				if(moreItems > 0)
				{
					WorldForgeConsole.WriteLine("[" + moreItems + " more]");
				}
				//Console.Write(buffer);
				if(cursorBufferLoc > 1)
				{
					while(cursorBufferLoc - Console.WindowTop < 1)
					{
						Console.WindowTop--;
					}
					while(cursorBufferLoc - Console.WindowTop + Console.WindowHeight < 3)
					{
						Console.WindowTop++;
					}
				}
				var key = Console.ReadKey().Key;
				if(key == ConsoleKey.Escape) break;
				else if(key == ConsoleKey.DownArrow)
				{
					cursorChildPos++;
				}
				else if(key == ConsoleKey.UpArrow)
				{
					cursorChildPos--;
				}
				else if(key == ConsoleKey.RightArrow)
				{
					EnterContainer(currentContentKeys[cursorChildPos].Split(sep).Last());
				}
				else if(key == ConsoleKey.LeftArrow)
				{
					ExitContainer();
				}
				cursorChildPos = Math.Max(0, Math.Min(currentContentKeys.Length - 1, cursorChildPos));
				Console.Clear();
			}
		}

		void Draw(string tree, string name, object obj, int indent, bool drawAll, bool isLast)
		{
			if(overflow > 20)
			{
				moreItems++;
				return;
			}
			if(!string.IsNullOrEmpty(tree)) tree += sep;
			tree += name;
			string s = "";
			for(int i = 0; i < indent; i++)
			{
				if(i == indent - 1)
				{
					if(isLast)
					{
						s += "└ ";
					}
					else
					{
						s += "│ ";
					}
				}
				else
				{
					s += "│ ";
				}
			}
			var pc = SplitParentAndChild(tree);
			bool enterContainer = ContainsPath(tree, cursorParent);
			if(IsContainer(obj)) s += enterContainer ? "▼ " : "► ";
			s += (string.IsNullOrEmpty(name) ? filename : name + "[" + GetTag(obj) + "] = " + obj);
			ChangeState state;
			if(changes.ContainsKey(tree))
			{
				state = changes[tree];
			}
			else
			{
				state = defaultState;
			}
			if(cursorChildPos < currentContentKeys.Length && currentContentKeys[cursorChildPos] == tree)
			{
				overflow = 0;
				//Console.Write(buffer);
				//buffer = "";
				cursorBufferLoc = Console.CursorTop;
				WorldForgeConsole.WriteLine(s, ConsoleColor.White, state.GetColor(true));
			}
			else
			{
				if(overflow >= 0) overflow++;
				if(overflow > 20)
				{
					moreItems++;
					return;
				}
				WorldForgeConsole.WriteLine(s, state.GetColor(false));
				//buffer += s + "\n";
			}
			if(obj is NBTCompound)
			{
				var content = ((NBTCompound)obj).contents;
				if(enterContainer)
				{
					var keys = content.Keys.ToArray();
					for(int i = 0; i < keys.Length; i++)
					{
						Draw(tree, keys[i], content[keys[i]], indent + 1, drawAll, i == keys.Length - 1);
					}
				}
			}
			else if(obj is NBTList)
			{
				var content = ((NBTList)obj).listContent;
				if(enterContainer)
				{
					for(int i = 0; i < content.Count; i++)
					{
						Draw(tree, i.ToString(), content[i], indent + 1, drawAll, i == content.Count - 1);
					}
				}
			}
		}

		bool IsContainer(object obj)
		{
			return obj is INBTContainer;
		}

		NBTTag GetTag(object obj)
		{
			return NBTMappings.GetTag(obj.GetType());
		}

		void EnterContainer(string enter)
		{
			string lastParent = cursorParent;
			cursorParent += sep + enter;
			int lastIndex = cursorChildPos;
			bool b = UpdateContentKeys();
			if(currentContentKeys.Length == 0 || !b)
			{
				cursorParent = lastParent;
				UpdateContentKeys();
				cursorChildPos = lastIndex;
			}
			else
			{
				indexTree.Add(lastIndex);
			}
		}

		void ExitContainer()
		{
			if(cursorParent.Length == 0) return;
			//if(!cursorParent.Contains(sep)) return;
			var split = cursorParent.Split(sep);
			if(split.Length <= 1)
			{
				cursorParent = "";
			}
			else
			{
				cursorParent = cursorParent.Substring(0, cursorParent.Length - split[split.Length - 1].Length - 1);
			}
			UpdateContentKeys();
			cursorChildPos = indexTree[indexTree.Count - 1];
			indexTree.RemoveAt(indexTree.Count - 1);
		}

		bool UpdateContentKeys()
		{
			try
			{
				if(cursorParent.StartsWith(sep.ToString())) cursorParent = cursorParent.Substring(1);
				cursorChildPos = 0;
				if(string.IsNullOrEmpty(cursorParent))
				{
					currentContainer = content.contents;
				}
				else
				{
					string[] split = cursorParent.Split(sep);
					currentContainer = content.contents;
					foreach(var s in split)
					{
						if(currentContainer is NBTCompound comp)
						{
							currentContainer = (INBTContainer)comp.Get(s);
						}
						else
						{
							currentContainer = (INBTContainer)((NBTList)currentContainer).Get(int.Parse(s));
						}
					}
				}
				var prefix = cursorParent;
				if(prefix.Length > 0) prefix += sep;
				currentContentKeys = currentContainer.GetContentKeys(prefix);
				return true;
			}
			catch
			{
				return false;
			}
		}

		string[] SplitParentAndChild(string tree)
		{
			string[] split = tree.Split(sep);
			string[] ret = new string[2];
			ret[0] = "";
			for(int i = 0; i < split.Length - 1; i++)
			{
				ret[0] += split[i];
			}
			ret[1] = split[split.Length - 1];
			return ret;
		}

		bool ContainsPath(string tree, string path)
		{
			if(string.IsNullOrEmpty(tree)) return true;
			var ts = tree.Split(sep);
			var ps = path.Split(sep);
			if(ps.Length < ts.Length) return false;
			for(int i = 0; i < ts.Length; i++)
			{
				if(ts[i] != ps[i]) return false;
			}
			return true;
		}
	}
}

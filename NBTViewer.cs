using Ionic.Zlib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static MCUtils.NBTContent;

namespace MCUtils {
	class NBTViewer : IUtilTask {

		string filename;
		NBTContent content;

		public NBTViewer(string path) {
			content = new NBTContent(GZipStream.UncompressBuffer(File.ReadAllBytes(path)), false);
			filename = Path.GetFileName(path);
		}

		public void Run() {
			Draw(filename + " [Data version " + content.dataVersion + "]", content.contents, 0);
		}

		void Draw(string name, object obj, int indent) {
			string s = "";
			for(int i = 0; i < indent; i++) s += "| ";
			if(IsContainer(obj)) s += "+ ";
			s += name + "[" + GetTag(obj) + "] = " + obj;
			Program.WriteLine(s);
			if(obj is CompoundContainer) {
				var content = ((CompoundContainer)obj).cont;
				foreach(var k in content.Keys) {
					Draw(k, content[k], indent + 1);
				}
			} else if(obj is ListContainer) {
				var content = ((ListContainer)obj).cont;
				for(int i = 0; i < content.Count; i++) {
					Draw("["+i+"]", content[i], indent + 1);
				}
			}
		}

		bool IsContainer(object obj) {
			return obj is Container;
		}

		NBTTag GetTag(object obj) {
			return NBTTagDictionary[obj.GetType()];
		}
	}
}

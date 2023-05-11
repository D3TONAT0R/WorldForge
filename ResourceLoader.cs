using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace MCUtils
{
	internal static class ResourceLoader
	{
		public static string GetPathOfResource(string resourceFileName)
		{
			return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Resources", resourceFileName);
		}

		public static string LoadAsText(string resourceFileName)
		{
			return File.ReadAllText(GetPathOfResource(resourceFileName));
		}

		public static string LoadBlockListAsText()
		{
			return LoadAsText("blocks.csv");
		}
	}
}

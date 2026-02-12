using System.Collections.Generic;

namespace WorldForge
{
	public class ServerProperties
	{
		public List<string> header = new List<string>();

		private readonly Dictionary<string, string> data = new Dictionary<string, string>();

		public string Difficulty
		{
			get => Get("difficulty");
			set => Set("difficulty", value);
		}

		public string LevelName
		{
			get => Get("level-name");
			set => Set("level-name", value);
		}

		public string LevelSeed
		{
			get => Get("level-seed");
			set => Set("level-seed", value);
		}

		public static ServerProperties Load(string path)
		{
			var properties = new ServerProperties();
			var lines = System.IO.File.ReadAllLines(path);
			bool inHeader = true;
			for (int i = 0; i < lines.Length; i++)
			{
				var line = lines[i].Trim();
				if(line.StartsWith("#"))
				{
					if(inHeader)
					{
						properties.header.Add(line);
					}
				}
				else if (line.Contains("="))
				{
					inHeader = false;
					var parts = line.Split(new char[] { '=' }, 2);
					var key = parts[0].Trim();
					var value = parts[1].Trim();
					properties.data[key] = value;
				}
			}
			return properties;
		}

		public string Get(string key)
		{
			if(data.TryGetValue(key, out var value))
			{
				return value;
			}
			return null;
		}

		public void Set(string key, string value)
		{
			data[key] = value;
		}
	}
}
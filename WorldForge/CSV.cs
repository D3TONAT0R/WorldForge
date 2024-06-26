using System.Collections.Generic;

namespace WorldForge
{
	public class CSV
	{
		public string[] headers;
		public List<string[]> data;

		public CSV(string[] lines)
		{
			char delimeter = ',';
			int lineIndex = 0;
			if(lines[0].StartsWith("sep="))
			{
				delimeter = lines[0].Split('=')[1].Trim()[0];
				lineIndex++;
			}
			headers = lines[lineIndex].Split(delimeter);
			lineIndex++;

			int columnCount = headers.Length;
			data = new List<string[]>();
			for(int j = lineIndex; j < lines.Length; j++)
			{
				if(lines[j].Length > 0)
				{
					var line = lines[j];
					string[] parts = line.Split(delimeter);
					if(string.IsNullOrEmpty(parts[0]))
					{
						continue;
					}
					var columns = new string[columnCount];
					for(int i = 0; i < columnCount; i++)
					{
						if(i < parts.Length)
						{
							columns[i] = parts[i];
						}
						else
						{
							columns[i] = "";
						}
					}
					data.Add(columns);
				}
			}
		}
	}
}

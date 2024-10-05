using System.Collections.Generic;

namespace WorldForge
{
	public class CSVLine
	{
		public string[] columns;

		public int ColumnCount => columns.Length;

		public CSVLine(string line, char delimiter)
		{
			columns = line.Split(delimiter);
		}

		public string this[int index] => columns[index];

		public bool TryGetString(int index, out string value)
		{
			if(index >= columns.Length)
			{
				value = null;
				return false;
			}
			value = columns[index];
			if(string.IsNullOrEmpty(value))
			{
				value = null;
				return false;
			}
			return true;
		}

		public bool TryGetByte(int index, out byte? value)
		{
			if(index >= columns.Length)
			{
				value = null;
				return false;
			}
			if(string.IsNullOrEmpty(columns[index]))
			{
				value = null;
				return false;
			}
			value = byte.Parse(columns[index]);
			return true;
		}

		public bool TryGetShort(int index, out short? value)
		{
			if(index >= columns.Length)
			{
				value = null;
				return false;
			}
			if(string.IsNullOrEmpty(columns[index]))
			{
				value = null;
				return false;
			}
			value = short.Parse(columns[index]);
			return true;
		}

		public bool TryGetInt(int index, out int? value)
		{
			if(index >= columns.Length)
			{
				value = null;
				return false;
			}
			if(string.IsNullOrEmpty(columns[index]))
			{
				value = null;
				return false;
			}
			value = int.Parse(columns[index]);
			return true;
		}

		public bool IsAllEmpty()
		{
			foreach(var c in columns)
			{
				if(!string.IsNullOrEmpty(c)) return false;
			}
			return true;
		}

		public override string ToString()
		{
			return string.Join(",", columns);
		}
	}

	public class CSV
	{
		public CSVLine header;
		public List<CSVLine> data;

		public CSV(string csv) : this(csv.Replace("\r", "").Split('\n'))
		{
			
		}

		public CSV(string[] lines)
		{
			char delimiter = ',';
			int lineIndex = 0;
			if(lines[0].StartsWith("sep="))
			{
				delimiter = lines[0].Split('=')[1].Trim()[0];
				lineIndex++;
			}
			header = new CSVLine(lines[lineIndex], delimiter);
			lineIndex++;

			data = new List<CSVLine>();
			for(int j = lineIndex; j < lines.Length; j++)
			{
				if(lines[j].Length > 0)
				{
					var line = lines[j];
					if(string.IsNullOrWhiteSpace(line))
					{
						continue;
					}
					var csvLine = new CSVLine(line, delimiter);
					if(!csvLine.IsAllEmpty())
					{
						data.Add(new CSVLine(line, delimiter));
					}
				}
			}
		}
	}
}

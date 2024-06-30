using System;
using System.Text;
using WorldForge.Coordinates;

namespace WorldForge.IO
{
	public class ChunkSerializerAlpha : ChunkSerializerMCR
	{
		public ChunkSerializerAlpha(GameVersion version) : base(version) { }

		public static void GetAlphaChunkPathAndName(ChunkCoord pos, out string folder1, out string folder2, out string fileName)
		{
			folder1 = ToBase36(pos.x.Mod(256) % 64);
			folder2 = ToBase36(pos.z.Mod(256) % 64);
			bool xNeg = pos.x < 0;
			bool zNeg = pos.z < 0;
			var chunkNameBuilder = new StringBuilder();
			chunkNameBuilder.Append("c.");
			if(xNeg) chunkNameBuilder.Append("-");
			chunkNameBuilder.Append(ToBase36(Math.Abs(pos.x)));
			chunkNameBuilder.Append(".");
			if(zNeg) chunkNameBuilder.Append("-");
			chunkNameBuilder.Append(ToBase36(Math.Abs(pos.z)));
			chunkNameBuilder.Append(".dat");
			fileName = chunkNameBuilder.ToString();
		}

		private const string BASE_36_CHARS = "0123456789abcdefghijklmnopqrstuvwxyz";

		public static string ToBase36(int value)
		{
			if(value == 0) return "0";

			string result = "";

			if(value < 0)
			{
				result = "-";
				value = Math.Abs(value);
			}

			while(value > 0)
			{
				result = BASE_36_CHARS[value % 36] + result; // TODO: use StringBuilder for better performance
				value /= 36;
			}

			return result;
		}

		public static int FromBase36(string value)
		{
			if(string.IsNullOrEmpty(value)) return 0;


			bool negative = false;
			if(value[0] == '-')
			{
				negative = true;
				value = value.Substring(1);
			}

			int result = 0;
			int power = 1;

			while(value.Length > 0)
			{
				char c = value[value.Length - 1];
				int v = BASE_36_CHARS.IndexOf(c);
				result += (int)Math.Pow(v, power);
			}
			return negative ? -result : result;
		}
	}
}

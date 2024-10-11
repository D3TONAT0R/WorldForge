using System.IO;
using System.Text.RegularExpressions;

namespace WorldForge.Coordinates
{
	public struct RegionLocation
	{
		public int x;
		public int z;

		public RegionLocation(int regionX, int regionZ)
		{
			x = regionX;
			z = regionZ;
		}

		public static bool TryGetFromFileName(string filename, out RegionLocation location)
		{
			string fname = Path.GetFileName(filename);
			location = new RegionLocation();
			if(Regex.IsMatch(fname, @".*\.-?[0-9]+\.-?[0-9]+\.mc.*"))
			{
				var split = fname.Split('.');
				location.x = int.Parse(split[split.Length - 3]);
				location.z = int.Parse(split[split.Length - 2]);
				return true;
			}
			return false;
		}

		public ChunkCoord GetChunkCoord(int chunkOffsetX = 0, int chunkOffsetZ = 0)
		{
			return new ChunkCoord(x * 32 + chunkOffsetX, z * 32 + chunkOffsetZ);
		}

		public BlockCoord GetBlockCoord(int blockOffsetX, int y, int blockOffsetZ)
		{
			return new BlockCoord(x * 512 + blockOffsetX, y, z * 512 + blockOffsetZ);
		}

		public string ToFileName() => $"r.{x}.{z}.mca";

		public string ToFileNameMCR() => $"r.{x}.{z}.mcr";

		public override string ToString()
		{
			return $"({x},{z})";
		}
	}
}

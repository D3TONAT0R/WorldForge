using WorldForge;
using WorldForge.Coordinates;

namespace WorldForgeToolbox
{
	public class Report
	{
		public List<Vein> veins;
		public SearchProfile profile;

		public static Report Create(string regionDirectory, int posX, int posZ, int searchRadius, SearchProfile profile)
		{
			var tempPath = CopyRegionsToTempFolder(regionDirectory);

			var dim = Dimension.FromRegionFolder(null, tempPath, DimensionID.Unknown, GameVersion.Release_1(21, 3));
			
			List<BlockCoord> positions = new List<BlockCoord>();
			for(int x = posX - searchRadius; x <= posX + searchRadius; x++)
			{
				for(int z = posZ - searchRadius; z <= posZ + searchRadius; z++)
				{
					for(int y = profile.searchYMin; y < profile.searchYMax; y++)
					{
						var bc = new BlockCoord(x, y, z);
						var block = dim.GetBlock(bc);
						foreach (var b in profile.blocks)
						{
							if(block == b) 
							{
								positions.Add(bc);
								break;
							}
						}
					}
				}
			}
			var report = new Report();
			report.profile = profile;
			report.veins = ToVeins(positions, 5);

			return report;
		}

		public void SortVeinsByDistance(int posX, int posZ)
		{
			var pos = new BlockCoord2D(posX, posZ);
			veins = veins.OrderBy(v => BlockCoord2D.Distance(pos, new BlockCoord2D(v.pos.x, v.pos.z))).ToList();
		}

		private static string CopyRegionsToTempFolder(string regionDirectory)
		{
			var tempPath = Path.Combine(Path.GetTempPath(), "NetheriteFinder");
			if(Directory.Exists(tempPath))
			{
				Directory.Delete(tempPath, true);
			}
			Directory.CreateDirectory(tempPath);
			foreach(var file in Directory.GetFiles(regionDirectory))
			{
				//TODO: only copy required region files
				File.Copy(file, Path.Combine(tempPath, Path.GetFileName(file)));
			}
			return tempPath;
		}

		static List<Vein> ToVeins(List<BlockCoord> positions, int max)
		{
			List<Vein> veins = new List<Vein>();
			for(int i = 0; i < positions.Count; i++)
			{
				var pos = positions[i];
				float d;
				int c = ClosestVein(pos, veins, out d);
				if(c == -1 || d > max)
				{
					veins.Add(new Vein(pos, 1));
				}
				else
				{
					veins[c].count++;
				}
			}
			return veins;
		}

		static int ClosestVein(BlockCoord pos, List<Vein> veins, out float d)
		{
			d = float.MaxValue;
			int c = -1;
			for(int i = 0; i < veins.Count; i++)
			{
				float dist = BlockCoord.Distance(pos, veins[i].pos);
				if(dist < d)
				{
					d = dist;
					c = i;
				}
			}
			return c;
		}

		public static void GetBearing(BlockCoord from, BlockCoord to, out int angle, out int distance)
		{
			var dx = to.x - from.x;
			var dz = to.z - from.z;
			angle = (int)(Math.Atan2(-dz, -dx) * 57.2958f) - 90;
			while(angle < 0) angle += 360;
			distance = (int)Math.Sqrt(dx * dx + dz * dz);
		}
	}
}
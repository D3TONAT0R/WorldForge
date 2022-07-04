using Ionic.Zlib;
using MCUtils.Coordinates;
using MCUtils.IO;
using MCUtils.NBT;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MCUtils
{
	public class World
	{

		public static readonly ProtoBlock defaultBlock = BlockList.Find("minecraft:stone");

		public Version gameVersion;
		public LevelData levelData;
		public Dictionary<RegionLocation, Region> regions;

		public string WorldName
		{
			get => levelData.worldName;
			set => levelData.worldName = value;
		}

		public World(Version version, int regionLowerX, int regionLowerZ, int regionUpperX, int regionUpperZ, string levelDatPath = null)
		{
			gameVersion = version;
			if (!string.IsNullOrEmpty(levelDatPath))
			{
				using (var stream = File.OpenRead(levelDatPath))
				{
					levelData = LevelData.Load(new NBTFile(stream));
				}
			}
			else
			{
				levelData = LevelData.CreateNew();
			}
			regions = new Dictionary<RegionLocation, Region>();
			for (int x = regionLowerX; x <= regionUpperX; x++)
			{
				for (int z = regionLowerZ; z <= regionUpperZ; z++)
				{
					var reg = new Region(x, z)
					{
						containingWorld = this
					};
					regions.Add(new RegionLocation(x, z), reg);
				}
			}
		}

		private World()
		{

		}

		//TODO: Danger, loads entire world at once
		public static World Load(string worldSaveDir, Version? gameVersion = null, bool throwOnRegionLoadFail = false)
		{
			var world = new World();
			var nbt = new NBTFile(Path.Combine(worldSaveDir, "level.dat"));
			world.levelData = LevelData.Load(nbt);
			world.gameVersion = Version.FromDataVersion(world.levelData.dataVersion) ?? Version.FirstVersion;
			world.regions = new Dictionary<RegionLocation, Region>();
			bool isAlphaFormat = gameVersion < Version.Beta_1(3);
			//TODO: how to load alpha chunks?
			foreach (var f in Directory.GetFiles(Path.Combine(worldSaveDir, "region"), "*.mc*"))
			{
				var filename = Path.GetFileName(f);
				if (Regex.IsMatch(filename, @"^r.-*\d.-*\d.mc(a|r)")) {
					try
					{
						Region region = null;
						if (!isAlphaFormat)
						{
							region = RegionLoader.LoadRegion(f, gameVersion);
						}
						else
						{
							//region = RegionLoader.LoadRegionAlphaChunks(worldSaveDir, RegionLocation();
						}
						world.regions.Add(region.regionPos, region);
					}
					catch(Exception e) when (!throwOnRegionLoadFail)
					{
						Console.WriteLine($"Failed to load region '{filename}': {e.Message}");
					}
				}
				else
				{
					Console.WriteLine($"Invalid file '{filename}' in region folder.");
				}
			}
			return world;
		}

		public static void GetWorldInfo(string worldSaveDir, out string worldName, out Version gameVersion, out RegionLocation[] regions)
		{
			NBTFile levelDat = new NBTFile(Path.Combine(worldSaveDir, "level.dat"));
			var dataComp = levelDat.contents.GetAsCompound("Data");
			
			gameVersion = Version.FromDataVersion(dataComp.Get<int>("DataVersion")) ?? Version.FirstVersion;
			worldName = dataComp.Get<string>("LevelName");
			var regionList = new List<RegionLocation>();
			foreach (var f in Directory.GetFiles(Path.Combine(worldSaveDir, "region"), "*.mc*"))
			{
				try
				{
					regionList.Add(RegionLocation.FromRegionFileName(f));
				}
				catch
				{

				}
			}
			regions = regionList.ToArray();
		}

		/// <summary>Instantiates empty regions in the specified area, allowing for blocks to be placed</summary>
		public void InitializeArea(int x1, int z1, int x2, int z2)
		{
			for (int x = x1.RegionCoord(); x <= x2.RegionCoord(); x++)
			{
				for (int z = z1.RegionCoord(); z <= z2.RegionCoord(); z++)
				{
					AddRegion(x, z);
				}
			}
		}

		[Obsolete("Use TryGetRegion instead for better performance and direct access to the region", false)]
		/// <summary>Is the location within the world's generated regions?</summary>
		public bool IsWithinBoundaries(int x, int y, int z)
		{
			x = (int)Math.Floor(x / 512f);
			z = (int)Math.Floor(z / 512f);
			return regions.ContainsKey(new RegionLocation(x, z));
		}

		/// <summary>Gets the region at the given block coordinates, if present</summary>
		public Region TryGetRegion(int x, int z)
		{
			regions.TryGetValue(new RegionLocation(x.RegionCoord(), z.RegionCoord()), out var region);
			return region;
		}

		///<summary>Returns true if the block at the given location is the default block (normally minecraft:stone).</summary>
		public bool IsDefaultBlock(int x, int y, int z)
		{
			var b = GetBlock(x, y, z);
			if (b == null) return false;
			return b == defaultBlock;
		}

		///<summary>Returns true if the block at the given location is air.</summary>
		public bool IsAir(int x, int y, int z)
		{
			var b = GetBlock(x, y, z);
			return b == null || b.IsAir;
		}

		///<summary>Returns true if the block at the given location is air and does actually exist (a chunk is present at the location).</summary>
		public bool IsAirNotNull(int x, int y, int z)
		{
			var b = GetBlock(x, y, z);
			return b != null && b.IsAir;
		}

		///<summary>Gets the block type at the given location.</summary>
		public ProtoBlock GetBlock(int x, int y, int z)
		{
			return TryGetRegion(x, z)?.GetBlock(x % 512, y, z % 512);
		}

		///<summary>Gets the full block state at the given location.</summary>
		public BlockState GetBlockState(int x, int y, int z)
		{
			return TryGetRegion(x, z)?.GetBlockState(x % 512, y, z % 512);
		}

		///<summary>Gets the biome at the given location.</summary>
		public BiomeID? GetBiome(int x, int z)
		{
			return TryGetRegion(x, z)?.GetBiome(x % 512,z % 512);
		}

		///<summary>Sets the biome at the given location.</summary>
		public void SetBiome(int x, int z, BiomeID biome)
		{
			TryGetRegion(x, z)?.SetBiome(x % 512, z % 512, biome);
		}

		/// <summary>
		/// Marks the given coordinate to be ticked when the respective chunk is loaded.
		/// </summary>
		public void MarkForTickUpdate(int x, int y, int z)
		{
			TryGetRegion(x, z)?.MarkForTickUpdate(x % 512, y, z % 512);
		}

		/// <summary>
		/// Unmarks a previously marked coordinate to be ticked when the respective chunk is loaded.
		/// </summary>
		public void UnmarkForTickUpdate(int x, int y, int z)
		{
			TryGetRegion(x, z)?.UnmarkForTickUpdate(x % 512, y, z % 512);
		}

		//private readonly object lockObj = new object();

		private Region GetRegionAt(int x, int z)
		{
			return TryGetRegion(x, z);
			/*lock (lockObj)
			{
				var region = TryGetRegion(x, z);
				if (region == null)
				{
					if (allowNew)
					{
						var rloc = new RegionLocation(x.RegionCoord(), z.RegionCoord());
						var r = new Region(rloc);
						regions.Add(rloc, r);
						return r;
					}
					else
					{
						return null;
					}
				}
				else
				{
					return region;
				}
			}*/
		}

		public bool AddRegion(int rx, int rz)
		{
			if (regions.ContainsKey(new RegionLocation(rx, rz)))
			{
				var rloc = new RegionLocation(rx, rz);
				var r = new Region(rloc);
				r.containingWorld = this;
				regions.Add(rloc, r);
				return true;
			}
			else
			{
				return false;
			}
		}

		///<summary>Sets the block type at the given location.</summary>
		public bool SetBlock(int x, int y, int z, string block)
		{
			return SetBlock(x, y, z, new BlockState(BlockList.Find(block)));
		}

		///<summary>Sets the block state at the given location.</summary>
		public bool SetBlock(int x, int y, int z, BlockState block)
		{
			if (y < 0 || y > 255) return false;
			var r = GetRegionAt(x, z);
			if (r != null)
			{
				return r.SetBlock(x % 512, y, z % 512, block);
			}
			else
			{
				return false;
			}
		}

		///<summary>Sets the default bock (normally minecraft:stone) at the given location. This method is faster than SetBlockAt.</summary>
		public void SetDefaultBlock(int x, int y, int z)
		{
			if (y < 0 || y > 255) return;
			var r = GetRegionAt(x, z);
			if (r != null)
			{
				r.SetDefaultBlock(x % 512, y, z % 512);
			}
			else
			{
				throw new ArgumentException($"The location was outside of the world: {x},{y},{z}");
			}
		}

		/// <summary>
		/// Generates a Heightmap from the specified area (With Z starting from top)
		/// </summary>
		public short[,] GetHeightmap(int xMin, int zMin, int xMax, int zMax, HeightmapType type)
		{
			short[,] hm = new short[xMax - xMin + 1, zMax - zMin + 1];
			for (int z = zMin; z <= zMax; z++)
			{
				for (int x = xMin; x <= xMax; x++)
				{
					hm[x - xMin, z - zMin] = GetRegionAt(x, z)?.GetChunk(x % 512, z % 512, false)?.GetHighestBlock(x % 16, z % 16, type) ?? short.MinValue;
				}
			}
			return hm;
		}

		/// <summary>
		/// Generates a colored overview map from the specified area (With Z starting from top)
		/// </summary>
		public Bitmap GetSurfaceMap(int xMin, int zMin, int xMax, int zMax, HeightmapType surfaceType, bool shading)
		{
			return GetSurfaceMap(xMin, zMin, GetHeightmap(xMin, zMin, xMax, zMax, surfaceType), shading);
		}

		public Bitmap GetSurfaceMap(int xMin, int zMin, short[,] heightmap, bool shading)
		{
			int xMax = xMin + heightmap.GetLength(0);
			int zMax = zMin + heightmap.GetLength(1);
			Bitmap bmp = new Bitmap(heightmap.GetLength(0), heightmap.GetLength(1), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			for (int z = zMin; z < zMax; z++)
			{
				for (int x = xMin; x < xMax; x++)
				{
					int y = heightmap[x - xMin, z - zMin];
					if (y < 0) continue;
					var block = GetBlock(x, y, z);
					int shade = 0;
					if (shading && z - 1 >= zMin)
					{
						if (block.IsWater)
						{
							//Water dithering
							var depth = GetWaterDepth(x, y, z);
							if (depth < 8) shade = 1;
							else if (depth < 16) shade = 0;
							else shade = -1;
							if (depth % 8 >= 4 && shade > -1)
							{
								if (x % 2 == z % 2) shade--;
							}
						}
						else
						{
							var above = heightmap[x - xMin, z - 1 - zMin];
							if (above > y) shade = -1;
							else if (above < y) shade = 1;
						}
					}
					var aboveBlock = GetBlock(x, y + 1, z);
					if (aboveBlock != null && aboveBlock.ID == "minecraft:snow") block = aboveBlock;
					bmp.SetPixel(x - xMin, z - zMin, Blocks.GetMapColor(block, shade));
				}
			}
			return bmp;
		}

		/// <summary>
		/// Gets the depth of the water at the given location, in blocks
		/// </summary>
		public int GetWaterDepth(int x, int y, int z)
		{
			return GetRegionAt(x, z)?.GetWaterDepth(x % 512, y, z % 512) ?? 0;
		}

		/// <summary>
		/// Gets the highest block at the given location.
		/// </summary>
		public short GetHighestBlock(int x, int z, HeightmapType heightmapType)
		{
			return GetRegionAt(x, z)?.GetHighestBlock(x % 512, z % 512, heightmapType) ?? short.MinValue;
		}

		public void WriteRegionFile(FileStream stream, int regionPosX, int regionPosZ)
		{
			RegionSerializer.WriteRegionToStream(regions[new RegionLocation(regionPosX, regionPosZ)], stream, gameVersion);
		}

		public void WriteWorldSave(string path, bool createRegionCopyDir = false)
		{
			Directory.CreateDirectory(path);

			var level = CreateLevelDAT(true);

			File.WriteAllBytes(Path.Combine(path, "level.dat"), level.WriteBytesGZip());

			Directory.CreateDirectory(Path.Combine(path, "region"));

			var options = new ParallelOptions() { MaxDegreeOfParallelism = 4 };
			string extension = gameVersion >= Version.FirstAnvilVersion ? "mca" : "mcr";
			Parallel.ForEach(regions, options, (KeyValuePair<RegionLocation, Region> region) =>
			{
				string name = $"r.{region.Key.x}.{region.Key.z}.{extension}";
				region.Value.WriteToFile(Path.Combine(path, "region"), gameVersion, name);
			});
			if(createRegionCopyDir)
			{
				Directory.CreateDirectory(Path.Combine(path, "region_original"));
				foreach(var f in Directory.GetFiles(Path.Combine(path, "region")))
				{
					File.Copy(f, Path.Combine(path, "region_original", Path.GetFileName(f)));
				}
			}
		}

		private NBTFile CreateLevelDAT(bool creativeModeWithCheats)
		{
			var serializer = LevelDATSerializer.CreateForVersion(gameVersion);
			var nbt = new NBTFile();
			serializer.WriteLevelDAT(this, nbt, creativeModeWithCheats);
			return nbt;
		}
	}
}

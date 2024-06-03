using Ionic.Zlib;
using MCUtils.Coordinates;
using MCUtils.IO;
using MCUtils.NBT;
using MCUtils.TileEntities;
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
		public bool IsWithinBoundaries(BlockCoord pos)
		{
			int x = (int)Math.Floor(pos.x / 512f);
			int z = (int)Math.Floor(pos.z / 512f);
			return regions.ContainsKey(new RegionLocation(x, z));
		}

		/// <summary>Gets the region at the given block coordinates, if present</summary>
		public Region GetRegion(int x, int z)
		{
			regions.TryGetValue(new RegionLocation(x.RegionCoord(), z.RegionCoord()), out var region);
			return region;
		}

		public bool TryGetRegion(int x, int z, out Region region)
		{
			return regions.TryGetValue(new RegionLocation(x.RegionCoord(), z.RegionCoord()), out region);
		}

		///<summary>Returns true if the block at the given location is the default block (normally minecraft:stone).</summary>
		public bool IsDefaultBlock(BlockCoord pos)
		{
			var b = GetBlock(pos);
			if (b == null) return false;
			return b == defaultBlock;
		}

		///<summary>Returns true if the block at the given location is air or null.</summary>
		public bool IsAirOrNull(BlockCoord pos)
		{
			var b = GetBlock(pos);
			return b == null || b.IsAir;
		}

		///<summary>Returns true if the block at the given location is air and does actually exist (a chunk is present at the location).</summary>
		public bool IsAirNotNull(BlockCoord pos)
		{
			var b = GetBlock(pos);
			return b != null && b.IsAir;
		}

		///<summary>Gets the block type at the given location.</summary>
		public ProtoBlock GetBlock(BlockCoord pos)
		{
			return GetRegion(pos.x, pos.z)?.GetBlock(pos.LocalRegionCoords);
		}

		///<summary>Gets the full block state at the given location.</summary>
		public BlockState GetBlockState(BlockCoord pos)
		{
			return GetRegion(pos.x, pos.z)?.GetBlockState(pos.LocalRegionCoords);
		}

		///<summary>Gets the tile entity for the block at the given location (if available).</summary>
		public TileEntity GetTileEntity(BlockCoord pos)
		{
			return GetRegion(pos.x, pos.z)?.GetTileEntity(pos.LocalRegionCoords);
		}

		///<summary>Gets the biome at the given location.</summary>
		public BiomeID? GetBiome(int x, int z)
		{
			return GetRegion(x, z)?.GetBiomeAt(x.Mod(512), z.Mod(512));
		}

		///<summary>Gets the biome at the given location.</summary>
		public BiomeID? GetBiome(BlockCoord pos)
		{
			return GetRegion(pos.x, pos.z)?.GetBiomeAt(pos.LocalRegionCoords);
		}

		///<summary>Sets the biome at the given location.</summary>
		public void SetBiome(int x, int z, BiomeID biome)
		{
			GetRegion(x, z)?.SetBiomeAt(x.Mod(512), z.Mod(512), biome);
		}

		///<summary>Sets the biome at the given location.</summary>
		public void SetBiome(BlockCoord pos, BiomeID biome)
		{
			GetRegion(pos.x, pos.z)?.SetBiomeAt(pos.LocalRegionCoords, biome);
		}

		/// <summary>
		/// Marks the given coordinate to be ticked when the respective chunk is loaded.
		/// </summary>
		public void MarkForTickUpdate(BlockCoord pos)
		{
			GetRegion(pos.x, pos.z)?.MarkForTickUpdate(pos.LocalRegionCoords);
		}

		/// <summary>
		/// Unmarks a previously marked coordinate to be ticked when the respective chunk is loaded.
		/// </summary>
		public void UnmarkForTickUpdate(BlockCoord pos)
		{
			GetRegion(pos.x, pos.z)?.UnmarkForTickUpdate(pos.LocalRegionCoords);
		}

		//private readonly object lockObj = new object();

		private Region GetRegionAt(int x, int z)
		{
			return GetRegion(x, z);
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

		public void InitializeChunks(int blockXMin, int blockZMin, int blockXMax, int blockZMax, bool replaceExistingChunks)
		{
			//TODO
		}

		///<summary>Sets the block type at the given location.</summary>
		public bool SetBlock(BlockCoord pos, string block, bool allowNewChunks = false)
		{
			return SetBlock(pos, new BlockState(BlockList.Find(block)), allowNewChunks);
		}

		///<summary>Sets the block state at the given location.</summary>
		public bool SetBlock(BlockCoord pos, BlockState block, bool allowNewChunks = false)
		{
			//TODO: Check for varying build limits
			//if (pos.y < 0 || pos.y > 255) return false;
			var r = GetRegionAt(pos.x, pos.z);
			if (r != null)
			{
				return r.SetBlock(pos.LocalRegionCoords, block, allowNewChunks);
			}
			else
			{
				return false;
			}
		}

		///<summary>Sets the default bock (normally minecraft:stone) at the given location. This method is faster than SetBlockAt.</summary>
		public void SetDefaultBlock(BlockCoord pos, bool allowNewChunks = false)
		{
			//TODO: Check for variying build limits (-64 to 256) in 1.18+, 128 in older versions, etc..
			//if (pos.y < 0 || pos.y > 255) return;
			var r = GetRegionAt(pos.x, pos.z);
			if (r != null)
			{
				r.SetDefaultBlock(pos.LocalRegionCoords, allowNewChunks);
			}
			else
			{
				throw new ArgumentException($"The location was outside of the world: {pos}");
			}
		}

		///<summary>Sets the tile entity at the given location.</summary>
		public bool SetTileEntity(BlockCoord pos, TileEntity te)
		{
			if(TryGetRegion(pos.x, pos.z, out var region))
			{
				return region.SetTileEntity(pos, te);
			}
			else
			{
				return false;
			}
		}

		public bool SetBlockWithTileEntity(BlockCoord pos, BlockState block, TileEntity te, bool allowNewChunks = false)
		{
			if(SetBlock(pos, block, allowNewChunks))
			{
				return SetTileEntity(pos, te);
			}
			else
			{
				return false;
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
					hm[x - xMin, z - zMin] = GetRegionAt(x, z)?.GetChunk(x.Mod(512), z.Mod(512), false)?.GetHighestBlock(x.Mod(16), z.Mod(16), type) ?? short.MinValue;
				}
			}
			return hm;
		}

		/// <summary>
		/// Generates a colored overview map from the specified area (With Z starting from top)
		/// </summary>
		public IBitmap GetSurfaceMap(int xMin, int zMin, int xMax, int zMax, HeightmapType surfaceType, bool shading)
		{
			return GetSurfaceMap(xMin, zMin, GetHeightmap(xMin, zMin, xMax, zMax, surfaceType), shading);
		}

		public IBitmap GetSurfaceMap(int xMin, int zMin, short[,] heightmap, bool shading)
		{
			int xMax = xMin + heightmap.GetLength(0);
			int zMax = zMin + heightmap.GetLength(1);
			if(Bitmaps.BitmapFactory == null)
			{
				throw new ArgumentNullException("No bitmap factory was provided.");
			}
			var bmp = Bitmaps.BitmapFactory.Create(heightmap.GetLength(0), heightmap.GetLength(1));
			for (int z = zMin; z < zMax; z++)
			{
				for (int x = xMin; x < xMax; x++)
				{
					int y = heightmap[x - xMin, z - zMin];
					if (y < 0) continue;
					var block = GetBlock((x, y, z));
					int shade = 0;
					if (shading && z - 1 >= zMin)
					{
						if (block.IsWater)
						{
							//Water dithering
							var depth = GetWaterDepth((x, y, z));
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
					var aboveBlock = GetBlock((x, y + 1, z));
					if (aboveBlock != null && aboveBlock.ID == "minecraft:snow") block = aboveBlock;
					bmp.SetPixel(x - xMin, z - zMin, Blocks.GetMapColor(block, shade));
				}
			}
			return bmp;
		}

		/// <summary>
		/// Gets the depth of the water at the given location, in blocks
		/// </summary>
		public int GetWaterDepth(BlockCoord pos)
		{
			return GetRegionAt(pos.x, pos.z)?.GetWaterDepth(pos.LocalRegionCoords) ?? 0;
		}

		/// <summary>
		/// Gets the highest block at the given location.
		/// </summary>
		public short GetHighestBlock(int x, int z, HeightmapType heightmapType)
		{
			return GetRegionAt(x, z)?.GetHighestBlock(x.Mod(512), z.Mod(512), heightmapType) ?? short.MinValue;
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

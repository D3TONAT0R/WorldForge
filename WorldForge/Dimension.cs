using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WorldForge.Biomes;
using WorldForge.Chunks;
using WorldForge.Coordinates;
using WorldForge.IO;
using WorldForge.NBT;
using WorldForge.Regions;
using WorldForge.TileEntities;

namespace WorldForge
{
	public class Dimension
	{
		public World ParentWorld { get; private set; }

		public DimensionID dimensionID;

		public BiomeID defaultBiome = BiomeID.the_void;

		public Dictionary<RegionLocation, Region> regions = new Dictionary<RegionLocation, Region>();

		public static Dimension CreateNew(World parentWorld, DimensionID id, BiomeID defaultBiome)
		{
			return new Dimension(parentWorld, id, defaultBiome);
		}

		public static Dimension CreateNew(World parentWorld, DimensionID id, BiomeID defaultBiome, int regionLowerX, int regionLowerZ, int regionUpperX, int regionUpperZ)
		{
			var dim = CreateNew(parentWorld, id, defaultBiome);

			return dim;
		}

		public static Dimension CreateOverworld(World parentWorld, BiomeID defaultBiome = BiomeID.plains)
		{
			return CreateNew(parentWorld, DimensionID.Overworld, defaultBiome);
		}

		public static Dimension CreateNether(World parentWorld, BiomeID defaultBiome = BiomeID.nether_wastes)
		{
			return CreateNew(parentWorld, DimensionID.Nether, defaultBiome);
		}

		public static Dimension CreateTheEnd(World parentWorld, BiomeID defaultBiome = BiomeID.the_end)
		{
			return CreateNew(parentWorld, DimensionID.TheEnd, defaultBiome);
		}

		private Dimension(World parentWorld, DimensionID dimensionID, BiomeID defaultBiome)
		{
			ParentWorld = parentWorld;
			this.dimensionID = dimensionID;
			this.defaultBiome = defaultBiome;
		}

		//TODO: Danger, loads entire dimension at once
		public static Dimension Load(World world, string worldRoot, string subdir, DimensionID id, GameVersion? gameVersion = null, bool throwOnRegionLoadFail = false)
		{
			var dim = new Dimension(world, id, BiomeID.the_void);
			dim.regions = new Dictionary<RegionLocation, Region>();
			bool isAlphaFormat = world.GameVersion < GameVersion.Beta_1(3);
			//TODO: how to load alpha chunks?
			var dataRootDir = !string.IsNullOrEmpty(subdir) ? Path.Combine(worldRoot, subdir) : worldRoot;
			if(isAlphaFormat)
			{
				LoadAlphaChunkFiles(dataRootDir, world.GameVersion, throwOnRegionLoadFail, dim);
			}
			else
			{
				var regionDir = Path.Combine(dataRootDir, "region");
				if(Directory.Exists(regionDir))
				{
					LoadRegionFiles(regionDir, world.GameVersion, throwOnRegionLoadFail, dim);
				}
			}
			return dim;
		}

		private static void LoadAlphaChunkFiles(string chunksRootDir, GameVersion? version, bool throwOnRegionLoadFail, Dimension dim)
		{
			var cs = ChunkSerializer.GetOrCreateSerializer<ChunkSerializerAlpha>(version ?? new GameVersion(GameVersion.Stage.Infdev, 1, 0, 0));
			foreach(var f in Directory.GetFiles(chunksRootDir, "c.*.dat", SearchOption.AllDirectories))
			{
				string filename = Path.GetFileName(f);
				try
				{
					var split = filename.Split('.');
					string xBase36 = split[1];
					string zBase36 = split[2];
					var chunkPos = new ChunkCoord(Convert.ToInt32(xBase36, 36), Convert.ToInt32(zBase36, 36));
					var regionPos = new RegionLocation(chunkPos.x >> 5, chunkPos.z >> 5);
					if(!dim.TryGetRegion(regionPos.x, regionPos.z, out var region))
					{
						region = new Region(regionPos, dim);
						dim.regions.Add(regionPos, region);
					}
					//TODO: find a way to enable late loading of alpha chunks
					var nbt = new NBTFile(f);
					var chunk = ChunkData.CreateFromNBT(region, chunkPos, nbt);
					cs.ReadChunkNBT(chunk, version);
				}
				catch(Exception e) when(!throwOnRegionLoadFail)
				{
					Console.WriteLine($"Failed to load region '{filename}': {e.Message}");
				}
			}
		}

		private static void LoadRegionFiles(string regionRootDir, GameVersion? gameVersion, bool throwOnRegionLoadFail, Dimension dim)
		{
			foreach(var f in Directory.GetFiles(regionRootDir, "*.mc*"))
			{
				var filename = Path.GetFileName(f);
				if(Regex.IsMatch(filename, @"^r.-*\d.-*\d.mc(a|r)"))
				{
					try
					{
						Region region = RegionLoader.LoadRegion(f, gameVersion);
						region.ParentDimension = dim;
						dim.regions.Add(region.regionPos, region);
					}
					catch(Exception e) when(!throwOnRegionLoadFail)
					{
						Console.WriteLine($"Failed to load region '{filename}': {e.Message}");
					}
				}
				else
				{
					Console.WriteLine($"Invalid file '{filename}' in region folder.");
				}
			}
		}

		public static void GetWorldInfo(string worldSaveDir, out string worldName, out GameVersion gameVersion, out RegionLocation[] regions)
		{
			NBTFile levelDat = new NBTFile(Path.Combine(worldSaveDir, "level.dat"));
			var dataComp = levelDat.contents.GetAsCompound("Data");

			gameVersion = GameVersion.FromDataVersion(dataComp.Get<int>("DataVersion")) ?? GameVersion.FirstVersion;
			worldName = dataComp.Get<string>("LevelName");
			var regionList = new List<RegionLocation>();
			foreach(var f in Directory.GetFiles(Path.Combine(worldSaveDir, "region"), "*.mc*"))
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

		/// <summary>Instantiates empty regions in the specified area, allowing for blocks to be placed.</summary>
		public void InitializeArea(int regionLowerX, int regionLowerZ, int regionUpperX, int regionUpperZ)
		{
			for(int x = regionLowerX; x <= regionUpperX; x++)
			{
				for(int z = regionLowerZ; z <= regionUpperZ; z++)
				{
					var loc = new RegionLocation(x, z);
					regions.Add(loc, new Region(loc, this));
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
			if(b == null) return false;
			return b == World.DEFAULT_BLOCK;
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
			var rloc = new RegionLocation(rx, rz);
			if(!regions.ContainsKey(rloc))
			{
				var r = new Region(rloc, this);
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
			if(r != null)
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
			if(r != null)
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
			for(int z = zMin; z <= zMax; z++)
			{
				for(int x = xMin; x <= xMax; x++)
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
			for(int z = zMin; z < zMax; z++)
			{
				for(int x = xMin; x < xMax; x++)
				{
					int y = heightmap[x - xMin, z - zMin];
					if(y < 0) continue;
					var block = GetBlock((x, y, z));
					int shade = 0;
					if(shading && z - 1 >= zMin)
					{
						if(block.IsWater)
						{
							//Water dithering
							var depth = GetWaterDepth((x, y, z));
							if(depth < 8) shade = 1;
							else if(depth < 16) shade = 0;
							else shade = -1;
							if(depth % 8 >= 4 && shade > -1)
							{
								if(x % 2 == z % 2) shade--;
							}
						}
						else
						{
							var above = heightmap[x - xMin, z - 1 - zMin];
							if(above > y) shade = -1;
							else if(above < y) shade = 1;
						}
					}
					var aboveBlock = GetBlock((x, y + 1, z));
					if(aboveBlock != null && aboveBlock.ID == "minecraft:snow") block = aboveBlock;
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

		public void WriteRegionFile(FileStream stream, int regionPosX, int regionPosZ, GameVersion gameVersion)
		{
			RegionSerializer.WriteRegionToStream(regions[new RegionLocation(regionPosX, regionPosZ)], stream, gameVersion);
		}

		//TODO: add support to write alpha chunks
		public void WriteData(string rootDir, GameVersion gameVersion, bool createRegionCopyDir = false)
		{
			Directory.CreateDirectory(rootDir);
			Directory.CreateDirectory(Path.Combine(rootDir, "region"));

			var options = new ParallelOptions() { MaxDegreeOfParallelism = 4 };
			string extension = gameVersion >= GameVersion.FirstAnvilVersion ? "mca" : "mcr";
			Parallel.ForEach(regions, options, (KeyValuePair<RegionLocation, Region> region) =>
			{
				string name = $"r.{region.Key.x}.{region.Key.z}.{extension}";
				region.Value.WriteToFile(Path.Combine(rootDir, "region"), gameVersion, name);
			});
			if(createRegionCopyDir)
			{
				Directory.CreateDirectory(Path.Combine(rootDir, "region_original"));
				foreach(var f in Directory.GetFiles(Path.Combine(rootDir, "region")))
				{
					File.Copy(f, Path.Combine(rootDir, "region_original", Path.GetFileName(f)));
				}
			}
		}
	}
}

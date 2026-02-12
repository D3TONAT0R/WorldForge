using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

		public string RelativeSourceDirectory { get; set; }

		public DimensionID dimensionID;

		public BiomeID DefaultBiome
		{
			get => defaultBiome;
			set => defaultBiome = value ?? throw new NullReferenceException("Default biome cannot be null.");
		}

		private BiomeID defaultBiome = BiomeID.TheVoid;

		public ConcurrentDictionary<RegionLocation, Region> regions = new ConcurrentDictionary<RegionLocation, Region>();

		#region Creation methods
		public static Dimension CreateNew(World parentWorld, DimensionID id, BiomeID defaultBiome)
		{
			return new Dimension(parentWorld, id, defaultBiome);
		}

		public static Dimension CreateNew(World parentWorld, DimensionID id, BiomeID defaultBiome, int regionLowerX, int regionLowerZ, int regionUpperX, int regionUpperZ)
		{
			var dim = CreateNew(parentWorld, id, defaultBiome);
			dim.InitializeArea(regionLowerX, regionLowerZ, regionUpperX, regionUpperZ);
			return dim;
		}

		public static Dimension CreateOverworld(World parentWorld, BiomeID defaultBiome)
		{
			return CreateNew(parentWorld, DimensionID.Overworld, defaultBiome);
		}

		public static Dimension CreateOverworld(World parentWorld)
		{
			return CreateNew(parentWorld, DimensionID.Overworld, BiomeID.Plains);
		}

		public static Dimension CreateNether(World parentWorld, BiomeID defaultBiome)
		{
			return CreateNew(parentWorld, DimensionID.Nether, defaultBiome);
		}

		public static Dimension CreateNether(World parentWorld)
		{
			return CreateNew(parentWorld, DimensionID.Nether, BiomeID.NetherWastes);
		}

		public static Dimension CreateTheEnd(World parentWorld, BiomeID defaultBiome)
		{
			return CreateNew(parentWorld, DimensionID.TheEnd, defaultBiome);
		}

		public static Dimension CreateTheEnd(World parentWorld)
		{
			return CreateNew(parentWorld, DimensionID.TheEnd, BiomeID.TheEnd);
		}

		private Dimension(World parentWorld, DimensionID dimensionID, BiomeID defaultBiome)
		{
			ParentWorld = parentWorld;
			this.dimensionID = dimensionID;
			DefaultBiome = defaultBiome;
		}

		public static Dimension Load(World world, string worldRoot, string subdir, DimensionID id, GameVersion? gameVersion = null, bool throwOnRegionLoadFail = false)
		{
			var dimensionRootDir = !string.IsNullOrEmpty(subdir) ? Path.Combine(worldRoot, subdir) : worldRoot;
			if (!Directory.Exists(dimensionRootDir)) return null;
			var dim = new Dimension(world, id, BiomeID.TheVoid);
			dim.RelativeSourceDirectory = subdir ?? "";
			dim.regions = new ConcurrentDictionary<RegionLocation, Region>();
			var version = gameVersion ?? world?.GameVersion ?? GameVersion.FirstAnvilVersion;
			bool isAlphaFormat = version < GameVersion.Beta_1(3);
			if (isAlphaFormat)
			{
				LoadAlphaChunkFiles(dimensionRootDir, version, throwOnRegionLoadFail, dim);
			}
			else
			{
				LoadRegionFiles(dimensionRootDir, version, throwOnRegionLoadFail, dim);
			}
			return dim;
		}

		public static Dimension LoadServerDimension(World world, string worldRoot, string subdir, out DimensionID id, GameVersion? gameVersion = null, bool throwOnRegionLoadFail = false)
		{
			var dimRoot = Path.Combine(worldRoot, subdir);
			string contentSubdir;
			if (Directory.Exists(Path.Combine(dimRoot, "region")))
			{
				// It's a normal overworld dimension
				id = DimensionID.Overworld;
				return Load(world, dimRoot, "", id, gameVersion, throwOnRegionLoadFail);
			}
			if (Directory.Exists(Path.Combine(dimRoot, "dimensions")))
			{
				var namespaceDir = Directory.GetDirectories(Path.Combine(dimRoot, "dimensions")).FirstOrDefault();
				if (namespaceDir != null)
				{
					var idDir = Directory.GetDirectories(namespaceDir).FirstOrDefault();
					if (idDir != null)
					{
						var ns = Path.GetFileName(namespaceDir);
						var name = Path.GetFileName(idDir);
						id = DimensionID.FromID($"{ns}:{name}");
						contentSubdir = Path.Combine("dimensions", ns, name);
					}
					else
					{
						throw new InvalidOperationException($"Empty dimensions directory");
					}
				}
				else
				{
					throw new InvalidOperationException($"Empty dimensions directory");
				}
			}
			else
			{
				// Get the first directory starting with "DIM"
				var dimDirs = Directory.GetDirectories(dimRoot, "DIM*");
				if (dimDirs.Length > 0)
				{
					if (dimDirs.Length > 1)
					{
						throw new InvalidOperationException($"Multiple dimension directories found in server world: {string.Join(", ", dimDirs)}");
					}
					id = DimensionID.FromIndex(int.Parse(Path.GetFileName(dimDirs[0]).Substring(3)));
					contentSubdir = Path.GetFileName(dimDirs[0]);
				}
				else
				{
					throw new InvalidOperationException($"Empty dimensions directory");
				}
			}
			return Load(world, dimRoot, contentSubdir, id, gameVersion, throwOnRegionLoadFail);
		}

		public static Dimension FromRegionFolder(World world, string regionFolder, DimensionID id, GameVersion? gameVersion = null, bool throwOnRegionLoadFail = false)
		{
			var dim = new Dimension(world, id, BiomeID.TheVoid);
			dim.regions = new ConcurrentDictionary<RegionLocation, Region>();
			foreach (var mainFileName in Directory.GetFiles(regionFolder, "*.mc*"))
			{
				var filename = Path.GetFileName(mainFileName);
				if (Regex.IsMatch(filename, @"^r.-*\d+.-*\d+.mc(a|r)$"))
				{
					try
					{
						var paths = new RegionFilePaths(mainFileName, null, null);
						var region = RegionDeserializer.PreloadRegion(paths, dim, gameVersion);
						dim.AddRegion(region, true);
					}
					catch (Exception e) when (!throwOnRegionLoadFail)
					{
						Logger.Exception($"Failed to preload region '{filename}'", e);
					}
				}
				else
				{
					Logger.Error($"Invalid file '{filename}' in region folder.");
				}
			}
			return dim;
		}

		private static void LoadAlphaChunkFiles(string chunksRootDir, GameVersion? version, bool throwOnRegionLoadFail, Dimension dim)
		{
			var cs = ChunkSerializer.GetOrCreateSerializer<ChunkSerializerAlpha>(version ?? new GameVersion(GameVersion.Stage.Infdev, 1, 0, 0));
			foreach (var f in Directory.GetFiles(chunksRootDir, "c.*.dat", SearchOption.AllDirectories))
			{
				string filename = Path.GetFileName(f);
				try
				{
					var split = filename.Split('.');
					string xBase36 = split[1];
					string zBase36 = split[2];
					var chunkPos = new ChunkCoord(Convert.ToInt32(xBase36, 36), Convert.ToInt32(zBase36, 36));
					var regionPos = new RegionLocation(chunkPos.x >> 5, chunkPos.z >> 5);
					if (!dim.TryGetRegion(regionPos, out var region))
					{
						region = Region.CreateNew(regionPos, dim);
						dim.AddRegion(region, true);
					}
					var nbt = new NBTFile(f);
					var chunk = Chunk.CreateFromNBT(region, new ChunkCoord(chunkPos.x & 31, chunkPos.z & 31), new ChunkSourceData(nbt, null, null));
					region.chunks[chunkPos.x & 31, chunkPos.z & 31] = chunk;
				}
				catch (Exception e) when (!throwOnRegionLoadFail)
				{
					Logger.Exception($"Failed to load region '{filename}'", e);
				}
			}
		}

		private static void LoadRegionFiles(string dimensionRootDir, GameVersion? gameVersion, bool throwOnRegionLoadFail, Dimension dim)
		{
			var mainRegionDir = Path.Combine(dimensionRootDir, "region");
			if (!Directory.Exists(mainRegionDir))
			{
				Logger.Error($"Region folder not found: {mainRegionDir}");
				return;
			}

			var entitiesRegionDir = Path.Combine(dimensionRootDir, "entities");
			if (!Directory.Exists(entitiesRegionDir)) entitiesRegionDir = null;

			var poiRegionDir = Path.Combine(dimensionRootDir, "poi");
			if (!Directory.Exists(poiRegionDir)) poiRegionDir = null;


			foreach (var mainFileName in Directory.GetFiles(mainRegionDir, "*.mc*"))
			{
				var filename = Path.GetFileName(mainFileName);
				if (Regex.IsMatch(filename, @"^r.-*\d+.-*\d+.mc(a|r)$"))
				{
					try
					{
						string entitiesFileName = entitiesRegionDir != null ? Path.Combine(entitiesRegionDir, filename) : null;
						string poiFileName = poiRegionDir != null ? Path.Combine(poiRegionDir, filename) : null;
						var paths = new RegionFilePaths(mainFileName, entitiesFileName, poiFileName);
						var region = RegionDeserializer.PreloadRegion(paths, dim, gameVersion);
						dim.AddRegion(region, true);
					}
					catch (Exception e) when (!throwOnRegionLoadFail)
					{
						Logger.Exception($"Failed to preload region '{filename}'", e);
					}
				}
				else
				{
					Logger.Error($"Invalid file '{filename}' in region folder.");
				}
			}
		}

		public static void GetWorldInfo(string worldSaveDir, out string worldName, out GameVersion gameVersion, out List<RegionLocation> regions)
		{
			NBTFile levelDat = new NBTFile(Path.Combine(worldSaveDir, "level.dat"));
			var dataComp = levelDat.contents.GetAsCompound("Data");

			gameVersion = GameVersion.FromDataVersion(dataComp.Get<int>("DataVersion")) ?? GameVersion.FirstVersion;
			worldName = dataComp.Get<string>("LevelName");
			regions = new List<RegionLocation>();
			foreach (var f in Directory.GetFiles(Path.Combine(worldSaveDir, "region"), "*.mc*"))
			{
				if (RegionLocation.TryGetFromFileName(f, out var loc))
				{
					regions.Add(loc);
				}
			}
		}

		public void LoadAllChunks()
		{
			foreach (var r in regions)
			{
				r.Value.LoadAllChunks();
			}
		}

		#endregion

		#region Region and chunk related methods

		/// <summary>Instantiates empty regions in the specified area, allowing for blocks to be placed.</summary>
		public void InitializeArea(int regionLowerX, int regionLowerZ, int regionUpperX, int regionUpperZ)
		{
			for (int x = regionLowerX; x <= regionUpperX; x++)
			{
				for (int z = regionLowerZ; z <= regionUpperZ; z++)
				{
					var loc = new RegionLocation(x, z);
					AddRegion(Region.CreateNew(loc, this), true);
				}
			}
		}

		/// <summary>
		/// Gets the region at the given location, if present.
		/// </summary>
		public Region GetRegion(RegionLocation location)
		{
			regions.TryGetValue(location, out var region);
			return region;
		}

		public bool TryGetRegion(RegionLocation location, out Region region)
		{
			return regions.TryGetValue(location, out region);
		}

		/// <summary>Gets the region at the given block coordinates, if present</summary>
		public Region GetRegionAtBlock(BlockCoord pos)
		{
			regions.TryGetValue(pos.Region, out var region);
			return region;
		}

		public bool TryGetRegionAtBlock(BlockCoord pos, out Region region)
		{
			return regions.TryGetValue(pos.Region, out region);
		}

		public bool AddRegion(Region reg, bool throwException)
		{
			bool added = regions.TryAdd(reg.regionPos, reg);
			if (!added && throwException)
			{
				throw new ArgumentException("Region already exists in dimension.");
			}
			return added;
		}

		public bool CreateRegionIfMissing(RegionLocation loc)
		{
			if (!regions.ContainsKey(loc))
			{
				var r = Region.CreateNew(loc, this);
				return AddRegion(r, false);
			}
			return false;
		}

		public bool HasRegion(RegionLocation loc)
		{
			return regions.ContainsKey(loc);
		}


		public IEnumerable<Chunk> EnumerateChunks(bool keepLoadedRegions = true)
		{
			foreach (var region in regions.Values)
			{
				var loadedRegion = region;
				if (!region.IsLoaded)
				{
					if (keepLoadedRegions) region.Load();
					else loadedRegion = region.LoadClone();
				}
				foreach (var chunk in loadedRegion.chunks)
				{
					if (chunk != null)
					{
						yield return chunk;
					}
				}
			}
		}

		public IEnumerable<Chunk> EnumerateChunks(ChunkCoord min, ChunkCoord max, bool keepLoadedRegions = true, bool includeNullChunks = false)
		{
			RegionLocation minRegion = min.RegionCoord;
			RegionLocation maxRegion = max.RegionCoord;
			Dictionary<RegionLocation, Region> regionCache = new Dictionary<RegionLocation, Region>();
			for (int cx = min.x; cx <= max.x; cx++)
			{
				for (int cz = min.z; cz <= max.z; cz++)
				{
					var regionLoc = new RegionLocation(cx >> 5, cz >> 5);
					if (!HasRegion(regionLoc)) continue;
					if (!regionCache.TryGetValue(regionLoc, out var region))
					{
						var loadedRegion = regions[regionLoc];
						if (!loadedRegion.IsLoaded)
						{
							if (keepLoadedRegions) loadedRegion.Load();
							else loadedRegion = loadedRegion.LoadClone();
						}
						regionCache.Add(regionLoc, loadedRegion);
						region = loadedRegion;
					}
					var chunk = region.chunks[cx & 31, cz & 31];
					if (chunk == null)
					{
						if (includeNullChunks) yield return null;
						continue;
					}
					yield return chunk;
				}
			}
		}

		#endregion

		#region Block related methods

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
		public BlockID GetBlock(BlockCoord pos)
		{
			return GetRegionAtBlock(pos)?.GetBlock(pos.LocalRegionCoords);
		}

		///<summary>Gets the full block state at the given location.</summary>
		public BlockState GetBlockState(BlockCoord pos)
		{
			return GetRegionAtBlock(pos)?.GetBlockState(pos.LocalRegionCoords);
		}

		///<summary>Sets the block type at the given location.</summary>
		public bool SetBlock(BlockCoord pos, BlockID block, bool allowNewChunks = false)
		{
			return SetBlock(pos, BlockState.Simple(block), allowNewChunks);
		}

		///<summary>Sets the block state at the given location.</summary>
		public bool SetBlock(BlockCoord pos, BlockState block, bool allowNewChunks = false)
		{
			//TODO: Check for varying build limits
			//if (pos.y < 0 || pos.y > 255) return false;
			var r = GetRegionAtBlock(pos);
			if (r != null)
			{
				return r.SetBlock(pos.LocalRegionCoords, block, allowNewChunks);
			}
			else
			{
				return false;
			}
		}

		///<summary>Gets the tile entity for the block at the given location (if available).</summary>
		public TileEntity GetTileEntity(BlockCoord pos)
		{
			return GetRegionAtBlock(pos)?.GetTileEntity(pos.LocalRegionCoords);
		}

		///<summary>Sets the tile entity at the given location.</summary>
		public bool SetTileEntity(BlockCoord pos, TileEntity te)
		{
			if (TryGetRegionAtBlock(pos, out var region))
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
			if (TryGetRegionAtBlock(pos, out var region))
			{
				region.SetBlockWithTileEntity(pos, block, te, allowNewChunks);
			}
			return false;
		}

		#endregion

		#region Biome related methods

		///<summary>Gets the biome at the given location.</summary>
		public BiomeID GetBiome(int x, int z)
		{
			return GetRegionAtBlock(new BlockCoord(x, 0, z))?.GetBiomeAt(x & 511, z & 511);
		}

		///<summary>Gets the biome at the given location.</summary>
		public BiomeID GetBiome(BlockCoord pos)
		{
			return GetRegionAtBlock(pos)?.GetBiomeAt(pos.LocalRegionCoords);
		}

		///<summary>Sets the biome at the given location.</summary>
		public void SetBiome(int x, int z, BiomeID biome)
		{
			GetRegionAtBlock(new BlockCoord(x, 0, z))?.SetBiomeAt(x & 511, z & 511, biome);
		}

		///<summary>Sets the biome at the given location.</summary>
		public void SetBiome(BlockCoord pos, BiomeID biome)
		{
			GetRegionAtBlock(pos)?.SetBiomeAt(pos.LocalRegionCoords, biome);
		}

		#endregion

		#region Tick update methods

		/// <summary>
		/// Marks the given coordinate to be ticked when the respective chunk is loaded.
		/// </summary>
		public void MarkForTickUpdate(BlockCoord pos)
		{
			GetRegionAtBlock(pos)?.MarkForTickUpdate(pos.LocalRegionCoords);
		}

		/// <summary>
		/// Unmarks a previously marked coordinate to be ticked when the respective chunk is loaded.
		/// </summary>
		public void UnmarkForTickUpdate(BlockCoord pos)
		{
			GetRegionAtBlock(pos)?.UnmarkForTickUpdate(pos.LocalRegionCoords);
		}

		#endregion

		#region Convenience methods

		/// <summary>
		/// Generates a Heightmap from the specified area (With Z starting from top)
		/// </summary>
		public short[,] GetHeightmap(Boundary boundary, HeightmapType type, bool forceManualCalculation = false)
		{
			short[,] heightmap = new short[boundary.LengthX, boundary.LengthZ];
			Dictionary<ChunkCoord, short[,]> chunkHeightmaps = new Dictionary<ChunkCoord, short[,]>();
			for (int z = boundary.zMin; z < boundary.zMax; z++)
			{
				for (int x = boundary.xMin; x < boundary.xMax; x++)
				{
					var pos = new BlockCoord(x, 0, z);
					var chunkPos = pos.Chunk;
					if (!chunkHeightmaps.TryGetValue(chunkPos, out var chunkHeightmap))
					{
						var chunk = GetRegionAtBlock(pos)?.GetChunk(chunkPos.x & 31, chunkPos.z & 31);
						chunkHeightmap = chunk?.GetHeightmap(type, forceManualCalculation);
						chunkHeightmaps.Add(chunkPos, chunkHeightmap);
					}
					short height;
					if (chunkHeightmap != null)
					{
						height = chunkHeightmap[x & 0xF, z & 0xF];
					}
					else
					{
						height = GetHighestBlock(x, z, type);
					}
					heightmap[x - boundary.xMin, z - boundary.zMin] = height;
				}
			}
			return heightmap;
		}

		/// <summary>
		/// Gets the depth of the water at the given location, in blocks
		/// </summary>
		public int GetWaterDepth(BlockCoord pos)
		{
			return GetRegionAtBlock(pos)?.GetWaterDepth(pos.LocalRegionCoords) ?? 0;
		}

		/// <summary>
		/// Gets the highest block at the given location.
		/// </summary>
		public short GetHighestBlock(int x, int z, HeightmapType heightmapType)
		{
			return GetRegionAtBlock(new BlockCoord(x, 0, z))?.GetHighestBlock(x & 511, z & 511, heightmapType) ?? short.MinValue;
		}

		#endregion

		public void SaveFiles(string rootDir, GameVersion gameVersion)
		{
			Directory.CreateDirectory(rootDir);
			var parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = 4 };
			if (gameVersion >= GameVersion.FirstMCRVersion)
			{
				Directory.CreateDirectory(Path.Combine(rootDir, "region"));

				string extension = gameVersion >= GameVersion.FirstAnvilVersion ? "mca" : "mcr";
				Parallel.ForEach(regions, parallelOptions, region =>
				{
					string name = $"r.{region.Key.x}.{region.Key.z}.{extension}";
					region.Value.SaveToFiles(Path.Combine(rootDir), gameVersion, name);
				});
			}
			else
			{
				var alphaSerializer = (ChunkSerializerAlpha)ChunkSerializer.GetOrCreateSerializer<ChunkSerializerAlpha>(gameVersion);
				Parallel.ForEach(regions, parallelOptions, region =>
				{
					if (!region.Value.IsLoaded) region.Value.Load();
					foreach (var c in region.Value.chunks)
					{
						if (c != null)
						{
							ChunkSerializerAlpha.GetAlphaChunkPathAndName(c.WorldSpaceCoord, out var folder1, out var folder2, out var fileName);
							Directory.CreateDirectory(Path.Combine(rootDir, folder1, folder2));
							alphaSerializer.CreateChunkNBTs(c, out var file, out _, out _);
							File.WriteAllBytes(Path.Combine(rootDir, folder1, folder2, fileName), NBTSerializer.SerializeAsGzip(file, false));
						}
					}
				});
			}
		}
	}
}

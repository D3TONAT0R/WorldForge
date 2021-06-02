using Ionic.Zlib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using static MCUtils.ChunkData;
using static MCUtils.NBTContent;

namespace MCUtils
{
	public class World
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
		}

		public static readonly string defaultBlock = "minecraft:stone";

		public string worldName = "MCUtils generated world " + new Random().Next(10000);

		public Dictionary<RegionLocation, Region> regions;
		public bool allowNewRegions = false;

		public NBTContent levelDat;

		public World(int regionLowerX, int regionLowerZ, int regionUpperX, int regionUpperZ) : this(regionLowerX, regionLowerZ, regionUpperX, regionUpperZ, null)
		{

		}

		public World(int regionLowerX, int regionLowerZ, int regionUpperX, int regionUpperZ, string levelDatPath)
		{
			if (!string.IsNullOrEmpty(levelDatPath))
			{
				levelDat = new NBTContent(File.ReadAllBytes(levelDatPath), false);
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

		/// <summary>Is the location within the world's generated regions?</summary>
		public bool IsWithinBoundaries(int x, int y, int z)
		{
			if (y < 0 || y > 255) return false;
			x = (int)Math.Floor(x / 512f);
			z = (int)Math.Floor(z / 512f);
			return regions.ContainsKey(new RegionLocation(x, z));
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
			return b == null || b == "minecraft:air";
		}

		///<summary>Gets the block type at the given location.</summary>
		public string GetBlock(int x, int y, int z)
		{
			if (IsWithinBoundaries(x, y, z))
			{
				return regions[new RegionLocation(x.RegionCoord(), z.RegionCoord())].GetBlock(x % 512, y, z % 512);
			}
			else
			{
				return null;
			}
		}

		///<summary>Gets the full block state at the given location.</summary>
		public BlockState GetBlockState(int x, int y, int z)
		{
			if (IsWithinBoundaries(x, y, z))
			{
				return regions[new RegionLocation(x.RegionCoord(), z.RegionCoord())].GetBlockState(x % 512, y, z % 512);
			}
			else
			{
				return null;
			}
		}

		private Region GetRegionAt(int x, int z, bool allowNew)
		{
			var rloc = new RegionLocation(x.RegionCoord(), z.RegionCoord());
			if (!IsWithinBoundaries(x, 0, z))
			{
				if (allowNew)
				{
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
				return regions[rloc];
			}
		}

		///<summary>Sets the block type at the given location.</summary>
		public bool SetBlock(int x, int y, int z, string block)
		{
			return SetBlock(x, y, z, new BlockState(block));
		}

		///<summary>Sets the block state at the given location.</summary>
		public bool SetBlock(int x, int y, int z, BlockState block)
		{
			if (y < 0 || y > 255) return false;
			var r = GetRegionAt(x, z, allowNewRegions);
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
			var r = GetRegionAt(x, z, allowNewRegions);
			if (r != null)
			{
				r.SetDefaultBlock(x % 512, y, z % 512);
			}
			else
			{
				throw new ArgumentException($"The location was outside of the world: {x},{y},{z}");
			}
		}

		///<summary>Sets the biome at the given location.</summary>
		public void SetBiome(int x, int z, byte biome)
		{
			var r = GetRegionAt(x, z, false);
			if (r != null)
			{
				r.SetBiome(x % 512, z % 512, biome);
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
					hm[x-xMin, z-zMin] = GetRegionAt(x, z, false)?.GetChunk(x % 512, z % 512, false)?.GetHighestBlock(x % 16, z % 16, type) ?? short.MinValue;
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
					string block = GetBlock(x, y, z);
					int shade = 0;
					if (shading && z - 1 >= zMin)
					{
						if (block == "minecraft:water")
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
					bmp.SetPixel(x-xMin, z-zMin, Blocks.GetMapColor(block, shade));
				}
			}
			return bmp;
		}

		/// <summary>
		/// Gets the depth of the water at the given location, in blocks
		/// </summary>
		public int GetWaterDepth(int x, int y, int z)
		{
			return GetRegionAt(x, z, false)?.GetWaterDepth(x % 512, y, z % 512) ?? 0;
		}

		public void WriteRegionFile(FileStream stream, int regionPosX, int regionPosZ)
		{
			regions[new RegionLocation(regionPosX, regionPosZ)].WriteRegionToStream(stream);
		}

		public void WriteWorldSave(string path, int playerPosX, int playerPosZ)
		{
			Directory.CreateDirectory(path);

			int y = GetRegionAt(playerPosX, playerPosZ, false).GetChunk(playerPosX % 512, playerPosZ % 512, false).GetHighestBlock(playerPosX % 16, playerPosZ % 16);
			levelDat = CreateLevelDAT(playerPosX, y, playerPosZ, true);

			List<byte> levelDATBytes = new List<byte>();
			levelDat.WriteToBytes(levelDATBytes, false);
			var compressedLevelDAT = GZipStream.CompressBuffer(levelDATBytes.ToArray());

			File.WriteAllBytes(Path.Combine(path, "level.dat"), compressedLevelDAT);
			MCUtilsConsole.WriteLineSpecial("DAT written");

			Directory.CreateDirectory(Path.Combine(path, "region"));

			foreach (var region in regions)
			{
				string name = $"r.{region.Key.x}.{region.Key.z}.mca";
				using (var stream = new FileStream(Path.Combine(path, "region", name), FileMode.Create))
				{
					region.Value.WriteRegionToStream(stream);
				}
			}
		}

		private NBTContent CreateLevelDAT(int playerPosX, int playerPosY, int playerPosZ, bool creativeModeWithCheats)
		{
			NBTContent levelDAT = new NBTContent();
			var data = levelDAT.contents.AddCompound("Data");

			data.Add<int>("DataVersion", 2504);
			data.Add<byte>("initialized", 0);
			data.Add<long>("LastPlayed", 0);
			data.Add<byte>("WasModded", 0);

			var datapacks = data.AddCompound("DataPacks");
			datapacks.Add("Disabled", new ListContainer(NBTTag.TAG_String));
			datapacks.Add("Enabled", new ListContainer(NBTTag.TAG_String)).Add(null, "vanilla");

			data.AddCompound("GameRules");

			data.Add("Player", CreatePlayerCompound(playerPosX, playerPosY, playerPosZ, creativeModeWithCheats));

			var versionComp = data.AddCompound("Version");
			versionComp.Add<int>("Id", 2504);
			versionComp.Add<string>("Name", "1.16");
			versionComp.Add<byte>("Snapshot", 0);

			var worldGenComp = data.AddCompound("WorldGenSettings");
			worldGenComp.AddCompound("dimensions");
			worldGenComp.Add<byte>("bonus_chest", 0);
			worldGenComp.Add<byte>("generate_features", 1);
			worldGenComp.Add<long>("seed", new Random().Next(int.MaxValue));

			data.AddList("ScheduledEvents", NBTTag.TAG_List);
			data.AddList("ServerBrands", NBTTag.TAG_String).Add("vanilla");
			data.Add<byte>("allowCommands", (byte)(creativeModeWithCheats ? 1 : 0));

			data.Add<double>("BorderCenterX", 0);
			data.Add<double>("BorderCenterZ", 0);
			data.Add<double>("BorderDamagePerBlock", 0.2d);
			data.Add<double>("BorderSafeZone", 5);
			data.Add<double>("BorderSize", 60000000);
			data.Add<double>("BorderSizeLerpTarget", 60000000);
			data.Add<long>("BorderSizeLerpTime", 0);
			data.Add<double>("BorderWarningBlocks", 5);
			data.Add<double>("BorderWarningTime", 15);

			data.Add<int>("clearWeatherTime", 0);
			data.Add<long>("DayTime", 0);
			data.Add<byte>("raining", 0);
			data.Add<int>("rainTime", new Random().Next(20000, 200000));
			data.Add<byte>("thundering", 0);
			data.Add<int>("thunderTime", new Random().Next(50000, 100000));
			data.Add<long>("Time", 0);
			data.Add<int>("version", 19133);

			data.Add<byte>("Difficulty", 2);
			data.Add<byte>("DifficultyLocked", 0);

			data.Add<int>("GameType", creativeModeWithCheats ? 1 : 0);
			data.Add<byte>("hardcore", 0);

			data.Add<string>("LevelName", worldName);

			data.Add<float>("SpawnAngle", 0);
			data.Add<int>("SpawnX", playerPosX);
			data.Add<int>("SpawnY", playerPosY);
			data.Add<int>("SpawnZ", playerPosZ);

			data.Add<int>("WanderingTraderSpawnChance", 50);
			data.Add<int>("WanderingTraderSpawnDelay", 24000);

			return levelDAT;
		}

		private CompoundContainer CreatePlayerCompound(int posX, int posY, int posZ, bool creativeModeWithCheats)
		{
			var player = new CompoundContainer();

			var abilities = player.AddCompound("abilities");
			abilities.Add<byte>("flying", 0);
			abilities.Add<float>("flySpeed", 0.05f);
			abilities.Add<byte>("instabuild", (byte)(creativeModeWithCheats ? 1 : 0));
			abilities.Add<byte>("invulnerable", (byte)(creativeModeWithCheats ? 1 : 0));
			abilities.Add<byte>("mayBuild", 0);
			abilities.Add<byte>("mayfly", (byte)(creativeModeWithCheats ? 1 : 0));
			abilities.Add<float>("walkSpeed", 0.1f);

			player.AddCompound("Brain").AddCompound("memories");
			player.AddCompound("recipeBook");
			player.Add("Attributes", new ListContainer(NBTTag.TAG_Compound));
			player.Add("EnderItems", new ListContainer(NBTTag.TAG_Compound));
			player.Add("Inventory", new ListContainer(NBTTag.TAG_Compound));
			player.AddList("Motion", NBTTag.TAG_Double).AddRange(0d, 0d, 0d);

			var pos = player.AddList("Pos", NBTTag.TAG_Double);
			pos.Add<double>(posX);
			pos.Add<double>(posY);
			pos.Add<double>(posZ);
			player.AddList("Rotation", NBTTag.TAG_Float).AddRange(0f, 0f);

			player.Add("AbsorptionAmount", 0f);
			player.Add<short>("Air", 300);
			player.Add<short>("DeathTime", 0);
			player.Add<string>("Dimension", "minecraft:overworld");
			player.Add<float>("FallDistance", 0);
			player.Add<byte>("FallFlying", 0);
			player.Add<short>("Fire", -20);
			player.Add<float>("foodExhaustionLevel", 0);
			player.Add<int>("foodLevel", 20);
			player.Add<float>("foodSaturationLevel", 5);
			player.Add<int>("foodTickTimer", 0);
			player.Add<float>("Health", 20);
			player.Add<int>("HurtByTimestamp", 0);
			player.Add<short>("HurtTime", 0);
			player.Add<byte>("Invulnerable", 0);
			player.Add<byte>("OnGround", 0);
			player.Add<int>("playerGameType", creativeModeWithCheats ? 1 : 0);
			player.Add<int>("Score", 0);
			player.Add<byte>("seenCredits", 0);
			player.Add<int>("SelectedItemSlot", 0);
			player.Add<short>("SleepTimer", 0);
			player.Add<int>("XpLevel", 0);
			player.Add<float>("XpP", 0);
			player.Add<int>("XpSeed", 0);
			player.Add<int>("XpTotal", 0);

			player.Add<int>("DataVersion", 2504);

			//UUID?
			player.Add<int[]>("UUID", new int[] { 0, 0, 0, 0 });

			return player;
		}
	}
}

﻿using System;
using System.Collections.Generic;
using System.Text;
using static MCUtils.ChunkData;

namespace MCUtils {
	public class World {

		public struct RegionLocation {
			public int x;
			public int z;

			public RegionLocation(int regionX, int regionZ) {
				x = regionX;
				z = regionZ;
			}
		}

		public static readonly string defaultBlock = "minecraft:stone";

		public Dictionary<RegionLocation, Region> regions;
		public bool allowNewRegions = false;

		public World(int worldSizeX, int worldSizeZ) {
			regions = new Dictionary<RegionLocation, Region>();
			for(int x = 0; x * 512 < worldSizeX; x++) {
				for(int z = 0; z * 512 < worldSizeZ; z++) {
					regions.Add(new RegionLocation(x, z), new Region());
				}
			}
		}

		/// <summary>Is the location within the world's generated regions?</summary>
		public bool IsWithinBoundaries(int x, int y, int z) {
			if(y < 0 || y > 255) return false;
			x = (int)Math.Floor(x / 512f);
			z = (int)Math.Floor(z / 512f);
			return regions.ContainsKey(new RegionLocation(x, z));
		}

		///<summary>Returns true if the block at the given location is the default block (normally minecraft:stone).</summary>
		public bool IsDefaultBlock(int x, int y, int z) {
			var b = GetBlock(x, y, z);
			if(b == null) return false;
			return b == defaultBlock;
		}

		///<summary>Gets the block type at the given location.</summary>
		public string GetBlock(int x, int y, int z) {
			if(IsWithinBoundaries(x, y, z)) {
				return regions[new RegionLocation(x.RegionCoord(), z.RegionCoord())].GetBlock(x % 512, y, z % 512);
			} else {
				return null;
			}
		}

		///<summary>Gets the full block state at the given location.</summary>
		public BlockState GetBlockState(int x, int y, int z) {
			if(IsWithinBoundaries(x, y, z)) {
				return regions[new RegionLocation(x.RegionCoord(), z.RegionCoord())].GetBlockState(x % 512, y, z % 512);
			} else {
				return null;
			}
		}

		private Region GetRegionAt(int x, int z, bool allowNew) {
			var rloc = new RegionLocation(x.RegionCoord(), z.RegionCoord());
			if(!IsWithinBoundaries(x, 0, z)) {
				if(allowNew) {
					var r = new Region();
					regions.Add(rloc, r);
					return r;
				} else {
					return null;
				}
			} else {
				return regions[rloc];
			}
		}

		///<summary>Sets the block type at the given location.</summary>
		public bool SetBlock(int x, int y, int z, string block) {
			return SetBlock(x, y, z, new BlockState(block));
		}

		///<summary>Sets the block state at the given location.</summary>
		public bool SetBlock(int x, int y, int z, BlockState block) {
			if(y < 0 || y > 255) return false;
			var r = GetRegionAt(x, z, allowNewRegions);
			if(r != null) {
				return r.SetBlock(x % 512, y, z % 512, block);
			} else {
				return false;
			}
		}

		///<summary>Sets the default bock (normally minecraft:stone) at the given location. This method is faster than SetBlockAt.</summary>
		public void SetDefaultBlock(int x, int y, int z) {
			if(y < 0 || y > 255) return;
			var r = GetRegionAt(x, z, allowNewRegions);
			if(r != null) {
				r.SetDefaultBlock(x % 512, y, z % 512);
			}
		}

		///<summary>Sets the biome at the given location.</summary>
		public void SetBiome(int x, int z, byte biome) {
			var r = GetRegionAt(x, z, false);
			if(r != null) {
				r.SetBiome(x % 512, z % 512, biome);
			}
		}
	}
}

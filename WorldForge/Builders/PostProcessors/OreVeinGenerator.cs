﻿using System;
using System.Xml.Linq;
using WorldForge;
using WorldForge.Coordinates;

namespace WorldForge.Builders.PostProcessors
{
	public class OreVeinGenerator
	{

		public BlockState block;
		public BlockState deepslateBlockVariant;
		public int veinSizeMax = 10;
		public float spawnsPerColumn = 4 / 256f;
		public int heightMin = 1;
		public int heightMax = 32;
		public float heightFalloff = 0f;
		public float heightFalloffCenter = 16;

		private static BlockID stone;
		private static BlockID deepslate;

		public float SpawnsPerChunk
		{
			get => spawnsPerColumn * 256f;
			set => spawnsPerColumn = value / 256f;
		}

		static OreVeinGenerator()
		{
			stone = BlockList.Find("minecraft:stone");
			deepslate = BlockList.Find("minecraft:deepslate");
		}

		public OreVeinGenerator(BlockID block, int veinSize, float spawnsPerChunk, int yMin, int yMax, float falloff, float falloffCenter)
		{
			this.block = BlockState.Simple(block);
			veinSizeMax = veinSize;
			SpawnsPerChunk = spawnsPerChunk;
			heightMin = yMin;
			heightMax = yMax;
			heightFalloff = falloff;
			heightFalloffCenter = falloffCenter;
			deepslateBlockVariant = TryGetDeepslateVariant(this.block);
		}

		public OreVeinGenerator(string block, int veinSize, float spawnsPerChunk, int yMin, int yMax) : this(block, veinSize, spawnsPerChunk, yMin, yMax, 0, (yMin + yMax) / 2f)
		{

		}

		public OreVeinGenerator(XElement elem)
		{
			block = BlockState.Simple(elem.Element("block").Value);
			elem.TryParseInt("size", ref veinSizeMax);
			float rarity = 4;
			elem.TryParseFloat("rarity", ref rarity);
			SpawnsPerChunk = rarity;
			elem.TryParseInt("y-min", ref heightMin);
			elem.TryParseInt("y-max", ref heightMax);
			elem.TryParseFloat("falloff", ref heightFalloff);
			elem.TryParseFloat("center", ref heightFalloffCenter);
		}

		public void Generate(Dimension dim, Random random, float spawnChanceMul, int x, int z)
		{
			if(heightMin >= heightMax) return;
			if(Chance(random, spawnsPerColumn * spawnChanceMul))
			{
				int y = RandomRange(random, heightMin, heightMax);
				if(Chance(random, GetChanceAtY(y)))
				{
					int span = (int)Math.Floor((veinSizeMax - 1) / 16f) + 1;
					for(int i = 0; i < veinSizeMax; i++)
					{
						int x1 = x + RandomRange(random, -span, span);
						int y1 = y + RandomRange(random, -span, span);
						int z1 = z + RandomRange(random, -span, span);
						var pos = new BlockCoord(x1, y1, z1);
						if(dim.GetBlock(pos) == stone)
						{
							dim.SetBlock(pos, block);
						}
						else if(deepslateBlockVariant != null && dim.GetBlock(pos) == deepslate)
						{
							dim.SetBlock(pos, deepslateBlockVariant);
						}
					}
				}
			}
		}

		private float GetChanceAtY(int y)
		{
			if(heightFalloff <= 0) return 1f;

			heightFalloffCenter = Math.Max(heightMin + 0.01f, Math.Min(heightFalloffCenter, heightMax - 0.01f));
			float min = 1f - (y - heightFalloffCenter) / (heightMin - heightFalloffCenter);
			float max = 1f - (y - heightFalloffCenter) / (heightMax - heightFalloffCenter);
			float l = Math.Max(0, Math.Min(min, max));
			return Lerp(1f, l, heightFalloff);
		}

		private float Lerp(float a, float b, float t)
		{
			return a + (b - a) * t;
		}

		private int RandomRange(Random random, int min, int max)
		{
			return random.Next(min, max + 1);
		}

		private bool Chance(Random random, float prob)
		{
			return random.NextDouble() <= prob;
		}

		private BlockState TryGetDeepslateVariant(BlockState regular)
		{
			if(regular.Block.IsVanillaBlock)
			{
				switch(regular.Block.ID.id)
				{
					case "coal_ore":
					case "iron_ore":
					case "gold_ore":
					case "copper_ore":
					case "diamond_ore":
					case "lapis_ore":
					case "redstone_ore":
					case "emerald_ore":
						return new BlockState("deepslate_" + regular.Block.ID.id);
					default: return null;
				}
			}
			return null;
		}
	}
}

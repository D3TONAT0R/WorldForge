using System;
using System.Collections.Generic;
using System.Text;
using WorldForge.NBT;

namespace WorldForge
{
	public partial class LevelData
	{
		public abstract class BiomeSource : INBTConverter
		{
			[NBT("type")]
			public readonly string type;

			public BiomeSource(string type)
			{
				this.type = type;
			}

			public static BiomeSource CreateFromNBT(object nbtData)
			{
				var comp = (NBTCompound)nbtData;
				string type = comp.Get<string>("type");
				if(type.StartsWith("minecraft:")) type = type.Replace("minecraft:", "");
				if(type == "checkerboard") return new CheckerboardBiomeSource(comp);
				else if(type == "fixed") return new FixedBiomeSource(comp.Get<string>("biome"));
				else if(type == "multi_noise") return new MultiNoiseBiomeSource(comp);
				else if(type == "the_end") return new TheEndBiomeSource();
				else if(type == "vanilla_layered") return new VanillaLayeredBiomeSource(comp);
				else throw new NotImplementedException("Unknown BiomeSource type: " + type);
			}

			public virtual void FromNBT(object nbtData)
			{
				NBTConverter.LoadFromNBT((NBTCompound)nbtData, this);
			}

			public virtual object ToNBT(GameVersion version)
			{
				return NBTConverter.WriteToNBT(this, new NBTCompound(), version);
			}
		}

		//Legacy biome source, not used in newer versions (1.16+ ?)
		public class VanillaLayeredBiomeSource : BiomeSource
		{
			[NBT("seed")]
			public long seed;
			[NBT("large_biomes")]
			public bool largeBiomes = false;

			public VanillaLayeredBiomeSource(long seed, bool largeBiomes = false) : base("vanilla_layered")
			{
				this.seed = seed;
				this.largeBiomes = largeBiomes;
			}

			public VanillaLayeredBiomeSource(NBTCompound nbt) : base("vanilla_layered")
			{
				FromNBT(nbt);
			}

			public override void FromNBT(object nbtData)
			{
				base.FromNBT(nbtData);
				var comp = (NBTCompound)nbtData;
				comp.TryGet("seed", out seed);
				comp.TryGet("large_biomes", out largeBiomes);
			}

			public override object ToNBT(GameVersion version)
			{
				var comp = (NBTCompound)base.ToNBT(version);
				comp.Add("seed", seed);
				comp.Add("large_biomes", largeBiomes);
				return comp;
			}
		}

		public class CheckerboardBiomeSource : BiomeSource
		{
			public List<string> biomes = new List<string>();
			public int? scale = null;

			public CheckerboardBiomeSource(string biomeOrTag, int scale = 2) : base("checkerboard")
			{
				biomes.Add(biomeOrTag);
				this.scale = scale;
			}

			public CheckerboardBiomeSource(List<string> biomes, int scale = 2) : base("checkerboard")
			{
				this.biomes = biomes;
				this.scale = scale;
			}

			public CheckerboardBiomeSource(NBTCompound nbt) : base("checkerboard")
			{
				FromNBT(nbt);
			}

			public override void FromNBT(object nbtData)
			{
				base.FromNBT(nbtData);
				var comp = (NBTCompound)nbtData;
				var b = comp.Get("biomes");
				if(b is NBTList nbtList)
				{
					biomes = nbtList.ToList<string>();
				}
				else
				{
					biomes = new List<string>() { (string)b };
				}
				if(comp.TryGet("scale", out int s)) scale = s;
			}

			public override object ToNBT(GameVersion version)
			{
				return base.ToNBT(version);
			}
		}

		public class FixedBiomeSource : BiomeSource
		{
			[NBT("biome")]
			public string biome;

			public FixedBiomeSource(string biome) : base("fixed")
			{
				this.biome = biome;
			}
		}

		public class MultiNoiseBiomeSource : BiomeSource
		{
			public string preset;
			public NBTList customBiomes;
			public long seed;

			public MultiNoiseBiomeSource(string preset) : base("multi_noise")
			{
				this.preset = preset;
			}

			public MultiNoiseBiomeSource(NBTList biomes) : base("multi_noise")
			{
				customBiomes = biomes;
			}

			public MultiNoiseBiomeSource(NBTCompound nbt) : base("multi_noise")
			{
				FromNBT(nbt);
			}

			public override void FromNBT(object nbtData)
			{
				base.FromNBT(nbtData);
				var comp = (NBTCompound)nbtData;
				if(comp.TryGet("preset", out string p)) preset = p;
				else customBiomes = comp.Get<NBTList>("biomes");
				comp.TryGet("seed", out seed);
			}

			public override object ToNBT(GameVersion version)
			{
				var comp = (NBTCompound)base.ToNBT(version);
				if(customBiomes != null) comp.Add("biomes", customBiomes);
				else comp.Add("preset", preset);
				comp.Add("seed", seed);
				return comp;
			}
		}

		public class TheEndBiomeSource : BiomeSource
		{
			public TheEndBiomeSource() : base("the_end")
			{

			}
		}
	}
}

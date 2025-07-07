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
			[NBT("seed", "1.13", "1.19")]
			public long? seed;

			public BiomeSource(string type)
			{
				this.type = type;
			}

			public BiomeSource(string type, NBTCompound nbt)
			{
				this.type = type;
				if(nbt.TryGet("seed", out long s)) seed = s;
			}

			public static BiomeSource CreateFromNBT(object nbtData)
			{
				var comp = (NBTCompound)nbtData;
				string type = comp.Get<string>("type");
				bool isVanilla = !type.Contains(":") || type.StartsWith("minecraft:");
				if(type.StartsWith("minecraft:")) type = type.Replace("minecraft:", "");
				if(isVanilla)
				{
					switch (type)
					{
						case "checkerboard":
							return new CheckerboardBiomeSource(comp);
						case "fixed":
							return new FixedBiomeSource(comp.Get<string>("biome"));
						case "multi_noise":
							return new MultiNoiseBiomeSource(comp);
						case "the_end":
							return new TheEndBiomeSource();
						case "vanilla_layered":
							return new VanillaLayeredBiomeSource(comp);
						default:
							throw new NotImplementedException("Unknown BiomeSource type: " + type);
					}
				}
				else
				{
					return new CustomBiomeSource(type, comp);
				}
			}

			public virtual void FromNBT(object nbtData)
			{
				NBTConverter.LoadFromNBT((NBTCompound)nbtData, this);
			}

			public object ToNBT(GameVersion version)
			{
				NBTCompound comp = new NBTCompound();
				comp.Add("type", type);
				if(seed != null && version >= GameVersion.Release_1(13) && version < GameVersion.Release_1(19))
				{
					comp.Add("seed", seed.Value);
				}
				WriteToNBT(comp, version);
				return comp;
			}

			protected virtual void WriteToNBT(NBTCompound comp, GameVersion version)
			{

			}
		}

		//Legacy biome source, not used in newer versions (1.16+ ?)
		public class VanillaLayeredBiomeSource : BiomeSource
		{
			[NBT("large_biomes")]
			public bool largeBiomes = false;

			public VanillaLayeredBiomeSource(long seed, bool largeBiomes = false) : base("vanilla_layered")
			{
				this.seed = seed;
				this.largeBiomes = largeBiomes;
			}

			public VanillaLayeredBiomeSource(NBTCompound nbt) : base("vanilla_layered", nbt)
			{
				FromNBT(nbt);
			}

			public override void FromNBT(object nbtData)
			{
				base.FromNBT(nbtData);
				var comp = (NBTCompound)nbtData;
				comp.TryGet("large_biomes", out largeBiomes);
			}

			protected override void WriteToNBT(NBTCompound comp, GameVersion version)
			{
				//Write nbt data as if it were a MultiNoiseBiomeSource in versions past 1.16
				if(version >= GameVersion.Release_1(16))
				{
					comp.Set("type", "minecraft:vanilla_layered");
					//TODO: check if this is actually an overworld generator
					var preset = largeBiomes ? MultiNoiseBiomeSource.OVERWORLD_LARGE_BIOMES_PRESET : MultiNoiseBiomeSource.OVERWORLD_PRESET;
					comp.Add("preset", preset);
				}
				else
				{
					comp.Add("large_biomes", largeBiomes);
				}
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

			public CheckerboardBiomeSource(NBTCompound nbt) : base("checkerboard", nbt)
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

			protected override void WriteToNBT(NBTCompound comp, GameVersion version)
			{
				comp.Add("biomes", new NBTList(NBTTag.TAG_String, biomes));
				if(scale != null) comp.Add("scale", scale.Value);
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

			protected override void WriteToNBT(NBTCompound comp, GameVersion version)
			{
				comp.Add("biome", biome);
			}
		}

		public class MultiNoiseBiomeSource : BiomeSource
		{
			public enum PresetType
			{
				Overworld,
				Nether,
				End,
				OverworldLargeBiomes
			}

			public const string OVERWORLD_PRESET = "minecraft:overworld";
			public const string NETHER_PRESET = "minecraft:nether";
			public const string END_PRESET = "minecraft:end";
			public const string OVERWORLD_LARGE_BIOMES_PRESET = "minecraft:large_biomes";

			public string preset;
			public NBTList customBiomes;

			public bool LargeBiomes
			{
				get => preset == OVERWORLD_LARGE_BIOMES_PRESET;
			}

			public MultiNoiseBiomeSource(string preset) : base("multi_noise")
			{
				this.preset = preset;
			}

			public MultiNoiseBiomeSource(PresetType presetType) : base("multi_noise")
			{
				switch(presetType)
				{
					case PresetType.Overworld: preset = OVERWORLD_PRESET; break;
					case PresetType.Nether: preset = NETHER_PRESET; break;
					case PresetType.End: preset = END_PRESET; break;
					case PresetType.OverworldLargeBiomes: preset = OVERWORLD_LARGE_BIOMES_PRESET; break;
				}
			}

			public MultiNoiseBiomeSource(NBTList biomes) : base("multi_noise")
			{
				customBiomes = biomes;
			}

			public MultiNoiseBiomeSource(NBTCompound nbt) : base("multi_noise", nbt)
			{
				FromNBT(nbt);
			}

			public override void FromNBT(object nbtData)
			{
				base.FromNBT(nbtData);
				var comp = (NBTCompound)nbtData;
				if(comp.TryGet("preset", out string p)) preset = p;
				else customBiomes = comp.Get<NBTList>("biomes");
			}

			protected override void WriteToNBT(NBTCompound comp, GameVersion version)
			{
				if(customBiomes != null) comp.Add("biomes", customBiomes);
				else comp.Add("preset", preset);
			}
		}

		public class TheEndBiomeSource : BiomeSource
		{
			public TheEndBiomeSource() : base("the_end")
			{

			}
		}

		public class CustomBiomeSource : BiomeSource
		{
			public NBTCompound data;

			public CustomBiomeSource(string type, NBTCompound data) : base(type, data)
			{
				this.data = data;
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Text;
using WorldForge.Biomes;
using WorldForge.NBT;

namespace WorldForge
{
	public partial class LevelData
	{

		public struct SuperflatLayer : INBTConverter
		{
			[NBT("block")]
			public string block;
			[NBT("height")]
			public int height;

			public SuperflatLayer(string block, int height)
			{
				this.block = block;
				this.height = height;
			}

			public SuperflatLayer(NBTCompound nbt)
			{
				block = nbt.Get<string>("block");
				height = nbt.Get<int>("height");
			}

			public void FromNBT(object nbtData)
			{
				NBTConverter.LoadFromNBT((NBTCompound)nbtData, this);
			}

			public object ToNBT(GameVersion version)
			{
				return NBTConverter.WriteToNBT(this, new NBTCompound(), version);
			}
		}

		public abstract class DimensionGeneratorBase : INBTConverter
		{
			public DimensionID dimensionType;

			public abstract string Type { get; }

			public abstract string LegacyGeneratorTypeName { get; }

			public DimensionGeneratorBase(DimensionID dimensionType)
			{
				this.dimensionType = dimensionType;
			}

			public static DimensionGeneratorBase CreateFromNBT(DimensionID dim, NBTCompound comp)
			{
				var genComp = comp.Get<NBTCompound>("generator");
				string type = genComp.Get<string>("type");
				DimensionGeneratorBase gen;
				if(type == "minecraft:noise") gen = new DimensionGenerator(dim);
				else if(type == "minecraft:flat") gen = new SuperflatDimensionGenerator(dim);
				else if(type == "minecraft:debug") gen = new DebugDimensionGenerator(dim);
				else throw new NotImplementedException("Unknown dimension generator type: " + type);
				gen.FromNBT(genComp);
				return gen;
			}

			public virtual void FromNBT(object nbtData)
			{
				var comp = (NBTCompound)nbtData;
				dimensionType = new DimensionID(comp.Get<string>("type"));
				var generator = comp.Get<NBTCompound>("generator");
				ReadSettings(generator);
			}

			public virtual object ToNBT(GameVersion version)
			{
				var comp = new NBTCompound
				{
					{ "type", dimensionType.ID }
				};
				var generator = comp.AddCompound("generator");
				generator.Add("type", Type);
				WriteSettings(generator, version);
				return comp;
			}

			protected abstract void ReadSettings(NBTCompound nbt);

			protected abstract void WriteSettings(NBTCompound nbt, GameVersion version);

		}

		//TODO: custom per dimension seeds
		public class DimensionGenerator : DimensionGeneratorBase
		{
			//[NBT("biome_source")]
			public BiomeSource biomeSource;
			//[NBT("settings")]
			public string settingsPreset;
			public NBTCompound customSettings;

			public override string Type => "minecraft:noise";

			public override string LegacyGeneratorTypeName
			{
				get
				{
					bool largeBiomes = false;
					if(biomeSource is MultiNoiseBiomeSource m) largeBiomes = m.LargeBiomes;
					else if(biomeSource is VanillaLayeredBiomeSource v) largeBiomes = v.largeBiomes;
					return largeBiomes ? "largeBiomes" : "default";
				}
			}

			public static DimensionGenerator CreateDefaultOverworldGenerator(long? customSeed = null)
			{
				var gen = new DimensionGenerator(DimensionID.Overworld);
				gen.biomeSource = new MultiNoiseBiomeSource("minecraft:overworld");
				return gen;
			}

			public static DimensionGenerator CreateDefaultNetherGenerator(long? customSeed = null)
			{
				var gen = new DimensionGenerator(DimensionID.Nether);
				gen.biomeSource = new MultiNoiseBiomeSource("minecraft:nether");
				return gen;
			}

			public static DimensionGenerator CreateDefaultEndGenerator(long? customSeed = null)
			{
				var gen = new DimensionGenerator(DimensionID.TheEnd);
				gen.biomeSource = new TheEndBiomeSource();
				return gen;
			}

			public DimensionGenerator(DimensionID dim) : base(dim)
			{

			}

			protected override void ReadSettings(NBTCompound nbt)
			{
				if(nbt.TryGet<NBTCompound>("biome_source", out var biomeSourceNBT))
				{
					biomeSource = BiomeSource.CreateFromNBT(biomeSourceNBT);
				}
				var settingsNBT = nbt.Get("settings");
				if(settingsNBT is NBTCompound nbtComp)
				{
					customSettings = nbtComp;
				}
				else
				{
					settingsPreset = (string)settingsNBT;
				}
			}

			protected override void WriteSettings(NBTCompound nbt, GameVersion version)
			{
				if(biomeSource != null)
				{
					nbt.Add("biome_source", biomeSource.ToNBT(version));
				}
				if(customSettings != null)
				{
					nbt.Add("settings", customSettings);
				}
				else if(settingsPreset != null)
				{
					nbt.Add("settings", settingsPreset);
				}
			}
		}

		public class SuperflatDimensionGenerator : DimensionGeneratorBase
		{
			//[NBT("layers")]
			public List<SuperflatLayer> layers = new List<SuperflatLayer>();

			[NBT("structure_overrides")]
			public List<string> structureOverrides = new List<string>();

			//[NBT("biome")]
			public BiomeID biome;
			[NBT("features")]
			public bool features = false;
			[NBT("lakes")]
			public bool lakes = false;

			public SuperflatDimensionGenerator(DimensionID dim, params SuperflatLayer[] layers) : base(dim)
			{
				this.layers.AddRange(layers);
			}

			public static SuperflatDimensionGenerator CreateSuperflatOverworldGenerator(BiomeID biome, params SuperflatLayer[] layers)
			{
				var gen = new SuperflatDimensionGenerator(DimensionID.Overworld, layers);
				gen.biome = biome;
				return gen;
			}

			public override string Type => "minecraft:flat";

			public override string LegacyGeneratorTypeName => "flat";

			protected override void ReadSettings(NBTCompound nbt)
			{
				var settings = nbt.GetAsCompound("settings");
				layers = new List<SuperflatLayer>();
				if(settings.TryGet<NBTList>("layers", out var list))
				{
					foreach(var layer in list)
					{
						layers.Add(new SuperflatLayer((NBTCompound)layer));
					}
				}
				structureOverrides = settings.Get<List<string>>("structure_overrides");
				biome = BiomeIDResolver.ParseBiome(nbt.Get<string>("biome"));
			}

			protected override void WriteSettings(NBTCompound nbt, GameVersion version)
			{
				var settings = nbt.AddCompound("settings");
				var list = new NBTList(NBTTag.TAG_Compound);
				foreach(var layer in layers)
				{
					list.Add(layer.ToNBT(version));
				}
				settings.Add("layers", list);
				settings.Add("structure_overrides", structureOverrides);
				settings.Add("biome", BiomeIDResolver.GetIDForVersion(biome, version));
				settings.Add("features", features);
				settings.Add("lakes", lakes);
			}
		}

		public class DebugDimensionGenerator : DimensionGeneratorBase
		{
			public DebugDimensionGenerator(DimensionID dim) : base(dim)
			{

			}

			public override string Type => "minecraft:debug";

			public override string LegacyGeneratorTypeName => "debug_all_block_states";

			protected override void ReadSettings(NBTCompound nbt)
			{
				//no settings
			}

			protected override void WriteSettings(NBTCompound nbt, GameVersion version)
			{
				//no settings
			}
		}

		public class WorldGenerator
		{
			public const string overworldID = "minecraft:overworld";
			public const string theNetherID = "minecraft:the_nether";
			public const string theEndID = "minecraft:the_end";

			//[NBT("MapFeatures")]
			public bool mapFeatures = true;

			[NBT("bonus_chest")]
			public bool generateBonusChest = false;
			[NBT("generate_features")]
			public bool generateFeatures = true;
			[NBT("seed")]
			private long seed;
			//[NBT("dimensions")]
			public Dictionary<string, DimensionGeneratorBase> dimensionGenerators;

			public DimensionGeneratorBase OverworldGenerator
			{
				get
				{
					if(dimensionGenerators.TryGetValue(overworldID, out var g)) return g;
					else return null;
				}
				set
				{
					if(dimensionGenerators.ContainsKey(overworldID)) dimensionGenerators[overworldID] = value;
					else dimensionGenerators.Add(overworldID, value);
				}
			}

			public DimensionGeneratorBase NetherGenerator
			{
				get
				{
					if(dimensionGenerators.TryGetValue(theNetherID, out var g)) return g;
					else return null;
				}
				set
				{
					if(dimensionGenerators.ContainsKey(theNetherID)) dimensionGenerators[theNetherID] = value;
					else dimensionGenerators.Add(theNetherID, value);
				}
			}

			public DimensionGeneratorBase EndGenerator
			{
				get
				{
					if(dimensionGenerators.TryGetValue(theEndID, out var g)) return g;
					else return null;
				}
				set
				{
					if(dimensionGenerators.ContainsKey(theEndID)) dimensionGenerators[theEndID] = value;
					else dimensionGenerators.Add(theEndID, value);
				}
			}

			public long WorldSeed => seed;

			public WorldGenerator(long seed)
			{
				this.seed = seed;
				dimensionGenerators = new Dictionary<string, DimensionGeneratorBase>();
				dimensionGenerators.Add(overworldID, DimensionGenerator.CreateDefaultOverworldGenerator());
				dimensionGenerators.Add(theNetherID, DimensionGenerator.CreateDefaultNetherGenerator());
				dimensionGenerators.Add(theEndID, DimensionGenerator.CreateDefaultEndGenerator());
			}

			public WorldGenerator() : this(new Random().Next(int.MinValue, int.MaxValue))
			{

			}

			public WorldGenerator(NBTCompound nbt)
			{
				FromNBT(nbt);
			}

			public void SetWorldSeed(long newSeed)
			{
				seed = newSeed;
			}

			public void WriteToNBT(NBTCompound nbt, GameVersion version)
			{
				//TODO: WorldGenSettings added in 1.13?
				nbt.Add("RandomSeed", WorldSeed);
				nbt.Add("MapFeatures", mapFeatures);

				if(version >= GameVersion.Release_1(15))
				{
					var worldGenComp = nbt.AddCompound("WorldGenSettings");
					NBTConverter.WriteToNBT(this, worldGenComp, version);
					var dim = worldGenComp.AddCompound("dimensions");
					foreach(var kv in dimensionGenerators)
					{
						dim.Add(kv.Key, kv.Value.ToNBT(version));
					}
				}
				else
				{
					var overworldGen = OverworldGenerator;
					//TODO: find out when customizable generators were added
					if(overworldGen != null && version >= GameVersion.Release_1(0))
					{
						nbt.Add("generatorName", overworldGen.LegacyGeneratorTypeName);
						//TODO: custom generator options
						nbt.Add("generatorOptions", "");
						//nbt.Add("generatorOptions", overworldGen.ToNBT(version));
						nbt.Add("generatorVersion", 1);
					}
				}
			}

			public void FromNBT(object nbtData)
			{
				var nbt = (NBTCompound)nbtData;
				NBTConverter.LoadFromNBT(nbt, this);
				dimensionGenerators = new Dictionary<string, DimensionGeneratorBase>();
				if(nbt.TryGet("dimensions", out NBTCompound dimensions))
				{
					foreach(var kv in dimensions)
					{
						var dim = new DimensionID(kv.Key);
						var dimNBT = (NBTCompound)kv.Value;
						dimensionGenerators.Add(kv.Key, DimensionGeneratorBase.CreateFromNBT(dim, dimNBT));
					}
				}
			}
		}
	}
}

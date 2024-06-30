using System;
using System.Collections.Generic;
using System.Text;
using WorldForge.NBT;

namespace WorldForge
{
	public class BlockState : INBTConverter
	{

		public static BlockState Air
		{
			get
			{
				if(air == null) air = new BlockState(BlockList.Find("air"));
				return air;
			}
		}
		private static BlockState air;

		public static BlockState Unknown
		{
			get
			{
				if(unknown == null) unknown = new BlockState();
				return unknown;
			}
		}
		private static BlockState unknown;

		public BlockID block;
		private NBTCompound properties;

		private BlockState() { }

		public BlockState(BlockID blockType)
		{
			if(blockType == null)
			{
				throw new NullReferenceException("Attempted to create a BlockState with a null ProtoBlock.");
			}
			block = blockType;
			AddDefaultBlockProperties();
		}

		public BlockState(string blockType, params (string, string)[] properties) : this(BlockList.Find(blockType))
		{
			foreach(var prop in properties)
			{
				SetProperty(prop.Item1, prop.Item2);
			}
		}

		public BlockState(NBTCompound paletteNBT) : this(BlockList.Find(paletteNBT.Get<string>("Name")))
		{
			paletteNBT.TryGet("Properties", out properties);
		}

		public BlockState(BlockState original)
		{
			block = original.block;
			properties = original.properties?.Clone();
		}

		public bool HasProperty(string key)
		{
			if(properties == null) return false;
			return properties.Contains(key);
		}

		public string GetProperty(string key)
		{
			if(properties == null) return null;
			if(properties.TryGet(key, out string v))
			{
				return v;
			}
			else return null;
		}

		public bool TryGetProperty(string key, out string value)
		{
			value = GetProperty(key);
			return value != null;
		}

		public void SetProperty(string key, string value)
		{
			if(properties == null) properties = new NBTCompound();
			properties.Set(key, value);
		}

		public void SetProperty(string key, int value)
		{
			SetProperty(key, value.ToString());
		}

		public void SetProperty(string key, bool value)
		{
			SetProperty(key, value.ToString());
		}

		public bool RemoveProperty(string key)
		{
			if(properties == null) return false;
			return properties.Remove(key);
		}

		void AddDefaultBlockProperties()
		{
			if(block == null) return;
			switch(block.shortID)
			{
				case "oak_leaves":
				case "spruce_leaves":
				case "birch_leaves":
				case "jungle_leaves":
				case "acacia_leaves":
				case "dark_oak_leaves":
					//TODO: proper distance
					SetProperty("distance", 1);
					break;
			}
		}

		public bool Compare(BlockState other, bool compareProperties = true)
		{
			if(compareProperties)
			{
				if(!NBTCompound.AreEqual(properties, other.properties)) return false;
			}
			return block == other.block;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder(block?.ID);
			if(properties.contents.Count > 0)
			{
				sb.Append("[");
				bool first = true;
				foreach(var prop in properties.contents)
				{
					if(!first) sb.Append(",");
					first = false;
					sb.Append($"{prop.Key}={prop.Value}");
				}
				sb.Append("]");
			}
			return sb.ToString();
		}

		public object ToNBT(GameVersion version)
		{
			var nbt = new NBTCompound();
			nbt.Add("Name", block.ID);
			if(!NBTCompound.IsNullOrEmpty(properties))
			{
				nbt.Add("Properties", properties);
			}
			return nbt;
		}

		public void FromNBT(object nbtData)
		{
			var comp = (NBTCompound)nbtData;
			block = BlockList.Find(comp.Get<string>("Name"));
			comp.TryGet("Properties", out properties);
		}

		public static void ResolveBlockState(GameVersion version, ref BlockState state)
		{
			if(state == null) return;
			if (state.block == null)
			{
				state = Air;
			}
			else
			{
				if(version < state.block.AddedInVersion)
				{
					if(state.block.substitute != null)
					{
						state = new BlockState(state);
						state.block = (BlockID)state.block.substitute;
						ResolveBlockState(version, ref state);
					}
					else
					{
						state = Air;
					}
				}
			}
		}

		public NumericID ToNumericID(GameVersion version)
		{
			if(block == null) return new NumericID(0, 0);
			if(block.customNamespace == null)
			{
				if(block.shortID.EndsWith("_stairs"))
				{
					return new NumericID(block.numericID.Value.id, GetFacingMetaStairs(this));
				}
				switch(block.shortID)
				{
					case "torch":
					case "wall_torch":
						return new NumericID(block.numericID.Value.id, GetFacingMetaWallTorch(this));
					case "redstone_torch":
					case "redstone_wall_torch":
						short id = (short)(GetProperty("lit") == "true" ? 76 : 75);
						return new NumericID(id, GetFacingMetaWallTorch(this));
					case "furnace":
						id = (short)(GetProperty("lit") == "true" ? 62 : 61);
						return new NumericID(id, GetFacingMetaFurnace(this));
					case "chest":
						short meta = version >= GameVersion.Beta_1(8) ? GetFacingMetaFurnace(this) : (short)0;
						return new NumericID(54, meta);
				}
			}
            return block.numericID ?? NumericID.Air;
        }

		public static BlockState FromNumericID(NumericID numericID)
		{

			//Facing metadata:

			//Furnace
			//north -> 5	2
			//south -> 4	3
			//west -> 3		4
			//east -> 2		5

			//Torch
			//north -> 1	4
			//south -> 2	3
			//west -> 3		2
			//east -> 4		1

			//Chest
			//north -> 2
			//east -> 5
			//south -> 3
			//west -> 4

			//standing -> 5

			var id = numericID.id;
			var meta = numericID.damage;
			BlockState state;
			//Special cases
			switch(id)
			{
				case 50: //Torch
					return CreateTorchState("torch", meta);
				case 75: //Unlit Redstone Torch
				case 76: //Lit Redstone Torch
					state = CreateTorchState("redstone_torch", meta);
					state.SetProperty("lit", id == 76);
					return state;
				case 61: //Furnace
				case 62: //Lit Furnace
					state = new BlockState(BlockList.Find("furnace"));
					state.SetProperty("lit", id == 62);
					SetFacingPropertyFurnace(state, meta);
					return state;
				case 54: //Chest
					state = new BlockState(BlockList.Find("chest"));
					SetFacingPropertyFurnace(state, meta);
					return state;
				default:
					return new BlockState(BlockList.FindByNumeric(numericID));
			}
		}

		private static BlockState CreateTorchState(string name, int meta)
		{
			if(meta > 0 && meta <= 4)
			{
				//Wall torch
				var state = new BlockState(BlockList.Find("wall_"+name));
				SetFacingPropertyWallTorch(state, meta);
				return state;
			}
			else
			{
				//standing torch
				return new BlockState(BlockList.Find(name));
			}
		}

		private static void SetFacingPropertyFurnace(BlockState state, int meta)
		{
			switch(meta)
			{
				case 2: state.SetProperty("facing", "north"); break;
				case 3: state.SetProperty("facing", "south"); break;
				case 4: state.SetProperty("facing", "west"); break;
				case 5: state.SetProperty("facing", "east"); break;
			}
		}

		private static byte GetFacingMetaFurnace(BlockState state)
		{
			switch(state.GetProperty("facing"))
			{
				case "north": return 2;
				case "south": return 3;
				case "west": return 4;
				case "east": return 5;
				default: return 0;
			}
		}

		private static void SetFacingPropertyStairs(BlockState state, int meta)
		{
			switch(meta)
			{
				case 0: state.SetProperty("facing", "east"); break;
				case 1: state.SetProperty("facing", "west"); break;
				case 2: state.SetProperty("facing", "south"); break;
				case 3: state.SetProperty("facing", "north"); break;
			}
		}

		private static byte GetFacingMetaStairs(BlockState state)
		{
			switch(state.GetProperty("facing"))
			{
				case "east": return 0;
				case "west": return 1;
				case "south": return 2;
				case "north": return 3;
				default: return 0;
			}
		}

		private static void SetFacingPropertyWallTorch(BlockState state, int meta)
		{
			switch(meta)
			{
				case 1: state.SetProperty("facing", "east"); break;
				case 2: state.SetProperty("facing", "west"); break;
				case 3: state.SetProperty("facing", "south"); break;
				case 4: state.SetProperty("facing", "north"); break;
			}
		}

		private static byte GetFacingMetaWallTorch(BlockState state)
		{
			switch(state.GetProperty("facing"))
			{
				case "east": return 1;
				case "west": return 2;
				case "south": return 3;
				case "north": return 4;
				default: return 0;
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Text;
using WorldForge.NBT;

namespace WorldForge
{
	public struct BlockState : INBTConverter
	{

		public static BlockState Air
		{
			get
			{
				if(air == null) air = new BlockState(BlockList.Find("air"));
				return air.Value;
			}
		}
		private static BlockState? air;

		public static BlockState Unknown
		{
			get
			{
				if(unknown == null) unknown = new BlockState();
				return unknown.Value;
			}
		}
		private static BlockState? unknown;

		public BlockID Block { get; private set; }

		private NBTCompound properties;

		public int Hash { get; private set; }

		//public BlockState() { }

		public BlockState(BlockID block)
		{
			if(block == null)
			{
				throw new NullReferenceException("Attempted to create a BlockState with a null BlockID.");
			}
			Block = block;
			properties = null;
			Hash = 0;
			AddDefaultBlockProperties();
			InitializeHash();
		}

		public BlockState(string id, params (string, string)[] properties) : this(BlockList.Find(id))
		{
			foreach(var prop in properties)
			{
				SetProperty(prop.Item1, prop.Item2);
			}
			InitializeHash();
		}

		public BlockState(NBTCompound paletteNBT) : this(BlockList.Find(paletteNBT.Get<string>("Name")))
		{
			paletteNBT.TryGet("Properties", out properties);
			InitializeHash();
		}

		public BlockState(BlockState original) : this(original.Block)
		{
			properties = original.properties?.Clone();
			InitializeHash();
		}

		public BlockState(BlockID block, NBTCompound properties) : this(block)
		{
			this.properties = properties.Clone();
			InitializeHash();
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
			RecalculateHash();
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
			if(properties.Remove(key))
			{
				RecalculateHash();
				return true;
			}
			return false;
		}

		void AddDefaultBlockProperties()
		{
			if(Block == null) return;
			switch(Block.ID.id)
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
			if(compareProperties) return Hash == other.Hash;
			return Block == other.Block;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder(Block?.ID.FullID);
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
			nbt.Add("Name", Block.ID.FullID);
			if(!NBTCompound.IsNullOrEmpty(properties))
			{
				nbt.Add("Properties", properties);
			}
			return nbt;
		}

		public void FromNBT(object nbtData)
		{
			var comp = (NBTCompound)nbtData;
			Block = BlockList.Find(comp.Get<string>("Name"));
			comp.TryGet("Properties", out properties);
			RecalculateHash();
		}

		public static void ResolveBlockState(GameVersion version, ref BlockState state)
		{
			ResolveBlockState(version, ref state, 1);
		}

		private static void ResolveBlockState(GameVersion version, ref BlockState state, int iteration)
		{
			if(iteration > 10)
			{
				throw new Exception($"BlockState resolution loop detected. Block: {state}");
			}
			if(state.Block == null)
			{
				state = Air;
			}
			else
			{
				if(version < state.Block.AddedInVersion)
				{
					if(state.Block.substitute != null)
					{
						state = new BlockState(state);
						state = new BlockState((BlockID)state.Block.substitute, state.properties);
						ResolveBlockState(version, ref state, iteration + 1);
					}
					else
					{
						state = Air;
					}
				}
			}
			if(BlockList.TryGetPreviousID(state.Block, version, out var newID))
			{
				state = new BlockState(newID, state.properties);
			}
		}

		public NumericID? ToNumericID(GameVersion version)
		{
			if(Block == null) return new NumericID(0, 0);
			if(!Block.ID.HasCustomNamespace)
			{
				var id = Block.ID.id;
				if(id.EndsWith("_stairs"))
				{
					if(Block.numericID.HasValue)
					{
						return new NumericID(Block.numericID.Value.id, GetFacingMetaStairs(this));
					}
					return null;
				}
				else if(id.EndsWith("_wall_sign"))
				{
					byte meta = 0;
					if(TryGetProperty("facing", out var f))
					{
						switch(f)
						{
							case "north": meta = 2; break;
							case "south": meta = 3; break;
							case "west": meta = 4; break;
							case "east": meta = 5; break;
						}
					}
					return new NumericID(Block.numericID.Value.id, meta);
				}
				else if(id.EndsWith("_sign"))
				{
					byte meta = 0;
					if(TryGetProperty("rotation", out var r)) meta = byte.Parse(r);
					if(Block.numericID.HasValue)
					{
						return new NumericID(Block.numericID.Value.id, meta);
					}
					return null;
				}
				else if(id.EndsWith("_slab"))
				{
					bool doubleSlab = GetProperty("double") == "true";
					if(!Block.numericID.HasValue) return null;
					var numId = Block.numericID.Value;
					if(doubleSlab)
					{
						if(numId.id == 44) numId.id = 43;
					}
					return numId;
                }
				switch(id)
				{
					case "torch":
					case "wall_torch":
						if(!Block.numericID.HasValue) return null;
						return new NumericID(Block.numericID.Value.id, GetFacingMetaWallTorch(this));
					case "redstone_torch":
					case "redstone_wall_torch":
						short blockId = (short)(GetProperty("lit") == "true" ? 76 : 75);
						return new NumericID(blockId, GetFacingMetaWallTorch(this));
					case "furnace":
						blockId = (short)(GetProperty("lit") == "true" ? 62 : 61);
						return new NumericID(blockId, GetFacingMetaFurnace(this));
					case "chest":
						short meta = version >= GameVersion.Beta_1(8) ? GetFacingMetaFurnace(this) : (short)0;
						return new NumericID(54, meta);
				}
			}
            return Block.numericID;
        }

		public static BlockState? FromNumericID(NumericID numericID)
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
					var b = BlockList.FindByNumeric(numericID);
					if(b != null) return new BlockState(b);
					else return null;
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

		private void InitializeHash()
		{
			if(Hash == default) RecalculateHash();
		}

		private void RecalculateHash()
		{
			int h = Block.GetHashCode();
			if(properties != null)
			{
				foreach(var property in properties)
				{
					h += 17 * property.Key.GetHashCode();
					h += 31 * property.Value.GetHashCode();
				}
			}
			Hash = h;
		}

		public override int GetHashCode() => Hash;
	}
}

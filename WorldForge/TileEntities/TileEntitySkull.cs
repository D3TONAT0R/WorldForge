using System;
using System.Collections.Generic;
using WorldForge.Coordinates;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public class TileEntitySkull : TileEntity
	{
		public class Profile : INBTConverter
		{
			public bool NameOnly { get; set; }

			public string name = "";
			public UUID id;
			public List<NBTCompound> properties;

			public Profile()
			{

			}

			public object ToNBT(GameVersion version)
			{
				if(NameOnly) return name;
				else
				{
					NBTCompound nbt = new NBTCompound();
					nbt.Add("name", name);
					nbt.Add("id", id.ToNBT(version));
					if(properties != null) nbt.Add("properties", properties);
					return nbt;
				}
			}

			public void FromNBT(object nbtData)
			{
				if(nbtData is string s)
				{
					NameOnly = true;
					name = s;
				}
				else if(nbtData is NBTCompound nbt)
				{
					NameOnly = false;
					nbt.TryGet("name", out name);
					nbt.TryGet("id", out id);
					nbt.TryGet("properties", out properties);
				}
			}
		}

		[NBT("custom_name")]
		public string customName = null;
		[NBT("note_block_sound")]
		public string noteBlockSound = null;
		[NBT("profile")]
		public Profile profile = null;

		public TileEntitySkull() : base("skull")
		{

		}

		public TileEntitySkull(NBTCompound compound, out BlockCoord blockPos) : base(compound, out blockPos)
		{

		}
	}
}

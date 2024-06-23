using System;
using System.Collections.Generic;
using WorldForge.Coordinates;
using WorldForge.Entities;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public class TileEntityBeehive : TileEntity
	{
		public class HiveBee : INBTConverter
		{
			public MobBee entityData = null;
			[NBT("ticks_in_hive")]
			public int ticksInHive = 0;
			[NBT("min_ticks_in_hive")]
			public int minTicksInHive = 0;

			public void FromNBT(object nbtData)
			{
				var comp = (NBTCompound)nbtData;
				if(comp.Contains("entity_data"))
				{
					entityData = new MobBee(comp.GetAsCompound("entity_data"));
				}
				NBTConverter.LoadFromNBT(comp, this);
			}

			public object ToNBT(GameVersion version)
			{
				var comp = new NBTCompound();
				if(entityData != null)
				{
					comp.Add("entity_data", entityData.ToNBT(version));
				}
				return NBTConverter.WriteToNBT(this, comp, version);
			}
		}

		[NBT("bees")]
		public List<HiveBee> bees = new List<HiveBee>();
		[NBT("flower_pos")]
		public BlockCoord flowerPos = default;

		public TileEntityBeehive(string id) : base(id)
		{
		}

		public TileEntityBeehive(NBTCompound compound, out BlockCoord blockPos) : base(compound, out blockPos)
		{
		}

		protected override void OnWriteToNBT(NBTCompound nbt, GameVersion version)
		{
			
		}
	}
}

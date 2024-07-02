using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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

		//[NBT("bees")]
		public List<HiveBee> bees = new List<HiveBee>();
		[NBT("flower_pos")]
		public BlockCoord flowerPos = default;

		public TileEntityBeehive(string id) : base(id)
		{
		}

		public TileEntityBeehive(NBTCompound compound, out BlockCoord blockPos) : base(compound, out blockPos)
		{
			if(compound.TryGet<NBTList>("bees", out var list))
			{
				bees = new List<HiveBee>();
				foreach(var bee in list)
				{
					var hiveBee = new HiveBee();
					hiveBee.FromNBT(bee);
					bees.Add(hiveBee);
				}
			}
		}

		protected override void OnWriteToNBT(NBTCompound nbt, GameVersion version)
		{
			if(bees != null)
			{
				var list = new NBTList(NBTTag.TAG_Compound);
				foreach(var bee in bees)
				{
					list.Add(bee.ToNBT(version));
				}
				nbt.Add("bees", list);
			}
		}
	}
}

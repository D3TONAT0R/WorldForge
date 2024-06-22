using System.Collections.Generic;
using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobWarden : Mob
	{
		public class AngerData : INBTConverter
		{
			public class Suspect : INBTConverter
			{
				public UUID entity;
				public int angerLevel = 0;

				public object ToNBT(GameVersion version)
				{
					var comp = new NBTCompound();
					if(entity != null)
					{
						comp.Add("uuid", entity.ToNBT(version));
					}
					comp.Add("anger", angerLevel);
					return comp;
				}

				public void FromNBT(object nbtData)
				{
					var compound = (NBTCompound)nbtData;
					entity = new UUID(compound.Get<int[]>("uuid"));
					angerLevel = compound.Get<int>("anger");
				}
			}

			public List<Suspect> suspects = new List<Suspect>();

			public object ToNBT(GameVersion version)
			{
				var suspectsList = new NBTList(NBTTag.TAG_Compound);
				foreach(var suspect in suspects)
				{
					suspectsList.Add(suspect.ToNBT(version));
				}
				return new NBTCompound()
				{
					{ "suspects", suspectsList }
				};
			}

			public void FromNBT(object nbtData)
			{
				var comp = (NBTCompound)nbtData;
				suspects.Clear();
				var list = comp.GetAsList("suspects");
				foreach(var entry in list)
				{
					var suspect = new Suspect();
					suspect.FromNBT(entry);
					suspects.Add(suspect);
				}
			}
		}

		[NBT("anger")]
		public AngerData angerData = null;

		public MobWarden(NBTCompound compound) : base(compound)
		{

		}

		public MobWarden(Vector3 position) : base("minecraft:warden", position)
		{

		}
	}
}

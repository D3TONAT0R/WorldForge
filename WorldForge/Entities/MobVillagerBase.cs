using System.Collections.Generic;
using WorldForge.NBT;

namespace WorldForge.Entities
{
	public abstract class MobVillagerBase : MobBreedable
	{
		public enum VillagerLevel
		{
			Novice = 1,
			Apprentice = 2,
			Journeyman = 3,
			Expert = 4,
			Master = 5
		}

		public class VillagerData : INBTConverter
		{
			[NBT("level")]
			public VillagerLevel level = VillagerLevel.Novice;
			[NBT("profession")]
			public string profession = "minecraft:none";
			[NBT("type")]
			public string type = "minecraft:plains";

			public void FromNBT(object nbtData)
			{
				NBTConverter.LoadFromNBT((NBTCompound)nbtData, this);
			}

			public object ToNBT(GameVersion version)
			{
				return NBTConverter.WriteToNBT(this, new NBTCompound(), version);
			}
		}

		[NBT("Gossips")]
		public List<NBTCompound> gossips = new List<NBTCompound>();
		[NBT("Offers")]
		public NBTCompound offers;
		[NBT("VillagerData")]
		public VillagerData villagerData = new VillagerData();
		[NBT("Xp")]
		public int xp = 0;

		public MobVillagerBase(NBTCompound compound) : base(compound)
		{

		}

		public MobVillagerBase(string id, Vector3 position) : base(id, position)
		{

		}
	}
}

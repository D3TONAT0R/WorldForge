using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public class LootTableOptions : INBTCollection
	{
		public string lootTable = null;
		public long? lootTableSeed = 0;

		public void LoadFromNBT(NBTCompound nbt, bool remove)
		{
			if(!remove)
			{
				nbt.TryGet("LootTable", out lootTable);
				if(nbt.TryGet("LootTableSeed", out long s)) lootTableSeed = s;
			}
			else
			{
				nbt.TryTake("LootTable", out lootTable);
				if(nbt.TryTake("LootTableSeed", out long s)) lootTableSeed = s;
			}
		}

		public void WriteToNBT(NBTCompound nbt, GameVersion version)
		{
			if(!string.IsNullOrEmpty(lootTable))
			{
				nbt.Add("LootTable", lootTable);
				if(lootTableSeed.HasValue) nbt.Add("LootTableSeed", lootTableSeed.Value);
			}
		}
	}
}

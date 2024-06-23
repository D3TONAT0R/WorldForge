using WorldForge.NBT;

namespace WorldForge.Items
{
	public class Item
	{
		[NBT]
		public string id;
		[NBT]
		public NBTCompound tag;

		public Item(string id, NBTCompound tag)
		{
			if (!id.Contains(":")) id = "minecraft:" + id;
			this.id = id;
		}

		public Item(string id) : this(id, null)
		{

		}

		public Item(NBTCompound nbt)
		{
			var itemId = nbt.Get("id");
			if(itemId is string s) id = s;
			else if(itemId is int i)
			{
				id = "minecraft:air"; //TODO: Get item by id
			}
			nbt.TryGet<NBTCompound>("tag", out tag);
		}

		public void WriteToNBT(NBTCompound nbt, GameVersion version)
		{
			//TODO
			NBTConverter.WriteToNBT(this, nbt, version);
		}
	}
}

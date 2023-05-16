using MCUtils.NBT;

namespace MCUtils
{
	public class Item
	{
		[NBT]
		public string id;
		[NBT]
		public NBTCompound tag;

		public Item(string id, NBTCompound tag)
		{
			if(!id.Contains(":")) id = "minecraft:" + id;
			this.id = id;
		}

		public Item(string id) : this(id, null)
		{

		}

		public Item(NBTCompound nbt)
		{
			NBTConverter.LoadFromNBT(nbt, this);
		}

		public void WriteToNBT(NBTCompound nbt, Version version)
		{
			NBTConverter.WriteToNBT(this, nbt, version);
		}
	}
}

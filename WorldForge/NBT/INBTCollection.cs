namespace WorldForge.NBT
{
	public interface INBTCollection
	{
		void WriteToNBT(NBTCompound nbt, GameVersion version);

		void LoadFromNBT(NBTCompound nbt, bool remove);
	}
}

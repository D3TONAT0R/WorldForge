namespace WorldForge.NBT
{
	public interface INBTConverter
	{
		object ToNBT(GameVersion version);

		void FromNBT(object nbtData);
	}
}

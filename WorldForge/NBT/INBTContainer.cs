namespace WorldForge.NBT
{
	public interface INBTContainer
	{
		NBTTag ContainerType { get; }

		string[] GetContentKeys(string prefix = null);
	}
}

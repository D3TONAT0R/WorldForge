namespace MCUtils.NBT
{
	public abstract class AbstractNBTContainer
	{
		public AbstractNBTContainer()
		{

		}

		public abstract NBTTag containerType
		{
			get;
		}

		//public abstract T Add<T>(string key, T value);

		public abstract string[] GetContentKeys(string prefix = null);

		//public abstract object Get(string key);

		public override string ToString()
		{
			return containerType.ToString();
		}
	}
}

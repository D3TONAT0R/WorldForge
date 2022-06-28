using MCUtils.NBT;

namespace MCUtils
{
	public class Vector2 : INBTCompatible
	{
		public double x;
		public double y;

		public bool IsZero => x == 0 && y == 0;

		public Vector2(double x, double y)
		{
			this.x = x;
			this.y = y;
		}

		public Vector2(NBTList nbtList) : this(nbtList.Get<double>(0), nbtList.Get<double>(1))
		{

		}

		public object GetNBTCompatibleObject()
		{
			return new NBTList(NBTTag.TAG_Double)
			{
				x,y
			};
		}

		public void ParseFromNBT(object nbtData)
		{
			var list = (NBTList)nbtData;
			x = list.Get<double>(0);
			y = list.Get<double>(1);
		}
	}

	public class Vector3 : INBTCompatible
	{
		public double x;
		public double y;
		public double z;

		public bool IsZero => x == 0 && y == 0 && z == 0;

		public Vector3(double x, double y, double z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public Vector3(NBTList nbtList) : this(nbtList.Get<double>(0), nbtList.Get<double>(1), nbtList.Get<double>(2))
		{

		}

		public object GetNBTCompatibleObject()
		{
			return new NBTList(NBTTag.TAG_Double)
			{
				x,y,z
			};
		}

		public void ParseFromNBT(object nbtData)
		{
			var list = (NBTList)nbtData;
			x = list.Get<double>(0);
			y = list.Get<double>(1);
			z = list.Get<double>(2);
		}
	}
}

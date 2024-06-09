using System;
using WorldForge.NBT;

namespace WorldForge
{
	public abstract class Vector2Base<T> : INBTConverter
	{
		public T x;
		public T y;

		public bool IsZero => Convert.ToDouble(x) == 0 && Convert.ToDouble(y) == 0;

		public Vector2Base()
		{

		}

		public Vector2Base(T x, T y)
		{
			this.x = x;
			this.y = y;
		}

		public Vector2Base(NBTList nbtList) : this(nbtList.Get<T>(0), nbtList.Get<T>(1))
		{

		}

		public object ToNBT(GameVersion version)
		{
			return new NBTList(NBTMappings.GetTag(typeof(T)))
			{
				x, y
			};
		}

		public void FromNBT(object nbtData)
		{
			var list = (NBTList)nbtData;
			x = list.Get<T>(0);
			y = list.Get<T>(1);
		}
	}

	public class Vector2 : Vector2Base<double>
	{
		public Vector2()
		{

		}

		public Vector2(double x, double y) : base(x, y)
		{

		}
	}

	public class Vector2F : Vector2Base<float>
	{
		public Vector2F()
		{

		}

		public Vector2F(float x, float y) : base(x, y)
		{

		}
	}

	public class Vector3Base<T> : INBTConverter
	{
		public T x;
		public T y;
		public T z;

		public bool IsZero => Convert.ToDouble(x) == 0 && Convert.ToDouble(y) == 0 && Convert.ToDouble(z) == 0;

		public Vector3Base()
		{

		}

		public Vector3Base(T x, T y, T z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public Vector3Base(NBTList nbtList) : this(nbtList.Get<T>(0), nbtList.Get<T>(1), nbtList.Get<T>(2))
		{

		}

		public object ToNBT(GameVersion version)
		{
			return new NBTList(NBTMappings.GetTag(typeof(T)))
			{
				x,y,z
			};
		}

		public void FromNBT(object nbtData)
		{
			var list = (NBTList)nbtData;
			x = list.Get<T>(0);
			y = list.Get<T>(1);
			z = list.Get<T>(2);
		}
	}

	public class Vector3 : Vector3Base<double>
	{
		public Vector3()
		{

		}

		public Vector3(double x, double y, double z) : base(x, y, z)
		{

		}
	}

	public class Vector3F : Vector3Base<float>
	{
		public Vector3F()
		{

		}

		public Vector3F(float x, float y, float z) : base(x, y, z)
		{

		}
	}
}

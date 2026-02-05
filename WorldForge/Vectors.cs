using System;
using WorldForge.Coordinates;
using WorldForge.NBT;

namespace WorldForge
{
	public abstract class Vector2Base<T> : INBTConverter
	{
		public T x;
		public T y;

		public bool IsZero => Convert.ToDouble(x) == 0 && Convert.ToDouble(y) == 0;

		public double SqrMagnitude
		{
			get
			{
				var dx = Convert.ToDouble(x);
				var dy = Convert.ToDouble(y);
				return dx * dx + dy * dy;
			}
		}

		public double Magnitude => Math.Sqrt(SqrMagnitude);

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

		public static Vector2 operator +(Vector2 a, Vector2 b)
		{
			return new Vector2(a.x + b.x, a.y + b.y);
		}

		public static Vector2 operator -(Vector2 a, Vector2 b)
		{
			return new Vector2(a.x - b.x, a.y - b.y);
		}

		public static Vector2 operator *(Vector2 a, double b)
		{
			return new Vector2(a.x * b, a.y * b);
		}

		public static Vector2 operator /(Vector2 a, double b)
		{
			return new Vector2(a.x / b, a.y / b);
		}

		public static double Distance(Vector2 a, Vector2 b)
		{
			return (b - a).Magnitude;
		}

		public static Vector2 Normalize(Vector2 a)
		{
			return a / a.Magnitude;
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

		public static Vector2F operator +(Vector2F a, Vector2F b)
		{
			return new Vector2F(a.x + b.x, a.y + b.y);
		}

		public static Vector2F operator -(Vector2F a, Vector2F b)
		{
			return new Vector2F(a.x - b.x, a.y - b.y);
		}

		public static Vector2F operator *(Vector2F a, float b)
		{
			return new Vector2F(a.x * b, a.y * b);
		}

		public static Vector2F operator /(Vector2F a, float b)
		{
			return new Vector2F(a.x / b, a.y / b);
		}

		public static double Distance(Vector2F a, Vector2F b)
		{
			return (b - a).Magnitude;
		}

		public static Vector2F Normalize(Vector2F a)
		{
			return a / (float)a.Magnitude;
		}
	}

	public class Vector3Base<T> : INBTConverter
	{
		public T x;
		public T y;
		public T z;

		public bool IsZero => Convert.ToDouble(x) == 0 && Convert.ToDouble(y) == 0 && Convert.ToDouble(z) == 0;

		public double SqrMagnitude
		{
			get
			{
				var dx = Convert.ToDouble(x);
				var dy = Convert.ToDouble(y);
				var dz = Convert.ToDouble(z);
				return dx * dx + dy * dy + dz * dz;
			}
		}

		public double Magnitude => Math.Sqrt(SqrMagnitude);

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

		public static Vector3 operator +(Vector3 a, Vector3 b)
		{
			return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
		}

		public static Vector3 operator -(Vector3 a, Vector3 b)
		{
			return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
		}

		public static Vector3 operator *(Vector3 a, double b)
		{
			return new Vector3(a.x * b, a.y * b, a.z * b);
		}

		public static Vector3 operator /(Vector3 a, double b)
		{
			return new Vector3(a.x / b, a.y / b, a.z / b);
		}

		public BlockCoord Block => new BlockCoord((int)Math.Floor(x), (int)Math.Floor(y), (int)Math.Floor(z));

		public static double Distance(Vector3 a, Vector3 b)
		{
			return (b - a).Magnitude;
		}

		public static Vector3 Normalize(Vector3 a)
		{
			return a / a.Magnitude;
		}

		public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
		{
			return new Vector3(
				a.x + (b.x - a.x) * t,
				a.y + (b.y - a.y) * t,
				a.z + (b.z - a.z) * t
			);
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

		public static Vector3F operator +(Vector3F a, Vector3F b)
		{
			return new Vector3F(a.x + b.x, a.y + b.y, a.z + b.z);
		}

		public static Vector3F operator -(Vector3F a, Vector3F b)
		{
			return new Vector3F(a.x - b.x, a.y - b.y, a.z - b.z);
		}

		public static Vector3F operator *(Vector3F a, float b)
		{
			return new Vector3F(a.x * b, a.y * b, a.z * b);
		}

		public static Vector3F operator /(Vector3F a, float b)
		{
			return new Vector3F(a.x / b, a.y / b, a.z / b);
		}

		public static double Distance(Vector3F a, Vector3F b)
		{
			return (b - a).Magnitude;
		}

		public static Vector3F Normalize(Vector3F a)
		{
			return a / (float)a.Magnitude;
		}
	}
}

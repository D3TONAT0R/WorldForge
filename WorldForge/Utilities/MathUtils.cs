using System;
using System.Collections.Generic;
using System.Text;

namespace WorldForge
{
	public static class MathUtils
	{

		public static double Lerp(double a, double b, double t)
		{
			return a + (b - a) * t;
		}

		public static float Lerp(float a, float b, float t)
		{
			return a + (b - a) * t;
		}

		public static double InverseLerp(double a, double b, double v)
		{
			if(b == a) return a;
			return (v - a) / (b - a);
		}

		public static float InverseLerp(float a, float b, float v)
		{
			if(b == a) return a;
			return (v - a) / (b - a);
		}

		public static double Remap(double a1, double b1, double a2, double b2, double v)
		{
			return Lerp(a2, b2, InverseLerp(a1, b1, v));
		}

		public static float Remap(float a1, float b1, float a2, float b2, float v)
		{
			return Lerp(a2, b2, InverseLerp(a1, b1, v));
		}
	}
}

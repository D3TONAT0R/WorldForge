using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Threading.Tasks;
using WorldForge;

namespace WorldForge.Builders.PostProcessors
{
	public class Weightmap<T>
	{
		private T[][,] channels;

		private Weightmap(int channelCount)
		{
			channels = new T[channelCount][,];
		}

		public T GetValue(int x, int y, int channel = 0)
		{
			return channels[channel][x, y];
		}

		public void SetValue(int x, int y, int channel, T value)
		{
			channels[channel][x, y] = value;
		}

		public static Weightmap<float> CreateSingleChannelMap(string path, ColorChannel channel, int offsetX, int offsetZ, int sizeX, int sizeZ)
		{
			var map = new Weightmap<float>(1);
			var image = Image.Load<Rgba32>(path);
			map.channels[0] = GetMask(image, channel, offsetX, offsetZ, sizeX, sizeZ);
			return map;
		}

		public static Weightmap<float> CreateRGBAMap(string path, int offsetX, int offsetZ, int sizeX, int sizeZ)
		{
			var map = new Weightmap<float>(4);
			var image = Image.Load<Rgba32>(path);
			map.channels[0] = GetMask(image, ColorChannel.Red, offsetX, offsetZ, sizeX, sizeZ);
			map.channels[1] = GetMask(image, ColorChannel.Green, offsetX, offsetZ, sizeX, sizeZ);
			map.channels[2] = GetMask(image, ColorChannel.Blue, offsetX, offsetZ, sizeX, sizeZ);
			map.channels[3] = GetMask(image, ColorChannel.Alpha, offsetX, offsetZ, sizeX, sizeZ);
			return map;
		}

		public static Weightmap<byte> GetFixedWeightmap(string path, Rgba32[] mappings, int ditherLimit, int offsetX, int offsetZ, int sizeX, int sizeZ)
		{
			var image = Image.Load<Rgba32>(path);
			byte[,] map = new byte[sizeX, sizeZ];
			Parallel.For(0, sizeX, x =>
			{
				for(int y = 0; y < sizeZ; y++)
				{
					var c = image[offsetX + x, offsetZ + y];
					byte mapping;
					if(ditherLimit > 1)
					{
						mapping = GetDitheredMapping(x, y, c, mappings, ditherLimit);
					}
					else
					{
						mapping = GetClosestMapping(c, mappings);
					}
					map[x, y] = mapping;
				}
			});
			var weightmap = new Weightmap<byte>(1);
			weightmap.channels[0] = map;
			return weightmap;
		}

		public static Weightmap<bool> GetBitMask(float[,] mask, float threshold)
		{
			bool[,] bitMask = new bool[mask.GetLength(0), mask.GetLength(1)];
			for(int x = 0; x < mask.GetLength(0); x++)
			{
				for(int y = 0; y < mask.GetLength(1); y++)
				{
					bitMask[x, y] = mask[x, y] >= threshold;
				}
			}
			var weightmap = new Weightmap<bool>(1);
			weightmap.channels[0] = bitMask;
			return weightmap;
		}

		private static float[,] GetMask(Image<Rgba32> image, ColorChannel channel, int offsetX, int offsetZ, int sizeX, int sizeZ)
		{
			float[,] mask = new float[sizeX, sizeZ];
			Parallel.For(0, sizeX, x =>
			{
				for(int y = 0; y < sizeZ; y++)
				{
					var c = image[offsetX + x, offsetZ + y];
					byte v = 0;
					if(channel == ColorChannel.Red)
					{
						v = c.R;
					}
					else if(channel == ColorChannel.Green)
					{
						v = c.G;
					}
					else if(channel == ColorChannel.Blue)
					{
						v = c.B;
					}
					else if(channel == ColorChannel.Alpha)
					{
						v = c.A;
					}
					mask[x, y] = (v / 255f);
				}
			});
			return mask;
		}

		static byte GetClosestMapping(Rgba32 c, Rgba32[] mappings)
		{
			int[] deviations = new int[mappings.Length];
			for(int i = 0; i < mappings.Length; i++)
			{
				deviations[i] += Math.Abs(c.R - mappings[i].R);
				deviations[i] += Math.Abs(c.G - mappings[i].G);
				deviations[i] += Math.Abs(c.B - mappings[i].B);
			}
			byte index = 255;
			int closest = 999;
			for(byte i = 0; i < mappings.Length; i++)
			{
				if(deviations[i] < closest)
				{
					index = i;
					closest = deviations[i];
				}
			}
			return index;
		}

		static byte GetDitheredMapping(int x, int y, Rgba32 c, Rgba32[] mappings, int ditherLimit)
		{
			float[] probs = new float[mappings.Length];
			for(int i = 0; i < mappings.Length; i++)
			{
				int deviation = 0;
				deviation += Math.Abs(c.R - mappings[i].R);
				deviation += Math.Abs(c.G - mappings[i].G);
				deviation += Math.Abs(c.B - mappings[i].B);
				if(deviation >= ditherLimit)
				{
					probs[i] = 0;
				}
				else
				{
					probs[i] = 1 - (deviation / (float)ditherLimit);
				}
			}
			float max = 0;
			foreach(float p in probs) max += p;
			double d = SeededRandom.Double(x, y, 0) * max;
			double v = 0;
			for(byte i = 0; i < probs.Length; i++)
			{
				v += probs[i];
				if(d < v) return i;
			}
			return 255;
		}
	}
}

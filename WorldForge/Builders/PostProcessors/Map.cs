﻿using System;
using System.Threading.Tasks;
using WorldForge.Coordinates;

namespace WorldForge.Builders.PostProcessors
{
	public class Map<T>
	{
		private T[][,] data;

		private int LengthX => data[0].GetLength(0);
		private int LengthZ => data[0].GetLength(1);

		public BlockCoord2D LowerCornerPos { get; set; }
		public BlockCoord2D UpperCornerPos => LowerCornerPos + new BlockCoord2D(LengthX, LengthZ);

		public Map(int width, int height, int channelCount)
		{
			data = new T[channelCount][,];
			if(width >= 0 && height >= 0)
			{
				for(int i = 0; i < channelCount; i++)
				{
					data[i] = new T[width, height];
				}
			}
		}

		public T GetValue(BlockCoord2D pos, int channel = 0)
		{
			if(ToMapPosition(pos, out int x, out int y))
			{
				return data[channel][x, y];
			}
			return default;
		}

		public bool TryGetValue(BlockCoord2D pos, int channel, out T value)
		{
			if(ToMapPosition(pos, out int x, out int y))
			{
				value = data[channel][x, y];
				return true;
			}
			value = default;
			return false;
		}

		public T GetValueOrDefault(BlockCoord2D pos, int channel, T defaultValue)
		{
			if(ToMapPosition(pos, out int x, out int y))
			{
				return data[channel][x, y];
			}
			return defaultValue;
		}

		public void SetValue(BlockCoord2D pos, int channel, T value)
		{
			if(ToMapPosition(pos, out int x, out int y))
			{
				data[channel][x, y] = value;
			}
			else
			{
				throw new ArgumentException($"Position is out of bounds of the map: {pos}");
			}
		}

		public bool TrySetValue(BlockCoord2D pos, int channel, T value)
		{
			if(ToMapPosition(pos, out int x, out int y))
			{
				data[channel][x, y] = value;
				return true;
			}
			return false;
		}

		public bool ToMapPosition(BlockCoord2D pos, out int x, out int y)
		{
			x = pos.x - LowerCornerPos.x;
			y = pos.z - LowerCornerPos.z;
			if(x >= 0 && x < LengthX && y >= 0 && y < LengthZ)
			{
				return true;
			}
			return false;
		}

		public static Map<float> CreateSingleChannelMap(string path, ColorChannel channel, Boundary? bounds = null)
		{
			var image = Bitmaps.Load(path);
			var map = new Map<float>(-1, -1, 1);
			map.data[0] = CreateMask(image, channel, bounds);
			return map;
		}

		public static Map<float> CreateRGBAMap(string path, Boundary? bounds = null)
		{
			var map = new Map<float>(-1, -1, 4);
			var image = Bitmaps.Load(path);
			map.data[0] = CreateMask(image, ColorChannel.Red, bounds);
			map.data[1] = CreateMask(image, ColorChannel.Green, bounds);
			map.data[2] = CreateMask(image, ColorChannel.Blue, bounds);
			map.data[3] = CreateMask(image, ColorChannel.Alpha, bounds);
			return map;
		}

		public static Map<byte> CreateFixedMap(string path, BitmapColor[] mappings, int ditherLimit, Boundary? bounds = null)
		{
			var image = Bitmaps.Load(path);
			int sizeX = bounds?.LengthX ?? image.Width;
			int sizeZ = bounds?.LengthZ ?? image.Height;
			int offsetX = bounds?.xMin ?? 0;
			int offsetZ = bounds?.zMin ?? 0;
			var map = new Map<byte>(sizeX, sizeZ, 1);
			Parallel.For(0, sizeX, x =>
			{
				for(int y = 0; y < sizeZ; y++)
				{
					var c = image.GetPixel(offsetX + x, offsetZ + y);
					byte mapping;
					if(ditherLimit > 1)
					{
						mapping = (byte)BitmapColor.GetDitheredMapping(x, y, c, mappings, ditherLimit);
					}
					else
					{
						mapping = (byte)BitmapColor.GetClosestMapping(c, mappings);
					}
					map.data[0][x, y] = mapping;
				}
			});
			return map;
		}

		public static Map<byte> CreateByteMap(string path, ColorChannel channel = ColorChannel.Red, Boundary? bounds = null)
		{
			var map = new Map<byte>(-1, -1, 1);
			var image = Bitmaps.Load(path);
			map.data[0] = GetByteMask(image, channel, bounds);
			return map;
		}

		public static Map<bool> CreateBitMap(string path, float threshold, ColorChannel channel = ColorChannel.Red, Boundary? bounds = null)
		{
			var map = new Map<bool>(-1, -1, 1);
			var image = Bitmaps.Load(path);
			map.data[0] = CreateBitArray(image, threshold, channel, bounds);
			return map;
		}

		private static bool[,] CreateBitArray(IBitmap image, float threshold, ColorChannel channel, Boundary? bounds = null)
		{
			int sizeX = bounds?.LengthX ?? image.Width;
			int sizeZ = bounds?.LengthZ ?? image.Height;
			int offsetX = bounds?.xMin ?? 0;
			int offsetZ = bounds?.zMin ?? 0;
			bool[,] mask = new bool[sizeX, sizeZ];
			byte thresholdByte = (byte)(threshold * 255);
			Parallel.For(0, sizeX, x =>
			{
				for(int y = 0; y < sizeZ; y++)
				{
					var c = image.GetPixel(offsetX + x, offsetZ + y);
					bool v = false;
					if(channel == ColorChannel.Red)
					{
						v = c.r > thresholdByte;
					}
					else if(channel == ColorChannel.Green)
					{
						v = c.g > thresholdByte;
					}
					else if(channel == ColorChannel.Blue)
					{
						v = c.b > thresholdByte;
					}
					else if(channel == ColorChannel.Alpha)
					{
						v = c.a > thresholdByte;
					}
					mask[x, y] = v;
				}
			});
			return mask;
		}

		private static byte[,] GetByteMask(IBitmap image, ColorChannel channel, Boundary? bounds = null)
		{
			int sizeX = bounds?.LengthX ?? image.Width;
			int sizeZ = bounds?.LengthZ ?? image.Height;
			int offsetX = bounds?.xMin ?? 0;
			int offsetZ = bounds?.zMin ?? 0;
			byte[,] mask = new byte[sizeX, sizeZ];
			Parallel.For(0, sizeX, x =>
			{
				for(int y = 0; y < sizeZ; y++)
				{
					var c = image.GetPixel(offsetX + x, offsetZ + y);
					byte v = 0;
					if(channel == ColorChannel.Red)
					{
						v = c.r;
					}
					else if(channel == ColorChannel.Green)
					{
						v = c.g;
					}
					else if(channel == ColorChannel.Blue)
					{
						v = c.b;
					}
					else if(channel == ColorChannel.Alpha)
					{
						v = c.a;
					}
					mask[x, y] = v;
				}
			});
			return mask;
		}

		private static float[,] CreateMask(IBitmap image, ColorChannel channel, Boundary? bounds)
		{
			var byteMask = GetByteMask(image, channel, bounds);
			float[,] mask = new float[byteMask.GetLength(0), byteMask.GetLength(1)];
			for(int x = 0; x < byteMask.GetLength(0); x++)
			{
				for(int y = 0; y < byteMask.GetLength(1); y++)
				{
					mask[x, y] = byteMask[x, y] / 255f;
				}
			}
			return mask;
		}
	}
}

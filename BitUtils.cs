using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MCUtils
{
	public class BitUtils
	{

		public static byte CompressNibbles(byte lowNibble, byte highNibble)
		{
			lowNibble &= 0b00001111;
			highNibble &= 0b00001111;
			return (byte)((highNibble << 4) | lowNibble);
		}

		public static void ExtractNibbles(byte compressedNibbles, out byte lowNibble, out byte highNibble)
		{
			lowNibble = (byte)(compressedNibbles & 0b00001111);
			highNibble = (byte)((compressedNibbles & 0b11110000) >> 4);
		}

		public static int GetMaxBitCount(uint bitValue)
		{
			return (int)Math.Log(bitValue, 2) + 1;
		}

		public static BitArray CreateBitArray(long[] compressedBitArray, int endIndex = 64)
		{
			List<bool> bitArray = new List<bool>();
			for (int i = 0; i < compressedBitArray.Length; i++)
			{
				var bits = new BitArray(BitConverter.GetBytes(compressedBitArray[i]));
				ReverseBitArray(bits);
				for(int j = 0; j < endIndex; j++)
				{
					bitArray.Add(bits[j]);
				}
			}
			return new BitArray(bitArray.ToArray());
		}

		public static void ReverseBitArray(BitArray array)
		{
			int length = array.Length;
			int mid = length / 2;

			for (int i = 0; i < mid; i++)
			{
				bool bit = array[i];
				array[i] = array[length - i - 1];
				array[length - i - 1] = bit;
			}
		}

		public static ushort CreateInt16FromBits(BitArray bits, int index, int length)
		{
			int value = 0;
			for (int i = 0; i < length; i++)
			{
				//Reverse order bits
				int i2 = length - i - 1;
				//int i2 = i;
				if(bits[index + i])
				{
					value += 1 << i2;
				}
			}
			return (ushort)value;
		}
	}
}

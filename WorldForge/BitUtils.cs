using System;
using System.Collections;
using System.Collections.Generic;
using static System.Collections.Specialized.BitVector32;
using WorldForge.Chunks;

namespace WorldForge
{
	public class BitUtils
	{

		public static byte CompressNibbles(byte lowNibble, byte highNibble)
		{
			lowNibble &= 0b00001111;
			highNibble &= 0b00001111;
			return (byte)((highNibble << 4) | lowNibble);
		}

		public static byte[] CompressNibbleArray(byte[] nibbles)
		{
			if(nibbles.Length % 2 == 1) throw new InvalidOperationException("Nibble array length must be a multiple of 2.");
			byte[] compressed = new byte[nibbles.Length / 2];
			for(int i = 0; i < compressed.Length; i++)
			{
				compressed[i] = CompressNibbles(nibbles[i * 2], nibbles[i * 2 + 1]);
			}
			return compressed;
		}

		public static void ExtractNibblesFromByte(byte compressedNibbles, out byte lowNibble, out byte highNibble)
		{
			lowNibble = (byte)(compressedNibbles & 0b00001111);
			highNibble = (byte)((compressedNibbles & 0b11110000) >> 4);
		}

		public static byte[] ExtractNibblesFromByteArray(byte[] compressedNibbleArray)
		{
			var nibbles = new byte[compressedNibbleArray.Length * 2];
			for(int i = 0; i < compressedNibbleArray.Length; i++)
			{
				ExtractNibblesFromByte(compressedNibbleArray[i], out var low, out var high);
				nibbles[i * 2] = low;
				nibbles[i * 2 + 1] = high;
			}
			return nibbles;
		}

		public static int GetMaxBitCount(uint bitValue)
		{
			return (int)Math.Log(bitValue, 2) + 1;
		}

		public static BitArray CreateBitArray(long[] compressedBitArray, int endIndex = 64)
		{
			List<bool> bitArray = new List<bool>();
			for(int i = 0; i < compressedBitArray.Length; i++)
			{
				var bits = new BitArray(BitConverter.GetBytes(compressedBitArray[i]));
				//ReverseBitArray(bits);
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

			for(int i = 0; i < mid; i++)
			{
				bool bit = array[i];
				array[i] = array[length - i - 1];
				array[length - i - 1] = bit;
			}
		}

		public static ushort CreateInt16FromBits(BitArray bits, int index, int length)
		{
			int value = 0;
			for(int i = 0; i < length; i++)
			{
				//Reverse order bits
				//int i2 = length - i - 1;
				int i2 = i;
				if(bits[index + i])
				{
					value += 1 << i2;
				}
			}
			return (ushort)value;
		}

		public static ushort[] ExtractCompressedInts(long[] compressedData, int bitsPerInt, int intArrayLength, bool useFull64BitRange)
		{
			var bitArray = CreateBitArray(compressedData, useFull64BitRange ? 64 : (int)(64.0 / bitsPerInt) * bitsPerInt);
			ushort[] array = new ushort[intArrayLength];
			for(int i = 0; i < intArrayLength; i++)
			{
				array[i] = CreateInt16FromBits(bitArray, i * bitsPerInt, bitsPerInt);
			}
			return array;
		}

		public static ushort[] ExtractCompressedIntsOld(long[] compressedData, int bitsPerInt, int intArrayLength, bool useFull64BitRange)
		{
			string bits = "";
			for(int i = 0; i < compressedData.Length; i++)
			{
				string newBits = "";
				byte[] bytes = BitConverter.GetBytes(compressedData[i]);
				for(int j = 0; j < 8; j++)
				{
					newBits += Converter.ByteToBinary(bytes[j], true);
				}
				if(useFull64BitRange)
				{
					bits += newBits;
				}
				else
				{
					bits += newBits.Substring(0, (int)Math.Floor(newBits.Length / (double)bitsPerInt) * bitsPerInt);
				}
			}
			ushort[] array = new ushort[intArrayLength];
			for(int i = 0; i < array.Length; i++)
			{
				array[i] = Converter.BitsToValue(bits, i * bitsPerInt, bitsPerInt);
			}
			return array;
		}

		public static long[] PackBits(ushort[] values, int bitsPerValue, bool tightPacking)
		{
			int arraySize;
			if(tightPacking)
			{
				arraySize = (int)Math.Ceiling((double)bitsPerValue * values.Length / 64);
			}
			else
			{
				int valuesPerLong = 64 / bitsPerValue;
				arraySize = (int)Math.Ceiling((double)values.Length / valuesPerLong);
			}

			long[] longs = new long[arraySize];
			string[] longsBinary = new string[longs.Length];
			for(int j = 0; j < longsBinary.Length; j++)
			{
				longsBinary[j] = "";
			}
			int i = 0;
			for(int j = 0; j < values.Length; j++)
			{
				string bin = NumToBits(values[j], bitsPerValue);
				bin = Converter.ReverseString(bin);
				if(!tightPacking)
				{
					if(longsBinary[i].Length + bitsPerValue > 64)
					{
						//The full value doesn't fit, start on the next long
						i++;
						longsBinary[i] += bin;
					}
					else
					{
						for(int k = 0; k < bitsPerValue; k++)
						{
							if(longsBinary[i].Length >= 64) i++;
							longsBinary[i] += bin[k];
						}
					}
				}
			}

			for(int j = 0; j < longs.Length; j++)
			{
				string s = longsBinary[j];
				s = s.PadRight(64, '0');
				s = Converter.ReverseString(s);
				longs[j] = Convert.ToInt64(s, 2);
			}

			return longs;
		}

		private static string NumToBits(ushort num, int length)
		{
			string s = Convert.ToString(num, 2);
			if(s.Length > length)
			{
				throw new IndexOutOfRangeException("The number " + num + " does not fit in a binary string with length " + length);
			}
			return s.PadLeft(length, '0');
		}
	}
}

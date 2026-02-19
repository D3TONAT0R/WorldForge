using System;
using System.Collections;
using System.Collections.Generic;
using static System.Collections.Specialized.BitVector32;
using WorldForge.Chunks;
using System.Linq;
using System.Numerics;
using System.Text;

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

		public static ushort[] UnpackBits(long[] compressedData, int bitCount, int arrayLength, bool tightPacking)
		{
			ushort[] values = new ushort[arrayLength];
			int bitsPerLong = tightPacking ? 64 : (64 / bitCount) * bitCount;

			if(!tightPacking)
			{
				int srcIndex = 0;
				int srcPos = 0;
				int mask = (1 << bitCount) - 1;
				for (int i = 0; i < arrayLength; i++)
				{
					values[i] = (ushort)((compressedData[srcIndex] >> srcPos) & mask);
					srcPos += bitCount;
					if (srcPos + bitCount > bitsPerLong)
					{
						srcIndex++;
						srcPos = 0;
					}
				}
			}
			else
			{
				int srcIndex = 0;
				int srcPos = 0;
				int mask = (1 << bitCount) - 1;
				for (int i = 0; i < arrayLength; i++)
				{
					ushort v;
					if (srcPos + bitCount > bitsPerLong)
					{
						//Extract the remaining bits from the current long
						int remainingBits = bitsPerLong - srcPos;
						v = (ushort)((compressedData[srcIndex] >> srcPos) & ((1 << remainingBits) - 1));
						srcIndex++;
						srcPos = 0;
						//Extract the remaining bits from the next long
						v |= (ushort)((compressedData[srcIndex] & ((1 << (bitCount - remainingBits)) - 1)) << remainingBits);
						srcPos += bitCount - remainingBits;
					}
					else
					{
						v = (ushort)((compressedData[srcIndex] >> srcPos) & mask);
						srcPos += bitCount;
					}
					values[i] = v;
				}
			}
			return values;
		}

		public static ushort[] UnpackBitsOld(long[] compressedData, int bitsPerInt, int arrayLength, bool tightPacking)
		{
			ushort[] values = new ushort[arrayLength];
			int bitsPerLong = tightPacking ? 64 : (64 / bitsPerInt) * bitsPerInt;

			for (int i = 0; i < arrayLength; i++)
			{
				for (int j = 0; j < bitsPerInt; j++)
				{
					int bitPos = i * bitsPerInt + j;
					int longIndex = bitPos / bitsPerLong;
					int longBitPos = bitPos % bitsPerLong;
					if (GetBit(compressedData[longIndex], longBitPos))
					{
						SetBit(ref values[i], j, true);
					}
				}
			}
			return values;
		}

		public static long[] PackBits(ushort[] values, int bitsPerValue, bool tightPacking)
		{
			int arraySize;
			int valuesPerLong = tightPacking ? 64 : 64 / bitsPerValue;
			if (tightPacking)
			{
				arraySize = (int)Math.Ceiling((double)bitsPerValue * values.Length / 64);
			}
			else
			{
				arraySize = (int)Math.Ceiling((double)values.Length / valuesPerLong);
			}
			long[] longs = new long[arraySize];
			int dstIndex = 0;
			int dstPos = 0;
			int mask = (1 << bitsPerValue) - 1;
			if (!tightPacking)
			{
				int mod = 64 / bitsPerValue;
				for(int i = 0; i < values.Length; i++)
				{
					long v = values[i] & mask;
					longs[dstIndex] |= v << dstPos;
					dstPos += bitsPerValue;
					if((i + 1) % mod == 0)
					{
						dstIndex++;
						dstPos = 0;
					}
				}
			}
			else
			{
				for(int i = 0; i < values.Length; i++)
				{
					int endPos = dstPos + bitsPerValue - 1;
					if(endPos >= 64)
					{
						int remainingBits = 64 - dstPos;
						long v = values[i] & ((1 << remainingBits) - 1);
						longs[dstIndex] |= v << dstPos;
						dstIndex++;
						dstPos = 0;
						v = (values[i] >> remainingBits) & ((1 << (bitsPerValue - remainingBits)) - 1);
						longs[dstIndex] |= v << dstPos;
						dstPos += bitsPerValue - remainingBits;
					}
					else
					{
						long v = values[i] & mask;
						longs[dstIndex] |= v << dstPos;
						dstPos += bitsPerValue;
					}
				}
			}
			return longs;
		}

		public static long[] PackBitsOld(ushort[] values, int bitsPerValue, bool tightPacking)
		{
			int arraySize;
			int valuesPerLong = tightPacking ? 64 : 64 / bitsPerValue;
			if(tightPacking)
			{
				arraySize = (int)Math.Ceiling((double)bitsPerValue * values.Length / 64);
			}
			else
			{
				arraySize = (int)Math.Ceiling((double)values.Length / valuesPerLong);
			}
			//Convert bits to longs
			int bitCount = values.Length * bitsPerValue;
			long[] longs = new long[arraySize];
			int usedBits = tightPacking ? 64 : valuesPerLong * bitsPerValue;
			//Fill longs all the way
			for(int i = 0; i < bitCount; i++)
			{
				int il = i / usedBits;
				int ib = i % usedBits;
				bool bit = GetBit(values[i / bitsPerValue], i % bitsPerValue);
				if(bit)
				{
					SetBit(ref longs[il], ib, true);
				}
			}
			return longs;
		}

		public static byte ReverseBits(byte b)
		{
			return (byte)(((b * 0x80200802ul) & 0x0884422110ul) * 0x0101010101ul >> 32);
		}

		public static bool GetBit(ushort value, int index)
		{
			return (value & (1 << index)) != 0;
		}

		public static bool GetBit(long value, int index)
		{
			return (value & ((long)1 << index)) != 0;
		}

		public static void SetBit(ref ushort value, int index, bool state)
		{
			if(state) value |= (ushort)(1 << index);
			else value &= (ushort)~(1 << index);
		}

		public static void SetBit(ref long value, int index, bool state)
		{
			if(state) value |= (long)1 << index;
			else value &= ~((long)1 << index);
		}

		//Base 36 encoding / decoding taken from https://github.com/bogdanbujdea/csharpbase36

		private const string base36Digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

		public static string EncodeBase36(long value)
		{
			if(value == long.MinValue)
			{
				//hard coded value due to error when getting absolute value below: "Negating the minimum value of a twos complement number is invalid.".
				return "-1Y2P0IJ32E8E8";
			}
			bool negative = value < 0;
			value = Math.Abs(value);
			string encoded = string.Empty;
			do
				encoded = base36Digits[(int)(value % base36Digits.Length)] + encoded;
			while((value /= base36Digits.Length) != 0);
			return negative ? "-" + encoded : encoded;
		}

		public static long DecodeBase36(string value)
		{
			if(string.IsNullOrWhiteSpace(value))
				throw new ArgumentException("Empty value.");
			value = value.ToUpper();
			bool negative = false;
			if(value[0] == '-')
			{
				negative = true;
				value = value.Substring(1, value.Length - 1);
			}
			if(value.Any(c => !base36Digits.Contains(c)))
				throw new ArgumentException("Invalid value: \"" + value + "\".");
			var decoded = 0L;
			for(var i = 0; i < value.Length; ++i)
				decoded += base36Digits.IndexOf(value[i]) * (long)BigInteger.Pow(base36Digits.Length, value.Length - i - 1);
			return negative ? decoded * -1 : decoded;
		}

		///<summary>Reverses the endianness of the given byte array.</summary>
		public static byte[] ToBigEndian(byte[] input)
		{
			if(BitConverter.IsLittleEndian) Array.Reverse(input);
			return input;
		}
	}
}

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
				int i1 = length - i - 1;
				(array[i], array[i1]) = (array[i1], array[i]);
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
					newBits += ByteToBinary(bytes[j], true);
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
				array[i] = BitsToValue(bits, i * bitsPerInt, bitsPerInt);
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
			//Generate bit array
			BitArray bits = new BitArray(values.Length * bitsPerValue);
			for(int i = 0; i < values.Length; i++)
			{
				for(int j = 0; j < bitsPerValue; j++)
				{
					bits[i * bitsPerValue + j] = GetBit(values[i], j);
				}
			}
			//Convert bits to longs
			long[] longs = new long[arraySize];
			if(tightPacking)
			{
				//Fill longs all the way
				for(int i = 0; i < bits.Length; i++)
				{
					int il = i / 64;
					int ib = i % 64;
					if(bits[i])
					{
						SetBit(ref longs[il], ib, true);
					}
				}
			}
			else
			{
				//Fill longs until there isn't enough space left
				int valuesPerLong = 64 / bitsPerValue;
				int usedBits = valuesPerLong * bitsPerValue;
				for(int i = 0; i < bits.Length; i++)
				{
					int il = i / usedBits;
					int ib = i % usedBits;
					if(bits[i])
					{
						SetBit(ref longs[il], ib, true);
					}
				}
			}
			return longs;
		}

		public static string ToBitString(BitArray bits)
		{
			var sb = new StringBuilder();

			for(int i = 0; i < bits.Count; i++)
			{
				char c = bits[i] ? '1' : '0';
				sb.Append(c);
			}

			return sb.ToString();
		}

		public static byte ReverseBits(byte b)
		{
			return (byte)(((b * 0x80200802ul) & 0x0884422110ul) * 0x0101010101ul >> 32);
		}

		public static bool GetBit(ushort value, int index)
		{
			return (value & (1 << index)) != 0;
		}

		public static void SetBit(ref long value, int index, bool state)
		{
			if(state) value |= (long)1 << index;
			else value &= ~((long)1 << index);
		}

		public static long[] PackBitsOld(ushort[] values, int bitsPerValue, bool tightPacking)
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
				bin = ReverseString(bin);
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
				s = ReverseString(s);
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

		///<summary>Reads a 9-bit value from a binary string, used for storing heightmaps.</summary>
		public static ushort Read9BitValue(string bitString, int index)
		{
			string bits = ReverseString(bitString.Substring(index * 9, 9));
			bits = "0000000" + bits;
			bool[] bitArr = new bool[16];
			for(int i = 0; i < 16; i++)
			{
				bitArr[i] = bits[i] == '1';
			}
			return Convert.ToUInt16(bits, 2);
		}

		///<summary>Reverses the given string. Useful for converting endianness of bit strings.</summary>
		public static string ReverseString(string input)
		{
			char[] chrs = input.ToCharArray();
			Array.Reverse(chrs);
			return new string(chrs);
		}

		///<summary>Converts the byte to a bit string.</summary>
		public static string ByteToBinary(byte b, bool bigendian)
		{
			string s = Convert.ToString((int)b, 2);
			s = s.PadLeft(8, '0');
			if(bigendian) s = ReverseString(s);
			return s;
		}

		///<summary>Converts the given bit string at index to a UInt16 value.</summary>
		public static ushort BitsToValue(string bitString, int index, int length)
		{
			string bits = ReverseString(bitString.Substring(index, length));
			bits = bits.PadLeft(16, '0');
			bool[] bitArr = new bool[16];
			for(int i = 0; i < 16; i++)
			{
				bitArr[i] = bits[i] == '1';
			}
			return Convert.ToUInt16(bits, 2);
		}

		///<summary>Reverses the endianness of the given byte array.</summary>
		public static byte[] ToBigEndian(byte[] input)
		{
			if(BitConverter.IsLittleEndian) Array.Reverse(input);
			return input;
		}
	}
}

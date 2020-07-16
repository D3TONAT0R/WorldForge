using System;

namespace MCUtils {
	public static class Converter {

		///<summary>Reads a 9-bit value from a binary string, used for storing heightmaps.</summary>
		public static ushort Read9BitValue(string bitString, int index) {
			string bits = ReverseString(bitString.Substring(index * 9, 9));
			bits = "0000000" + bits;
			bool[] bitArr = new bool[16];
			for(int i = 0; i < 16; i++) {
				bitArr[i] = bits[i] == '1';
			}
			return Convert.ToUInt16(bits, 2);
		}

		///<summary>Reverses the given string. Useful for converting endianness of bit strings.</summary>
		public static string ReverseString(string input) {
			char[] chrs = input.ToCharArray();
			Array.Reverse(chrs);
			return new string(chrs);
		}

		///<summary>Converts the byte to a bit string.</summary>
		public static string ByteToBinary(byte b, bool bigendian) {
			string s = Convert.ToString((int)b, 2);
			s = s.PadLeft(8, '0');
			if(bigendian) s = ReverseString(s);
			return s;
		}

		///<summary>Converts the given bit string at index to a UInt16 value.</summary>
		public static ushort BitsToValue(string bitString, int index, int length) {
			string bits = ReverseString(bitString.Substring(index * length, length));
			bits = bits.PadLeft(16, '0');
			bool[] bitArr = new bool[16];
			for(int i = 0; i < 16; i++) {
				bitArr[i] = bits[i] == '1';
			}
			return Convert.ToUInt16(bits, 2);
		}

		///<summary>Reverses the endianness of the given byte array.</summary>
		public static byte[] ReverseEndianness(byte[] input) {
			if(BitConverter.IsLittleEndian) Array.Reverse(input);
			return input;
		}
	}
}
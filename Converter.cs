using System;

namespace MCUtils {
	public static class Converter {

		public static ushort Read9BitValue(string bitString, int index) {
			string bits = ReverseString(bitString.Substring(index * 9, 9));
			bits = "0000000" + bits;
			bool[] bitArr = new bool[16];
			for(int i = 0; i < 16; i++) {
				bitArr[i] = bits[i] == '1';
			}
			return Convert.ToUInt16(bits, 2);
		}

		public static string ReverseString(string input) {
			char[] chrs = input.ToCharArray();
			Array.Reverse(chrs);
			return new string(chrs);
		}

		public static string ByteToBinary(byte b, bool bigendian) {
			string s = Convert.ToString((int)b, 2);
			s = s.PadLeft(8, '0');
			if(bigendian) s = ReverseString(s);
			return s;
		}

		public static byte[] ToBigEndianByteArray(byte[] input) {
			if(BitConverter.IsLittleEndian) Array.Reverse(input);
			return input;
		}
	}
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace WorldForge.NBT
{
	public class NBTFile
	{
		///<summary>The compound container containing all stored data</summary>
		public NBTCompound contents;

		///<summary>
		///The version number this nbt compound was created with (only valid for versions release 1.9 and up)
		///</summary>
		public int? dataVersion;

		///<summary>Instantiates an empty NBT structure.</summary>
		public NBTFile()
		{
			contents = new NBTCompound();
		}

		///<summary>Creates an NBT structure from the given file.</summary>
		public NBTFile(string filePath) : this()
		{
			using(var stream = Compression.CreateGZipDecompressionStream(File.ReadAllBytes(filePath)))
			{
				LoadFromStream(stream);
			}
		}

		///<summary>Creates an NBT structure from the given bytes.</summary>
		public NBTFile(Stream uncompressedStream) : this()
		{
			LoadFromStream(uncompressedStream);
			PostLoad();
		}

		private void LoadFromStream(Stream uncompressedStream)
		{
			while(uncompressedStream.Position < uncompressedStream.Length)
			{
				RegisterTag(uncompressedStream, contents);
			}
			PostLoad();
		}

		private void PostLoad()
		{
			//Remove the unnessecary root compound
			if(contents.ItemCount == 1 && contents.TryGet<NBTCompound>("", out var root))
			{
				contents = root;
			}
			if(contents.Contains("DataVersion")) dataVersion = (int)contents.Get("DataVersion");
		}

		///<summary>Generates an uncompressed byte array from the content of this NBT structure.</summary>
		public byte[] WriteBytesUncompressed()
		{
			var bytes = new List<byte>();
			Write(bytes, "", contents);
			return bytes.ToArray();
		}

		///<summary>Writes the content of this NBT structure to a file using Zlib compression.</summary>
		public void SaveToFile(string filePath)
		{
			File.WriteAllBytes(filePath, WriteBytesGZip());
		}

		///<summary>Generates a GZip compressed byte array from the content of this NBT structure. GZip is used to compress NBT data (*.dat) files.</summary>
		public byte[] WriteBytesGZip()
		{
			return Compression.CompressGZipBytes(WriteBytesUncompressed());
		}

		///<summary>Generates a Zlib compressed byte array from the content of this NBT structure. Zlib is used only within region files to store chunks.</summary>
		public byte[] WriteBytesZlib()
		{
			return Compression.CompressZlibBytes(WriteBytesUncompressed());
		}

		public void AddLevelRootCompound()
		{
			if(contents.Contains("Level"))
			{
				throw new InvalidOperationException("Level root compound has already been added.");
			}

			NBTCompound root = new NBTCompound();
			NBTCompound level = new NBTCompound();
			foreach(string k in contents.contents.Keys)
			{
				level.Add(k, contents.Get(k));
			}
			root.Add("Level", level);
			contents = root;
		}

		///<summary>Finds and reads the heightmap data stored in a chunk NBT structure.</summary>
		public short[,] GetHeightmapFromChunkNBT(HeightmapType type)
		{
			try
			{
				if(contents.Contains("Heightmaps"))
				{
					//It's the "new" format
					//TODO: deal with 1.17's new heightmaps
					NBTCompound hmcomp = contents.GetAsCompound("Heightmaps");
					if(type == HeightmapType.SolidBlocksNoLiquid && hmcomp.Contains("OCEAN_FLOOR"))
					{
						//The highest non-air block, solid block
						return GetHeightmap((long[])hmcomp.Get("OCEAN_FLOOR"));
					}
					else if(type == HeightmapType.SolidBlocks && hmcomp.Contains("MOTION_BLOCKING"))
					{
						//The highest block that blocks motion or contains a fluid
						return GetHeightmap((long[])hmcomp.Get("MOTION_BLOCKING"));
					}
					else if(hmcomp.Contains("WORLD_SURFACE"))
					{
						//The highest non-air block
						return GetHeightmap((long[])hmcomp.Get("WORLD_SURFACE"));
					}
					else
					{
						return null;
					}
				}
				else if(contents.Contains("HeightMap"))
				{
					//It's the old, simple format
					byte[] hmbytes = (byte[])contents.Get("HeightMap");
					short[,] hm = new short[16, 16];
					for(int z = 0; z < 16; z++)
					{
						for(int x = 0; x < 16; x++)
						{
							var value = hmbytes[z * 16 + x];
							hm[x, z] = value;
						}
					}
					return hm;
				}
				else
				{
					//No heightmap data was found
					return null;
				}
			}
			catch
			{
				return null;
			}
		}

		///<summary>Reads the heightmap stored in the given long array.</summary>
		public short[,] GetHeightmap(long[] hmlongs)
		{
			if(hmlongs == null) return null;
			short[,] hm = new short[16, 16];
			try
			{
				string hmbits = "";
				if(hmlongs.Length == 37)
				{
					//1.16 format
					for(int i = 0; i < 37; i++)
					{
						byte[] bytes = BitConverter.GetBytes(hmlongs[i]);
						string s = "";
						for(int j = 0; j < 8; j++)
						{
							s += Converter.ByteToBinary(bytes[j], true);
						}
						hmbits += s.Substring(0, 63); //Remove the last unused bit
					}
				}
				else
				{
					//pre 1.16 "full bit range" format
					for(int i = 0; i < 36; i++)
					{
						byte[] bytes = BitConverter.GetBytes(hmlongs[i]);
						for(int j = 0; j < 8; j++)
						{
							hmbits += Converter.ByteToBinary(bytes[j], true);
						}
					}
				}
				short[] hmap = new short[256];
				for(int i = 0; i < 256; i++)
				{
					hmap[i] = (short)Converter.Read9BitValue(hmbits, i);
				}

				/*for(int i = 0; i < 256; i++) {
					hmap[i] = (short)Converter.Read9BitValue(hmbits, i);
				}*/
				if(hmbits != null)
				{
					for(int z = 0; z < 16; z++)
					{
						for(int x = 0; x < 16; x++)
						{
							var value = hmap[z * 16 + x];
							hm[x, z] = value;
						}
					}
				}
				return hm;
			}
			catch
			{
				return null;
			}
		}

		byte ReadNext(Stream stream)
		{
			int r = stream.ReadByte();
			if(r >= 0)
			{
				return (byte)r;
			}
			else
			{
				throw new EndOfStreamException();
			}
		}

		byte[] ReadNext(Stream stream, int count)
		{
			byte[] b = new byte[count];
			for(int i = 0; i < count; i++)
			{
				b[i] = ReadNext(stream);
			}
			return b;
		}

		NBTTag RegisterTag(Stream stream, AbstractNBTContainer c, NBTTag predef = NBTTag.UNSPECIFIED)
		{
			NBTTag tag;
			/*if(compound.GetType() == typeof(ListContainer)) {
				tag = ((ListContainer)compound).containerType;
				i++;
			} else {*/
			if(predef == NBTTag.UNSPECIFIED)
			{
				tag = (NBTTag)ReadNext(stream);
			}
			else
			{
				tag = predef;
			}
			//}
			object value;
			if(tag != NBTTag.TAG_End)
			{
				string name = "";
				if(predef == NBTTag.UNSPECIFIED)
				{
					short nameLength = BitConverter.ToInt16(Converter.ReverseEndianness(ReadNext(stream, 2)), 0);
					if(nameLength > 64)
					{
						Console.WriteLine("NL=" + nameLength + "! Something is going wrong");
					}
					for(int j = 0; j < nameLength; j++)
					{
						//TODO: Reading List with TAG_End throws IndexOutOfRangeException
						name += (char)ReadNext(stream);
					}
				}
				if(tag == NBTTag.TAG_Byte)
				{
					value = Get<byte>(stream);
				}
				else if(tag == NBTTag.TAG_Short)
				{
					value = Get<short>(stream);
				}
				else if(tag == NBTTag.TAG_Int)
				{
					value = Get<int>(stream);
				}
				else if(tag == NBTTag.TAG_Long)
				{
					value = Get<long>(stream);
				}
				else if(tag == NBTTag.TAG_Float)
				{
					value = Get<float>(stream);
				}
				else if(tag == NBTTag.TAG_Double)
				{
					value = Get<double>(stream);
				}
				else if(tag == NBTTag.TAG_Byte_Array)
				{
					value = Get<byte[]>(stream);
				}
				else if(tag == NBTTag.TAG_String)
				{
					value = Get<string>(stream);
				}
				else if(tag == NBTTag.TAG_List)
				{
					value = Get<NBTList>(stream);
				}
				else if(tag == NBTTag.TAG_Compound)
				{
					value = Get<NBTCompound>(stream);
				}
				else if(tag == NBTTag.TAG_Int_Array)
				{
					value = Get<int[]>(stream);
				}
				else if(tag == NBTTag.TAG_Long_Array)
				{
					value = Get<long[]>(stream);
				}
				else
				{
					throw new ArgumentException("Unrecognized nbt tag: " + tag);
				}

				if(c is NBTCompound comp)
				{
					comp.Add(name, value);
				}
				else if(c is NBTList list)
				{
					list.Add(value);
				}
				else
				{
					throw new InvalidOperationException("Unknown container type: " + c.GetType());
				}
			}
			else
			{

				//ExitContainer();
			}
			return tag;
		}

		NBTList GetList(NBTTag tag, int length, Stream stream)
		{
			NBTList arr = new NBTList(tag);
			//compound = EnterContainer(compound, arr);
			for(int j = 0; j < length; j++)
			{
				RegisterTag(stream, arr, tag);
			}
			//compound = ExitContainer();
			return arr;
		}

		T Get<T>(Stream stream)
		{
			object ret = null;
			if(typeof(T) == typeof(byte))
			{
				ret = ReadNext(stream);
			}
			else if(typeof(T) == typeof(short))
			{
				ret = BitConverter.ToInt16(Converter.ReverseEndianness(ReadNext(stream, 2)), 0);
			}
			else if(typeof(T) == typeof(int))
			{
				ret = BitConverter.ToInt32(Converter.ReverseEndianness(ReadNext(stream, 4)), 0);
			}
			else if(typeof(T) == typeof(long))
			{
				ret = BitConverter.ToInt64(Converter.ReverseEndianness(ReadNext(stream, 8)), 0);
			}
			else if(typeof(T) == typeof(float))
			{
				ret = BitConverter.ToSingle(Converter.ReverseEndianness(ReadNext(stream, 4)), 0);
			}
			else if(typeof(T) == typeof(double))
			{
				ret = BitConverter.ToDouble(Converter.ReverseEndianness(ReadNext(stream, 8)), 0);
			}
			else if(typeof(T) == typeof(byte[]))
			{
				int len = Get<int>(stream);
				byte[] arr = new byte[len];
				for(int j = 0; j < len; j++)
				{
					arr[j] = Get<byte>(stream);
				}
				ret = arr;
			}
			else if(typeof(T) == typeof(string))
			{
				int len = Get<short>(stream);
				byte[] arr = new byte[len];
				for(int j = 0; j < len; j++)
				{
					arr[j] = Get<byte>(stream);
				}
				ret = Encoding.UTF8.GetString(arr);
			}
			else if(typeof(T) == typeof(NBTList))
			{
				NBTTag type = (NBTTag)Get<byte>(stream);
				int len = Get<int>(stream);
				ret = GetList(type, len, stream);
			}
			else if(typeof(T) == typeof(NBTCompound))
			{
				var newCompound = new NBTCompound();
				while(RegisterTag(stream, newCompound) != NBTTag.TAG_End)
				{

				}
				ret = newCompound;
			}
			else if(typeof(T) == typeof(int[]))
			{
				int len = Get<int>(stream);
				int[] arr = new int[len];
				for(int j = 0; j < len; j++)
				{
					arr[j] = Get<int>(stream);
				}
				ret = arr;
			}
			else if(typeof(T) == typeof(long[]))
			{
				int len = Get<int>(stream);
				long[] arr = new long[len];
				for(int j = 0; j < len; j++)
				{
					arr[j] = Get<long>(stream);
				}
				ret = arr;
			}
			return (T)Convert.ChangeType(ret, typeof(T));
		}

		void Write(List<byte> bytes, string name, object o)
		{
			//Convert enums to their underlying type
			if(o is Enum e)
			{
				o = Convert.ChangeType(o, Enum.GetUnderlyingType(e.GetType()));
			}
			//Convert lists to NBT lists
			if(o.GetType().IsGenericType && o.GetType().GetGenericTypeDefinition() == typeof(List<>))
			{
				o = GenericListToNBTList(o);
			}

			var tag = NBTMappings.GetTag(o.GetType());
			bytes.Add((byte)tag);
			byte[] nameBytes = Encoding.UTF8.GetBytes(name);
			byte[] lengthBytes = Converter.ReverseEndianness(BitConverter.GetBytes((short)nameBytes.Length));
			bytes.AddRange(lengthBytes);
			bytes.AddRange(nameBytes);
			WriteValue(bytes, tag, o);
		}

		NBTList GenericListToNBTList(object obj)
		{
			var list = (IList)obj;

			//Get the type of the list
			var type = obj.GetType();
			//Get the type of the list's contents
			var contentsType = type.GetGenericArguments()[0];
			//Get the NBT tag for the list's contents
			var tag = NBTMappings.GetTag(contentsType);

			var nbtList = new NBTList(tag);
			foreach(var item in list)
			{
				nbtList.Add(item);
			}
			return nbtList;
		}

		void WriteValue(List<byte> bytes, NBTTag tag, object o)
		{
			if(tag == NBTTag.TAG_Byte)
			{
				if(o is byte b) bytes.Add(b);
				else if(o is sbyte s) unchecked { bytes.Add((byte)s); }
				else bytes.Add(Convert.ToByte(o));
			}
			else if(tag == NBTTag.TAG_Short)
			{
				bytes.AddRange(Converter.ReverseEndianness(BitConverter.GetBytes((short)o)));
			}
			else if(tag == NBTTag.TAG_Int)
			{
				bytes.AddRange(Converter.ReverseEndianness(BitConverter.GetBytes((int)o)));
			}
			else if(tag == NBTTag.TAG_Long)
			{
				bytes.AddRange(Converter.ReverseEndianness(BitConverter.GetBytes((long)o)));
			}
			else if(tag == NBTTag.TAG_Float)
			{
				bytes.AddRange(Converter.ReverseEndianness(BitConverter.GetBytes((float)o)));
			}
			else if(tag == NBTTag.TAG_Double)
			{
				bytes.AddRange(Converter.ReverseEndianness(BitConverter.GetBytes((double)o)));
			}
			else if(tag == NBTTag.TAG_Byte_Array)
			{
				WriteValue(bytes, NBTTag.TAG_Int, ((byte[])o).Length);
				foreach(byte b in (byte[])o)
				{
					WriteValue(bytes, NBTTag.TAG_Byte, b);
				}
			}
			else if(tag == NBTTag.TAG_String)
			{
				byte[] utf8 = Encoding.UTF8.GetBytes((string)o);
				WriteValue(bytes, NBTTag.TAG_Short, (short)utf8.Length);
				bytes.AddRange(utf8);
			}
			else if(tag == NBTTag.TAG_List)
			{
				NBTList list = (NBTList)o;
				bytes.Add((byte)list.contentsType);
				WriteValue(bytes, NBTTag.TAG_Int, list.listContent.Count);
				foreach(object item in list.listContent)
				{
					WriteValue(bytes, list.contentsType, item);
				}
			}
			else if(tag == NBTTag.TAG_Compound)
			{
				NBTCompound compound = (NBTCompound)o;
				foreach(string k in compound.contents.Keys)
				{
					Write(bytes, k, compound.contents[k]);
				}
				bytes.Add((byte)NBTTag.TAG_End);
			}
			else if(tag == NBTTag.TAG_Int_Array)
			{
				WriteValue(bytes, NBTTag.TAG_Int, ((int[])o).Length);
				foreach(var item in (int[])o)
				{
					WriteValue(bytes, NBTTag.TAG_Int, item);
				}
			}
			else if(tag == NBTTag.TAG_Long_Array)
			{
				WriteValue(bytes, NBTTag.TAG_Int, ((long[])o).Length);
				foreach(var item in (long[])o)
				{
					WriteValue(bytes, NBTTag.TAG_Long, item);
				}
			}
		}

	}
}
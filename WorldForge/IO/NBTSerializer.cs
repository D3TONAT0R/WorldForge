using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WorldForge.NBT;

namespace WorldForge.IO
{
	public static class NBTSerializer
	{
		public static void Deserialize(NBTFile file, Stream uncompressedStream)
		{
			while(uncompressedStream.Position < uncompressedStream.Length)
			{
				RegisterTag(uncompressedStream, file.contents);
			}
		}

		///<summary>Generates a GZip compressed byte array from the content of this NBT structure. GZip is used to compress NBT data (*.dat) files.</summary>
		public static byte[] SerializeAsGzip(NBTFile file, bool createSubContainer)
		{
			return Compression.CompressGZipBytes(Serialize(file, createSubContainer).ToArray());
		}

		///<summary>Generates a Zlib compressed byte array from the content of this NBT structure. Zlib is used only within region files to store chunks.</summary>
		public static byte[] SerializeAsZlib(NBTFile file, bool createSubContainer)
		{
			return Compression.CompressZlibBytes(Serialize(file, createSubContainer).ToArray());
		}

		///<summary>Generates an uncompressed byte array from the content of this NBT structure.</summary>
		public static MemoryStream Serialize(NBTFile file, bool writeSubContainerAndDataVersion)
		{
			NBTCompound final;
			if(writeSubContainerAndDataVersion)
			{
				final = new NBTCompound();
				if(file.dataVersion.HasValue) final.Add("DataVersion", file.dataVersion.Value);
				final.Add("data", file.contents);
			}
			else
			{
				final = file.contents;
			}
			var stream = new MemoryStream();
			Write(stream, "", final);
			return stream;
		}

		///<summary>Finds and reads the heightmap data stored in a chunk NBT structure.</summary>
		public static short[,] GetHeightmapFromChunkNBT(NBTFile file, HeightmapType type, GameVersion version, Dimension parent)
		{
			try
			{
				if(file.contents.Contains("Heightmaps"))
				{
					//It's the "new" format
					//TODO: deal with 1.17's new heightmaps
					NBTCompound hmcomp = file.contents.GetAsCompound("Heightmaps");
					if(type == HeightmapType.SolidBlocksNoLiquid && hmcomp.Contains("OCEAN_FLOOR"))
					{
						//The highest non-air block, solid block
						return GetHeightmap((long[])hmcomp.Get("OCEAN_FLOOR"), version, parent?.dimensionID == DimensionID.Overworld);
					}
					else if(type == HeightmapType.SolidBlocks && hmcomp.Contains("MOTION_BLOCKING"))
					{
						//The highest block that blocks motion or contains a fluid
						return GetHeightmap((long[])hmcomp.Get("MOTION_BLOCKING"), version, parent?.dimensionID == DimensionID.Overworld);
					}
					else if(hmcomp.Contains("WORLD_SURFACE"))
					{
						//The highest non-air block
						return GetHeightmap((long[])hmcomp.Get("WORLD_SURFACE"), version, parent?.dimensionID == DimensionID.Overworld);
					}
					else
					{
						return null;
					}
				}
				else if(file.contents.Contains("HeightMap"))
				{
					//It's the old, simple format
					byte[] hmbytes = (byte[])file.contents.Get("HeightMap");
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
		public static short[,] GetHeightmap(long[] hmlongs, GameVersion gameVersion, bool overworld)
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
							s += BitUtils.ByteToBinary(bytes[j], true);
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
							hmbits += BitUtils.ByteToBinary(bytes[j], true);
						}
					}
				}
				short[] hmap = new short[256];
				for(int i = 0; i < 256; i++)
				{
					hmap[i] = (short)BitUtils.Read9BitValue(hmbits, i);
				}

				/*for(int i = 0; i < 256; i++) {
					hmap[i] = (short)BitUtils.Read9BitValue(hmbits, i);
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

				//Lower heightmap by 65 blocks in case of >= 1.17
				//TODO: check for custom height overrides
				if(gameVersion >= GameVersion.Release_1(17) && overworld)
				{
					for(int z = 0; z < 16; z++)
					{
						for(int x = 0; x < 16; x++)
						{
							hm[x, z] -= 65;
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

		private static NBTTag RegisterTag(Stream stream, INBTContainer c, NBTTag expectedTag = NBTTag.UNSPECIFIED)
		{
			NBTTag tag;
			if(expectedTag == NBTTag.UNSPECIFIED)
			{
				tag = (NBTTag)ReadNext(stream);
			}
			else
			{
				tag = expectedTag;
			}
			object value;
			if(tag != NBTTag.TAG_End)
			{
				string name = "";
				if(expectedTag == NBTTag.UNSPECIFIED)
				{
					short nameLength = BitConverter.ToInt16(ReadNext(stream, 2), 0);
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
					throw new ArgumentException("Unrecognized NBT tag: " + tag);
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
			return tag;
		}

		private static NBTList GetList(NBTTag tag, int length, Stream stream)
		{
			NBTList arr = new NBTList(tag);
			for(int j = 0; j < length; j++)
			{
				RegisterTag(stream, arr, tag);
			}
			return arr;
		}

		private static T Get<T>(Stream stream)
		{
			object ret = null;
			if(typeof(T) == typeof(byte))
			{
				ret = ReadNext(stream);
			}
			else if(typeof(T) == typeof(short))
			{
				ret = BitConverter.ToInt16(ReadNext(stream, 2), 0);
			}
			else if(typeof(T) == typeof(int))
			{
				ret = BitConverter.ToInt32(ReadNext(stream, 4), 0);
			}
			else if(typeof(T) == typeof(long))
			{
				ret = BitConverter.ToInt64(ReadNext(stream, 8), 0);
			}
			else if(typeof(T) == typeof(float))
			{
				ret = BitConverter.ToSingle(ReadNext(stream, 4), 0);
			}
			else if(typeof(T) == typeof(double))
			{
				ret = BitConverter.ToDouble(ReadNext(stream, 8), 0);
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

		private static NBTList GenericListToNBTList(object obj)
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

		private static byte ReadNext(Stream stream)
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

		private static byte[] ReadNext(Stream stream, int count, bool bigEndian = true)
		{
			byte[] b = new byte[count];
			for(int i = 0; i < count; i++)
			{
				b[i] = ReadNext(stream);
			}
			if(bigEndian) BitUtils.ToBigEndian(b);
			return b;
		}

		private static void Write(MemoryStream stream, string name, object o)
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
			stream.WriteByte((byte)tag);
			byte[] nameBytes = Encoding.UTF8.GetBytes(name);
			byte[] lengthBytes = BitUtils.ToBigEndian(BitConverter.GetBytes((short)nameBytes.Length));
			stream.Write(lengthBytes, 0, lengthBytes.Length);
			stream.Write(nameBytes, 0, nameBytes.Length);
			WriteValue(stream, tag, o);
		}

		private static void WriteValue(MemoryStream stream, NBTTag tag, object o)
		{
			if(tag == NBTTag.TAG_Byte)
			{
				if(o is byte b) stream.WriteByte(b);
				else if(o is sbyte s) unchecked { stream.WriteByte((byte)s); }
				else stream.WriteByte(Convert.ToByte(o));
			}
			else if(tag == NBTTag.TAG_Short)
			{
				WriteToStream(stream, BitUtils.ToBigEndian(BitConverter.GetBytes((short)o)));
			}
			else if(tag == NBTTag.TAG_Int)
			{
				WriteToStream(stream, BitUtils.ToBigEndian(BitConverter.GetBytes((int)o)));
			}
			else if(tag == NBTTag.TAG_Long)
			{
				WriteToStream(stream, BitUtils.ToBigEndian(BitConverter.GetBytes((long)o)));
			}
			else if(tag == NBTTag.TAG_Float)
			{
				WriteToStream(stream, BitUtils.ToBigEndian(BitConverter.GetBytes((float)o)));
			}
			else if(tag == NBTTag.TAG_Double)
			{
				WriteToStream(stream, BitUtils.ToBigEndian(BitConverter.GetBytes((double)o)));
			}
			else if(tag == NBTTag.TAG_Byte_Array)
			{
				WriteValue(stream, NBTTag.TAG_Int, ((byte[])o).Length);
				foreach(byte b in (byte[])o)
				{
					WriteValue(stream, NBTTag.TAG_Byte, b);
				}
			}
			else if(tag == NBTTag.TAG_String)
			{
				byte[] utf8 = Encoding.UTF8.GetBytes((string)o);
				WriteValue(stream, NBTTag.TAG_Short, (short)utf8.Length);
				WriteToStream(stream, utf8);
			}
			else if(tag == NBTTag.TAG_List)
			{
				NBTList list = (NBTList)o;
				stream.WriteByte((byte)list.ContentsType);
				WriteValue(stream, NBTTag.TAG_Int, list.listContent.Count);
				foreach(object item in list.listContent)
				{
					WriteValue(stream, list.ContentsType, item);
				}
			}
			else if(tag == NBTTag.TAG_Compound)
			{
				NBTCompound compound = (NBTCompound)o;
				foreach(string k in compound.contents.Keys)
				{
					Write(stream, k, compound.contents[k]);
				}
				stream.WriteByte((byte)NBTTag.TAG_End);
			}
			else if(tag == NBTTag.TAG_Int_Array)
			{
				WriteValue(stream, NBTTag.TAG_Int, ((int[])o).Length);
				foreach(var item in (int[])o)
				{
					WriteValue(stream, NBTTag.TAG_Int, item);
				}
			}
			else if(tag == NBTTag.TAG_Long_Array)
			{
				WriteValue(stream, NBTTag.TAG_Int, ((long[])o).Length);
				foreach(var item in (long[])o)
				{
					WriteValue(stream, NBTTag.TAG_Long, item);
				}
			}
		}

		private static void WriteToStream(MemoryStream stream, byte[] bytes)
		{
			stream.Write(bytes, 0, bytes.Length);
		}
	}
}
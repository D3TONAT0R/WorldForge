using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MCUtils
{
	public class NBTContent
	{

		public abstract class Container
		{

			public Container()
			{

			}

			public abstract NBTTag containerType
			{
				get;
			}

			public abstract T Add<T>(string key, T value);

			public abstract string[] GetContentKeys(string prefix);

			public abstract object Get(string key);

			public override string ToString()
			{
				return containerType.ToString();
			}
		}

		///<summary>A container for the TAG_Compound tag.</summary>
		public class CompoundContainer : Container
		{

			public override NBTTag containerType
			{
				get { return NBTTag.TAG_Compound; }
			}

			public Dictionary<string, object> cont;

			public CompoundContainer() : base()
			{
				cont = new Dictionary<string, object>();
			}

			public override T Add<T>(string key, T value)
			{
				if (!cont.ContainsKey(key))
				{
					cont.Add(key, value);
				}
				else
				{
					cont[key] = value;
				}
				return value;
			}

			public void AddRange(params (string key, object obj)[] values)
			{
				foreach (var value in values)
				{
					Add(value.key, value.obj);
				}
			}

			public CompoundContainer AddCompound(string key)
			{
				return Add(key, new CompoundContainer());
			}

			public ListContainer AddList(string key, NBTTag tag)
			{
				return Add(key, new ListContainer(tag));
			}

			public override object Get(string key)
			{
				if (cont.ContainsKey(key))
				{
					return cont[key];
				}
				else
				{
					MCUtilsConsole.WriteError("Key '" + key + "' does not exist!");
					return null;
				}
			}

			public T Get<T>(string key)
			{
				return (T)Convert.ChangeType(Get(key), typeof(T));
			}

			public bool Contains(string key)
			{
				return cont.ContainsKey(key);
			}

			public CompoundContainer GetAsCompound(string key)
			{
				return Get(key) as CompoundContainer;
			}

			public ListContainer GetAsList(string key)
			{
				return Get(key) as ListContainer;
			}

			public static bool AreEqual(CompoundContainer a, CompoundContainer b)
			{
				if (a == null && b == null) return true;
				if ((a == null) != (b == null)) return false;
				return a.HasSameContent(b);
			}

			public bool HasSameContent(CompoundContainer other)
			{
				if (other.cont.Keys.Count != cont.Keys.Count) return false;
				foreach (string k in cont.Keys)
				{
					if (!other.Contains(k)) return false;
					if (!cont[k].Equals(other.cont[k])) return false;
				}
				return true;
			}

			public override string[] GetContentKeys(string prefix)
			{
				string[] k = new string[cont.Count];
				int i = 0;
				foreach (var key in cont.Keys)
				{
					k[i] = prefix + key;
					i++;
				}
				return k;
			}
		}

		///<summary>A container for the TAG_List tag.</summary>
		public class ListContainer : Container
		{

			public override NBTTag containerType => NBTTag.TAG_List;

			public NBTTag contentsType;
			public int Length
			{
				get
				{
					return cont.Count;
				}
			}
			public List<object> cont;

			public ListContainer(NBTTag baseType) : base()
			{
				contentsType = baseType;
				cont = new List<object>();
			}

			public override object Get(string key)
			{
				return this[int.Parse(key)];
			}

			public T Get<T>(int index)
			{
				return (T)Convert.ChangeType(this[index], typeof(T));
			}

			public override T Add<T>(string key, T value)
			{
				cont.Add(value);
				return value;
			}

			public T Add<T>(T value)
			{
				return Add(null, value);
			}

			public void AddRange(params object[] values)
			{
				foreach (var value in values)
				{
					Add(null, value);
				}
			}

			public object this[int i]
			{
				get { return cont[i]; }
				set { cont[i] = value; }
			}

			public override string[] GetContentKeys(string prefix)
			{
				string[] k = new string[cont.Count];
				for (int i = 0; i < cont.Count; i++) k[i] = prefix + i.ToString();
				return k;
			}
		}

		public enum NBTTag
		{
			TAG_End = 0,
			TAG_Byte = 1,
			TAG_Short = 2,
			TAG_Int = 3,
			TAG_Long = 4,
			TAG_Float = 5,
			TAG_Double = 6,
			TAG_Byte_Array = 7,
			TAG_String = 8,
			TAG_List = 9,
			TAG_Compound = 10,
			TAG_Int_Array = 11,
			TAG_Long_Array = 12,
			UNSPECIFIED = 99
		}

		public static Dictionary<Type, NBTTag> NBTTagDictionary = new Dictionary<Type, NBTTag> {
				{ typeof(byte), NBTTag.TAG_Byte },
				{ typeof(short), NBTTag.TAG_Short },
				{ typeof(int), NBTTag.TAG_Int },
				{ typeof(long), NBTTag.TAG_Long },
				{ typeof(float), NBTTag.TAG_Float },
				{ typeof(double), NBTTag.TAG_Double },
				{ typeof(byte[]), NBTTag.TAG_Byte_Array },
				{ typeof(string), NBTTag.TAG_String },
				{ typeof(ListContainer), NBTTag.TAG_List },
				{ typeof(CompoundContainer), NBTTag.TAG_Compound },
				{ typeof(int[]), NBTTag.TAG_Int_Array },
				{ typeof(long[]), NBTTag.TAG_Long_Array }
			};

		///<summary>The compound container containing all stored data</summary>
		public CompoundContainer contents;

		///<summary>
		///The version number this nbt compound was created with (only valid for versions release 1.9 and up)
		///</summary>
		public int? dataVersion;

		List<Container> parentTree;

		///<summary>Instantiates an empty NBT structure.</summary>
		public NBTContent()
		{
			contents = new CompoundContainer();
			parentTree = new List<Container>();
		}

		///<summary>Creates an NBT structure from the given bytes.</summary>
		public NBTContent(Stream uncompressedStream, bool isChunkFile) : this()
		{
			while (uncompressedStream.Position < uncompressedStream.Length)
			{
				RegisterTag(uncompressedStream, contents);
			}
			var root = contents.GetAsCompound("");
			if (root != null)
			{
				//Remove the unnessecary root compound and unpack the level compound
				foreach (string k in root.cont.Keys)
				{
					contents.Add(k, root.cont[k]);
				}
				contents.cont.Remove("");
			}
			if (contents.Contains("DataVersion")) dataVersion = (int)contents.Get("DataVersion");
			if (contents.Contains("Level"))
			{
				var level = contents.GetAsCompound("Level");
				foreach (string k in level.cont.Keys)
				{
					contents.Add(k, level.cont[k]);
				}
				contents.cont.Remove("Level");
			}
			//Program.writeLine("NBT Loaded!");
			/*if(isChunkFile) {
				var chunk = new ChunkData(null, this);
			}*/
		}

		///<summary>Generates a byte array from the content of this NBT structure.</summary>
		public void WriteToBytes(List<byte> bytes, bool addStandardLevelCompound)
		{
			if (addStandardLevelCompound)
			{
				//Repackage into the original structure

				CompoundContainer root = new CompoundContainer();
				CompoundContainer level = new CompoundContainer();
				foreach (string k in contents.cont.Keys)
				{
					level.Add(k, contents.Get(k));
				}
				root.Add("Level", level);
				root.Add("DataVersion", 2566);
				Write(bytes, "", root);
			}
			else
			{
				Write(bytes, "", contents);
			}
		}

		///<summary>Finds and reads the heightmap data stored in a chunk NBT structure.</summary>
		public short[,] GetHeightmapFromChunkNBT(HeightmapType type)
		{
			try
			{
				if (contents.Contains("Heightmaps"))
				{
					//It's the "new" format
					//TODO: deal with 1.17's new heightmaps
					CompoundContainer hmcomp = contents.GetAsCompound("Heightmaps");
					if (type == HeightmapType.SolidBlocksNoLiquid && hmcomp.Contains("OCEAN_FLOOR"))
					{
						//The highest non-air block, solid block
						return GetHeightmap((long[])hmcomp.Get("OCEAN_FLOOR"));
					}
					else if (type == HeightmapType.SolidBlocks && hmcomp.Contains("MOTION_BLOCKING"))
					{
						//The highest block that blocks motion or contains a fluid
						return GetHeightmap((long[])hmcomp.Get("MOTION_BLOCKING"));
					}
					else if (hmcomp.Contains("WORLD_SURFACE"))
					{
						//The highest non-air block
						return GetHeightmap((long[])hmcomp.Get("WORLD_SURFACE"));
					}
					else
					{
						return null;
					}
				}
				else if (contents.Contains("HeightMap"))
				{
					//It's the old, simple format
					byte[] hmbytes = (byte[])contents.Get("HeightMap");
					short[,] hm = new short[16, 16];
					for (int z = 0; z < 16; z++)
					{
						for (int x = 0; x < 16; x++)
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
			if (hmlongs == null) return null;
			short[,] hm = new short[16, 16];
			try
			{
				string hmbits = "";
				if (hmlongs.Length == 37)
				{
					//1.16 format
					for (int i = 0; i < 37; i++)
					{
						byte[] bytes = BitConverter.GetBytes(hmlongs[i]);
						string s = "";
						for (int j = 0; j < 8; j++)
						{
							s += Converter.ByteToBinary(bytes[j], true);
						}
						hmbits += s.Substring(0, 63); //Remove the last unused bit
					}
				}
				else
				{
					//pre 1.16 "full bit range" format
					for (int i = 0; i < 36; i++)
					{
						byte[] bytes = BitConverter.GetBytes(hmlongs[i]);
						for (int j = 0; j < 8; j++)
						{
							hmbits += Converter.ByteToBinary(bytes[j], true);
						}
					}
				}
				short[] hmap = new short[256];
				for (int i = 0; i < 256; i++)
				{
					hmap[i] = (short)Converter.Read9BitValue(hmbits, i);
				}

				/*for(int i = 0; i < 256; i++) {
					hmap[i] = (short)Converter.Read9BitValue(hmbits, i);
				}*/
				if (hmbits != null)
				{
					for (int z = 0; z < 16; z++)
					{
						for (int x = 0; x < 16; x++)
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
			if (r >= 0)
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
			for (int i = 0; i < count; i++)
			{
				b[i] = ReadNext(stream);
			}
			return b;
		}

		NBTTag RegisterTag(Stream stream, Container c, NBTTag predef = NBTTag.UNSPECIFIED)
		{
			NBTTag tag;
			/*if(compound.GetType() == typeof(ListContainer)) {
				tag = ((ListContainer)compound).containerType;
				i++;
			} else {*/
			if (predef == NBTTag.UNSPECIFIED)
			{
				tag = (NBTTag)ReadNext(stream);
			}
			else
			{
				tag = predef;
			}
			//}
			object value = null;
			if (tag != NBTTag.TAG_End)
			{
				string name = "";
				if (predef == NBTTag.UNSPECIFIED)
				{
					short nameLength = BitConverter.ToInt16(Converter.ReverseEndianness(ReadNext(stream, 2)), 0);
					if (nameLength > 64)
					{
						Console.WriteLine("NL=" + nameLength + "! Something is going wrong");
					}
					for (int j = 0; j < nameLength; j++)
					{
						//TODO: Reading List with TAG_End throws IndexOutOfRangeException
						name += (char)ReadNext(stream);
					}
				}
				/*if(name == "MOTION_BLOCKING" || name == "MOTION_BLOCKING_NO_LEAVES" || name == "OCEAN_FLOOR" || name == "WORLD_SURFACE") {
					Get<int>(data, ref i); //Throw away the length int, it's always 36
					value = GetHeightmap(data, c, ref i);
				} else */
				if (tag == NBTTag.TAG_Byte)
				{
					value = Get<byte>(stream);
				}
				else if (tag == NBTTag.TAG_Short)
				{
					value = Get<short>(stream);
				}
				else if (tag == NBTTag.TAG_Int)
				{
					value = Get<int>(stream);
				}
				else if (tag == NBTTag.TAG_Long)
				{
					value = Get<long>(stream);
				}
				else if (tag == NBTTag.TAG_Float)
				{
					value = Get<float>(stream);
				}
				else if (tag == NBTTag.TAG_Double)
				{
					value = Get<double>(stream);
				}
				else if (tag == NBTTag.TAG_Byte_Array)
				{
					value = Get<byte[]>(stream);
				}
				else if (tag == NBTTag.TAG_String)
				{
					value = Get<string>(stream);
				}
				else if (tag == NBTTag.TAG_List)
				{
					value = Get<ListContainer>(stream);
				}
				else if (tag == NBTTag.TAG_Compound)
				{
					value = Get<CompoundContainer>(stream);
				}
				else if (tag == NBTTag.TAG_Int_Array)
				{
					value = Get<int[]>(stream);
				}
				else if (tag == NBTTag.TAG_Long_Array)
				{
					value = Get<long[]>(stream);
				}
				else
				{
					throw new ArgumentException("Unrecognized nbt tag: " + tag);
				}
				c.Add(name, value);
				LogTree(tag, name, value);
			}
			else
			{

				//ExitContainer();
			}
			return tag;
		}

		ListContainer GetList(NBTTag tag, int length, Stream stream)
		{
			ListContainer arr = new ListContainer(tag);
			//compound = EnterContainer(compound, arr);
			for (int j = 0; j < length; j++)
			{
				RegisterTag(stream, arr, tag);
			}
			//compound = ExitContainer();
			return arr;
		}

		void LogTree(NBTTag tag, string name, object value)
		{
			string tree = "";
			for (int t = 0; t < parentTree.Count; t++)
			{
				tree += " > ";
			}
			if (name == "") name = "[LIST_ENTRY]";
			string vs = value != null ? value.ToString() : "-";
			if (vs.Length > 64) vs = vs.Substring(0, 60) + "[...]";
			tree += tag != NBTTag.TAG_End ? name + ": " + tag.ToString() + " = " + vs : "END";
			//Program.writeLine(tree);
		}

		T Get<T>(Stream stream)
		{
			object ret = null;
			if (typeof(T) == typeof(byte))
			{
				ret = ReadNext(stream);
			}
			else if (typeof(T) == typeof(short))
			{
				ret = BitConverter.ToInt16(Converter.ReverseEndianness(ReadNext(stream, 2)), 0);
			}
			else if (typeof(T) == typeof(int))
			{
				ret = BitConverter.ToInt32(Converter.ReverseEndianness(ReadNext(stream, 4)), 0);
			}
			else if (typeof(T) == typeof(long))
			{
				ret = BitConverter.ToInt64(Converter.ReverseEndianness(ReadNext(stream, 8)), 0);
			}
			else if (typeof(T) == typeof(float))
			{
				ret = BitConverter.ToSingle(Converter.ReverseEndianness(ReadNext(stream, 4)), 0);
			}
			else if (typeof(T) == typeof(double))
			{
				ret = BitConverter.ToDouble(Converter.ReverseEndianness(ReadNext(stream, 8)), 0);
			}
			else if (typeof(T) == typeof(byte[]))
			{
				int len = Get<int>(stream);
				byte[] arr = new byte[len];
				for (int j = 0; j < len; j++)
				{
					arr[j] = Get<byte>(stream);
				}
				ret = arr;
			}
			else if (typeof(T) == typeof(string))
			{
				int len = Get<short>(stream);
				byte[] arr = new byte[len];
				for (int j = 0; j < len; j++)
				{
					arr[j] = Get<byte>(stream);
				}
				ret = Encoding.UTF8.GetString(arr);
			}
			else if (typeof(T) == typeof(ListContainer))
			{
				NBTTag type = (NBTTag)Get<byte>(stream);
				int len = Get<int>(stream);
				ret = GetList(type, len, stream);
			}
			else if (typeof(T) == typeof(CompoundContainer))
			{
				var newCompound = new CompoundContainer();
				while (RegisterTag(stream, newCompound) != NBTTag.TAG_End)
				{

				}
				ret = newCompound;
			}
			else if (typeof(T) == typeof(int[]))
			{
				int len = Get<int>(stream);
				int[] arr = new int[len];
				for (int j = 0; j < len; j++)
				{
					arr[j] = Get<int>(stream);
				}
				ret = arr;
			}
			else if (typeof(T) == typeof(long[]))
			{
				int len = Get<int>(stream);
				long[] arr = new long[len];
				for (int j = 0; j < len; j++)
				{
					arr[j] = Get<long>(stream);
				}
				ret = arr;
			}
			return (T)Convert.ChangeType(ret, typeof(T));
		}

		void Write(List<byte> bytes, string name, object o)
		{
			var tag = NBTTagDictionary[o.GetType()];
			bytes.Add((byte)tag);
			byte[] nameBytes = Encoding.UTF8.GetBytes(name);
			byte[] lengthBytes = Converter.ReverseEndianness(BitConverter.GetBytes((short)nameBytes.Length));
			bytes.AddRange(lengthBytes);
			bytes.AddRange(nameBytes);
			WriteValue(bytes, tag, o);
		}

		void WriteValue(List<byte> bytes, NBTTag tag, object o)
		{
			if (tag == NBTTag.TAG_Byte)
			{
				bytes.Add((byte)o);
			}
			else if (tag == NBTTag.TAG_Short)
			{
				bytes.AddRange(Converter.ReverseEndianness(BitConverter.GetBytes((short)o)));
			}
			else if (tag == NBTTag.TAG_Int)
			{
				bytes.AddRange(Converter.ReverseEndianness(BitConverter.GetBytes((int)o)));
			}
			else if (tag == NBTTag.TAG_Long)
			{
				bytes.AddRange(Converter.ReverseEndianness(BitConverter.GetBytes((long)o)));
			}
			else if (tag == NBTTag.TAG_Float)
			{
				bytes.AddRange(Converter.ReverseEndianness(BitConverter.GetBytes((float)o)));
			}
			else if (tag == NBTTag.TAG_Double)
			{
				bytes.AddRange(Converter.ReverseEndianness(BitConverter.GetBytes((double)o)));
			}
			else if (tag == NBTTag.TAG_Byte_Array)
			{
				WriteValue(bytes, NBTTag.TAG_Int, ((byte[])o).Length);
				foreach (byte b in (byte[])o)
				{
					WriteValue(bytes, NBTTag.TAG_Byte, b);
				}
			}
			else if (tag == NBTTag.TAG_String)
			{
				byte[] utf8 = Encoding.UTF8.GetBytes((string)o);
				WriteValue(bytes, NBTTag.TAG_Short, (short)utf8.Length);
				bytes.AddRange(utf8);
			}
			else if (tag == NBTTag.TAG_List)
			{
				ListContainer list = (ListContainer)o;
				bytes.Add((byte)list.contentsType);
				WriteValue(bytes, NBTTag.TAG_Int, list.cont.Count);
				foreach (object item in list.cont)
				{
					WriteValue(bytes, list.contentsType, item);
				}
			}
			else if (tag == NBTTag.TAG_Compound)
			{
				CompoundContainer compound = (CompoundContainer)o;
				foreach (string k in compound.cont.Keys)
				{
					Write(bytes, k, compound.cont[k]);
				}
				bytes.Add((byte)NBTTag.TAG_End);
			}
			else if (tag == NBTTag.TAG_Int_Array)
			{
				WriteValue(bytes, NBTTag.TAG_Int, ((int[])o).Length);
				foreach (var item in (int[])o)
				{
					WriteValue(bytes, NBTTag.TAG_Int, item);
				}
			}
			else if (tag == NBTTag.TAG_Long_Array)
			{
				WriteValue(bytes, NBTTag.TAG_Int, ((long[])o).Length);
				foreach (var item in (long[])o)
				{
					WriteValue(bytes, NBTTag.TAG_Long, item);
				}
			}
		}

	}
}
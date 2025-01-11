using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WorldForge.IO;

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

		///<summary>Creates an empty NBT structure.</summary>
		public NBTFile(int? dataVersion = null)
		{
			contents = new NBTCompound();
			this.dataVersion = dataVersion;
		}

		///<summary>Creates an NBT structure with the given data.</summary>
		public NBTFile(NBTCompound contents, int? dataVersion)
		{
			this.contents = contents;
			this.dataVersion = dataVersion;
		}

		///<summary>Creates an NBT structure from the given file.</summary>
		public NBTFile(string filePath) : this()
		{
			using(var stream = Compression.CreateGZipDecompressionStream(File.ReadAllBytes(filePath)))
			{
				NBTSerializer.Deserialize(this, stream);
				PostLoad();
			}
		}

		///<summary>Creates an NBT structure from the given bytes.</summary>
		public NBTFile(Stream uncompressedStream) : this()
		{
			NBTSerializer.Deserialize(this, uncompressedStream);
			PostLoad();
		}

		private void PostLoad()
		{
			//Remove the unnessecary root compound
			if(contents.ItemCount == 1 && contents.TryGet<NBTCompound>("", out var root))
			{
				contents = root;
			}
			if(contents.Contains("DataVersion")) dataVersion = (int)contents.Take("DataVersion");
			//Drop the root compound if the only child is a compound itself
			if(contents.ItemCount == 1 && contents.TryGet<NBTCompound>("data", out var data))
			{
				contents = data;
			}
		}

		///<summary>Writes the content of this NBT structure to a file using Zlib compression.</summary>
		public void Save(string filePath, bool createSubContainer = true)
		{
			File.WriteAllBytes(filePath, NBTSerializer.SerializeAsGzip(this, createSubContainer));
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
	}
}
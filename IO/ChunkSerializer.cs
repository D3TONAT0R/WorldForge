using System;
using System.Collections.Generic;
using System.Text;
using static MCUtils.NBTContent;

namespace MCUtils.IO
{
	public static class ChunkSerializer
	{
		public static NBTContent CreateCompoundForChunk(ChunkData chunk, int chunkX, int chunkZ, Version version)
		{
			var nbt = new NBTContent();
			nbt.dataVersion = version.GetDataVersion();
			nbt.contents.Add("xPos", chunkX);
			nbt.contents.Add("zPos", chunkZ);
			nbt.contents.Add("Status", "light");
			ListContainer sections = new ListContainer(NBTTag.TAG_Compound);
			nbt.contents.Add("Sections", sections);
			nbt.contents.Add("TileEntities", new ListContainer(NBTTag.TAG_Compound));
			nbt.contents.Add("Entities", new ListContainer(NBTTag.TAG_Compound));
			chunk.WriteToNBT(nbt.contents, version);
			//Add the rest of the tags and leave them empty
			nbt.contents.Add("Heightmaps", new CompoundContainer());
			nbt.contents.Add("Structures", new CompoundContainer());
			/*
			nbt.contents.Add("LiquidTicks", new ListContainer(NBTTag.TAG_Compound));
			ListContainer postprocessing = new ListContainer(NBTTag.TAG_List);
			for(int i = 0; i < 16; i++) postprocessing.Add("", new ListContainer(NBTTag.TAG_List));
			nbt.contents.Add("PostProcessing", postprocessing);
			nbt.contents.Add("TileTicks", new ListContainer(NBTTag.TAG_Compound));
			nbt.contents.Add("InhabitedTime", 0L);
			nbt.contents.Add("LastUpdate", 0L);
			*/
			return nbt;
		}
	}
}

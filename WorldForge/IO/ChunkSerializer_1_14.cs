using System.Collections.Generic;
using WorldForge.Chunks;
using WorldForge.NBT;

namespace WorldForge.IO
{
	public class ChunkSerializer_1_14 : ChunkSerializer_1_13
	{
		public override bool SeparatePOIData => true;

		public ChunkSerializer_1_14(GameVersion version) : base(version) { }

		public override void LoadPOIs(ChunkData c, NBTCompound chunkNBT, GameVersion? version)
		{
			//TODO: load POI data
		}

		public override void WritePOIs(ChunkData c, NBTCompound chunkNBT)
		{
			NBTCompound sections = new NBTCompound();
			Dictionary<int, NBTList> recordsPerSection = new Dictionary<int, NBTList>();
			foreach(var poi in c.POIs)
			{
				int sy = poi.position.y & 15;
				if(!recordsPerSection.TryGetValue(sy, out var records))
				{
					records = new NBTList(NBTTag.TAG_Compound);
					recordsPerSection.Add(sy, records);
				}
				records.Add(poi.ToNBT(TargetVersion));
			}
			chunkNBT.Add("Sections", sections);
		}
	}
}

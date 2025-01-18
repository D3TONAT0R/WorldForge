using System.Collections.Generic;
using WorldForge.Chunks;
using WorldForge.NBT;

namespace WorldForge.IO
{
	public class ChunkSerializer_1_14 : ChunkSerializer_1_13
	{
		public override bool SeparatePOIData => true;

		public ChunkSerializer_1_14(GameVersion version) : base(version) { }

		public override void LoadPOIs(Chunk c, NBTCompound chunkNBT, GameVersion? version)
		{
			if(chunkNBT.TryGet("Sections", out NBTCompound sections))
			{
				foreach(var section in sections)
				{
					var sectionComp = (NBTCompound)section.Value;
					if(sectionComp.TryGet("Valid", out bool valid) && !valid) return;
					if(sectionComp.TryGet("Records", out NBTList records))
					{
						foreach(var record in records)
						{
							c.POIs.Add(new POI((NBTCompound)record));
						}
					}
				}
			}
		}

		public override void WritePOIs(Chunk c, NBTCompound chunkNBT)
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

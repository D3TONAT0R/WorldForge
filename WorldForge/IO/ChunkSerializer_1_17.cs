namespace WorldForge.IO
{
	public class ChunkSerializer_1_17 : ChunkSerializer_1_16
	{
		public override bool SeparateEntitiesData => true;

		public ChunkSerializer_1_17(GameVersion version) : base(version) { }
	}
}

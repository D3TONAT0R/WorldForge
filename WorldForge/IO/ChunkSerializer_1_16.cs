namespace WorldForge.IO
{
	public class ChunkSerializer_1_16 : ChunkSerializer_1_15
	{
		public ChunkSerializer_1_16(GameVersion version) : base(version) { }

		public override bool UseFull64BitRange => false;
	}
}

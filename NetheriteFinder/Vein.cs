using WorldForge.Coordinates;

namespace NetheriteFinder
{
	public class Vein
	{
		public BlockCoord pos;
		public int count;

		public Vein(BlockCoord pos, int count)
		{
			this.pos = pos;
			this.count = count;
		}

		public override string ToString()
		{
			return $"{pos} (Vein of {count})";
		}

		public override int GetHashCode()
		{
			return pos.GetHashCode() + count;
		}
	}
}
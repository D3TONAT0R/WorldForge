using System.Drawing;
using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobSlime : Mob
	{
		public const int SMALL_SIZE = 0;
		public const int MEDIUM_SIZE = 1;
		public const int LARGE_SIZE = 3;

		[NBT("Size")]
		public int size = 0;
		[NBT("wasOnGround")]
		public bool wasOnGround = false;

		public MobSlime(NBTCompound compound) : base(compound)
		{

		}

		public MobSlime(Vector3 position, int size) : base("minecraft:slime", position)
		{
			this.size = size;
		}

		protected MobSlime(string id, Vector3 position, int size) : base(id, position)
		{
			this.size = size;
		}
	}
}

using System.Drawing;
using WorldForge.Coordinates;
using WorldForge.NBT;

namespace WorldForge.Entities
{
	public abstract class MobRaidSpawnable : Mob
	{
		[NBT("CanJoinRaid")]
		public bool canJoinRaid = false;
		[NBT("PatrolLeader")]
		public bool patrolLeader = false;
		[NBT("Patrolling")]
		public bool patrolling = false;
		//[NBT("PatrolTarget")]
		public BlockCoord? patrolTarget = null;
		[NBT("RaidId")]
		public int raidId = 0;
		[NBT("Wave")]
		public int wave = 0;

		public MobRaidSpawnable(NBTCompound compound) : base(compound)
		{
			if(compound.TryGet("patrol_target", out int[] pos))
			{
				patrolTarget = new BlockCoord(pos[0], pos[1], pos[2]);
			}
		}

		public MobRaidSpawnable(string id, Vector3 position) : base(id, position)
		{

		}

		public override NBTCompound ToNBT(GameVersion version)
		{
			var comp = base.ToNBT(version);
			if(patrolTarget.HasValue)
			{
				comp.Add("patrol_target", patrolTarget.Value.ToIntArray());
			}
			return comp;
		}
	}
}

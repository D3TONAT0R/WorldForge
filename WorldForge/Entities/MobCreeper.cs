using System;
using System.Collections.Generic;
using System.Text;
using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobCreeper : Mob
	{
		[NBT("ExplosionRadius")]
		public int explosionRadius = 3;
		[NBT("Fuse")]
		public int fuse = 30;
		[NBT("ignited")]
		public bool ignited = false;
		//[NBT("powered")]
		public bool? powered = false;

		public MobCreeper(NBTCompound compound) : base(compound)
		{
			if(compound.TryGet("powered", out bool p))
			{
				powered = p;
			}
		}

		public MobCreeper(Vector3 position) : base("minecraft:creeper", position)
		{

		}

		public override NBTCompound ToNBT(GameVersion version)
		{
			var comp = base.ToNBT(version);
			if(powered.HasValue)
			{
				comp.Add("powered", powered.Value);
			}
			return comp;
		}
	}
}

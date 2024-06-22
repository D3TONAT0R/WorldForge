using System.Collections.Generic;
using WorldForge.Coordinates;
using WorldForge.Items;
using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobWanderingTrader : MobBreedable
	{
		[NBT("DespawnDelay")]
		public int despawnDelay = 48000;
		[NBT("Offers")]
		public NBTCompound offers = null;
		//[NBT("wander_target")]
		public BlockCoord wanderTarget;
		[NBT("Inventory")]
		public List<ItemStack> inventory = null;

		public MobWanderingTrader(NBTCompound compound) : base(compound)
		{
			if(compound.TryGet("wander_target", out int[] pos))
			{
				wanderTarget = new BlockCoord(pos[0], pos[1], pos[2]);
			}
		}

		public MobWanderingTrader(Vector3 position) : base("minecraft:trader_llama", position)
		{

		}

		public override NBTCompound ToNBT(GameVersion version)
		{
			var comp = base.ToNBT(version);
			comp.Add("wander_target", new int[] { wanderTarget.x, wanderTarget.y, wanderTarget.z });
			return comp;
		}
	}
}

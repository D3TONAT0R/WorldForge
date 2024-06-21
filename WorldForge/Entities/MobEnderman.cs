using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobEnderman : Mob
	{
		//[NBT("carriedBlockState")]
		public BlockState carriedBlockState;

		//Angerable data
		[NBT("AngerTime")]
		public int angerTime = 0;
		[NBT("AngryAt")]
		public UUID angryAt = null;

		public MobEnderman(NBTCompound compound) : base(compound)
		{
			if(compound.TryGet("carriedBlockState", out NBTCompound blockNBT))
			{
				carriedBlockState = new BlockState(blockNBT);
			}
		}

		public MobEnderman(Vector3 position) : base("minecraft:enderman", position)
		{

		}

		public override NBTCompound ToNBT(GameVersion version)
		{
			var nbt = base.ToNBT(version);
			if(carriedBlockState != null)
			{
				nbt.Add("carriedBlockState", carriedBlockState.ToNBT());
			}
			return nbt;
		}
	}
}

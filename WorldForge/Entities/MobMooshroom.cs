using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobMooshroom : MobBreedable
	{
		[NBT("EffectDuration")]
		public int effectDuration = 0;
		[NBT("EffectId")]
		public byte effectId = 0;
		[NBT("Type")]
		public string type = "red";

		public MobMooshroom(NBTCompound compound) : base(compound)
		{

		}

		public MobMooshroom(Vector3 position) : base("minecraft:mooshroom", position)
		{

		}
	}
}

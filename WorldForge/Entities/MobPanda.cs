using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobPanda : MobBreedable
	{
		[NBT("MainGene")]
		public string mainGene = "normal";
		[NBT("HiddenGene")]
		public string hiddenGene = "normal";

		public MobPanda(NBTCompound compound) : base(compound)
		{

		}

		public MobPanda(Vector3 position) : base("minecraft:panda", position)
		{

		}
	}
}

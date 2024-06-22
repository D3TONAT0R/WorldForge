using System.Collections.Generic;
using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobFox : MobBreedable
	{
		[NBT("Crouching")]
		public bool crouching = false;
		[NBT("Sitting")]
		public bool sitting = false;
		[NBT("Sleeping")]
		public bool sleeping = false;
		[NBT("Type")]
		public string type = "red";
		[NBT("Trusted")]
		public List<UUID> trustedPlayers = new List<UUID>();

		public MobFox(NBTCompound compound) : base(compound)
		{

		}

		public MobFox(Vector3 position) : base("minecraft:fox", position)
		{

		}
	}
}

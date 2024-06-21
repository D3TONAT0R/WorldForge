using System;
using System.Collections.Generic;
using System.Text;
using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class GenericEntity : Entity
	{
		public GenericEntity(NBTCompound compound) : base(compound)
		{

		}

		public GenericEntity(string id, Vector3 position) : base(id, position)
		{

		}
	}
}

using System;
using System.Collections.Generic;
using System.Text;

namespace MCUtils.NBT
{
	public interface INBTConverter
	{
		object ToNBT(Version version);

		void FromNBT(object nbtData);
	}
}

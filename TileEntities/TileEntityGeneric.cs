using MCUtils.Coordinates;
using MCUtils.NBT;

namespace MCUtils.TileEntities
{
	public class TileEntityGeneric : TileEntity
	{
		public TileEntityGeneric(string id, BlockCoord blockPos) : base(id, blockPos)
		{

		}

		public TileEntityGeneric(NBTCompound compound) : base(compound)
		{

		}

		protected override void Serialize(NBTCompound nbt, Version version)
		{
			
		}
	}
}

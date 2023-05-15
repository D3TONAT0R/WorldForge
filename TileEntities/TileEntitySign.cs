using MCUtils.Coordinates;
using MCUtils.NBT;

namespace MCUtils.TileEntities
{
	public class TileEntitySign : TileEntity
	{

		public TileEntitySign(BlockCoord blockPos) : base("sign", blockPos)
		{

		}

		public TileEntitySign(NBTCompound compound) : base(compound)
		{
		}

		protected override void Serialize(NBTCompound nbt, Version version)
		{
			throw new System.NotImplementedException();
		}
	}
}

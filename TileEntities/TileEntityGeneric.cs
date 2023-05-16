using MCUtils.Coordinates;
using MCUtils.NBT;

namespace MCUtils.TileEntities
{
	public class TileEntityGeneric : TileEntity
	{
		public TileEntityGeneric(string id) : base(id)
		{

		}

		public TileEntityGeneric(NBTCompound compound, out BlockCoord blockPos) : base(compound, out blockPos)
		{

		}

		protected override void Serialize(NBTCompound nbt, Version version)
		{
			
		}

		protected override string ResolveEntityID(Version version)
		{
			if(version >= Version.Release_1(11))
			{
				return id;
			}
			else
			{
				switch(id)
				{
					case "daylight_detector": return "DLDetector";
					case "enchanting_table": return "EnchantTable";
					case "end_portal": return "AirPortal";
					case "ender_chest": return "EnderChest";
					case "flower_pot": return "FlowerPot";
					case "note_block": return "Music";
				}
				return id;
			}
		}
	}
}

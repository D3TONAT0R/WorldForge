using System;
using WorldForge.Coordinates;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public class TileEntitySkulkSensor : TileEntity
	{
		[NBT("last_vibration_frequency")]
		public int lastVibrationFrequency = 0;
		[NBT("listener")]
		public NBTCompound listener = null;

		public TileEntitySkulkSensor() : base("sculk_sensor")
		{
		}

		public TileEntitySkulkSensor(NBTCompound compound, out BlockCoord blockPos) : base(compound, out blockPos)
		{
		}

		protected override void OnWriteToNBT(NBTCompound nbt, GameVersion version)
		{
			
		}
	}
}

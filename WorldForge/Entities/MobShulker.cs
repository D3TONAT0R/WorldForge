using System;
using WorldForge.Coordinates;
using WorldForge.NBT;

namespace WorldForge.Entities
{
	public class MobShulker : Mob
	{
		public enum AttachFace : byte
		{
			Top = 0,
			Bottom = 1,
			North = 2,
			South = 3,
			West = 4,
			East = 5
		}

		public enum ShulkerColor : byte
		{
			White = 0,
			Orange = 1,
			Magenta = 2,
			LightBlue = 3,
			Yellow = 4,
			Lime = 5,
			Pink = 6,
			Gray = 7,
			Silver = 8,
			Cyan = 9,
			Purple = 10,
			Blue = 11,
			Brown = 12,
			Green = 13,
			Red = 14,
			Black = 15,
			Default = 16
		}

		//APX, APY, APZ
		public BlockCoord approximateCoordinate;
		[NBT("AttachFace")]
		public AttachFace attachFace = AttachFace.Top;
		[NBT("Color")]
		public ShulkerColor color = ShulkerColor.Default;
		[NBT("Peek")]
		public byte peek = 0;

		public MobShulker(NBTCompound compound) : base(compound)
		{
			if(compound.Contains("APX"))
			{
				approximateCoordinate = new BlockCoord(compound.Get<int>("APX"), compound.Get<int>("APY"), compound.Get<int>("APZ"));
			}
		}

		public MobShulker(Vector3 position, AttachFace face) : base("minecraft:shulker", position)
		{
			approximateCoordinate = new BlockCoord((int)Math.Floor(position.x), (int)Math.Floor(position.y), (int)Math.Floor(position.z));
			attachFace = face;
		}

		public override NBTCompound ToNBT(GameVersion version)
		{
			var comp = base.ToNBT(version);
			comp.Add("APX", approximateCoordinate.x);
			comp.Add("APY", approximateCoordinate.y);
			comp.Add("APZ", approximateCoordinate.z);
			return comp;
		}
	}
}

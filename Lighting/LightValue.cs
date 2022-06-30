using System;
using System.Collections.Generic;
using System.Text;

namespace MCUtils.Lighting
{
	public struct LightValue
	{
		const byte BL_MASK = 0xF0;
		const byte BL_OFFSET = 4;
		const byte SL_MASK = 0x0F;

		public static readonly LightValue None = new LightValue(0x00);
		public static readonly LightValue FullBright = new LightValue(0xFF);
		public static readonly LightValue FullSkyLight = new LightValue(0x0F);
		public static readonly LightValue FullBlockLight = new LightValue(0xF0);

		public byte rawValue;

		public byte BlockLight
		{
			get => (byte)((rawValue & BL_MASK) >> BL_OFFSET);
			set => rawValue = (byte)((rawValue & SL_MASK) | ((ClampNibble(value) << BL_OFFSET) & BL_MASK));
		}
		public byte SkyLight
		{
			get => (byte)(rawValue & SL_MASK);
			set => rawValue = (byte)((rawValue & BL_MASK) | (ClampNibble(value) & SL_MASK));
		}

		public bool IsDark => rawValue == 0;
		public bool HasBlockLight => BlockLight > 0;
		public bool HasSkyLight => SkyLight > 0;

		public LightValue Attenuated
		{
			get
			{
				var l = new LightValue(rawValue);
				l.BlockLight = (byte)Math.Max(0, l.BlockLight-1);
				l.SkyLight = (byte)Math.Max(0, l.SkyLight - 1);
				return l;
			}
		}

		public LightValue AttenuatedDown
		{
			get
			{
				var l = new LightValue(rawValue);
				l.BlockLight = (byte)Math.Max(0, l.BlockLight - 1);
				if(l.SkyLight < 15) l.SkyLight = (byte)Math.Max(0, l.SkyLight - 1);
				return l;
			}
		}

		public LightValue(byte blockLightN4, byte skyLightN4)
		{
			rawValue = 0;
			BlockLight = blockLightN4;
			SkyLight = skyLightN4;
		}

		public LightValue(byte lights)
		{
			rawValue = lights;
		}

		public void Attenuate()
		{
			if (SkyLight > 0) SkyLight--;
			if (BlockLight > 0) BlockLight--;
		}

		public void DiffuseSkyLight(byte amount)
		{
			if(SkyLight > 0) SkyLight -= amount;
		}

		public bool HasStrongerLightThan(LightValue other)
		{
			return SkyLight > other.SkyLight || BlockLight > other.BlockLight;
		}

		public static LightValue operator -(LightValue a, LightValue b)
		{
			return new LightValue((byte)Math.Max(a.BlockLight - b.BlockLight, 0), (byte)Math.Max(a.SkyLight - b.SkyLight, 0));
		}

		public override string ToString()
		{
			return rawValue.ToString("X2");
		}

		private byte ClampNibble(byte b)
		{
			return Math.Max((byte)0, Math.Min((byte)15, b));
		}
	}
}

using System;
using System.Collections.Generic;
using WorldForge.Coordinates;
using WorldForge.NBT;

namespace WorldForge
{
	public class MapData : INBTConverter
	{
		public class BannerMarker : INBTConverter
		{
			public BlockCoord pos;
			public ColorType? color;
			public JSONTextComponent name;

			public BannerMarker(BlockCoord pos, ColorType? color = null, JSONTextComponent name = null)
			{
				this.pos = pos;
				this.color = color;
				this.name = name;
			}

			public object ToNBT(GameVersion version)
			{
				var comp = new NBTCompound();
				comp.Add("pos", pos.ToIntArray());
				if(color != null) comp.Add("color", color.Value.ToLowercaseString());
				if(name != null) comp.Add("name", name.ToJSON());
				return comp;
			}

			public void FromNBT(object nbtData)
			{
				var comp = (NBTCompound)nbtData;
				pos = new BlockCoord(comp.Get<int[]>("pos"));
				if(comp.TryGet("color", out string s)) color = s.ParseColorType();
				if(comp.TryGet("name", out string n)) name = JSONTextComponent.Parse(n);
			}
		}

		public class FrameMarker : INBTConverter
		{
			public BlockCoord pos;
			public int entityId;
			public int rotation;

			public FrameMarker(BlockCoord pos, int entityId, int rotation)
			{
				this.pos = pos;
				this.entityId = entityId;
				this.rotation = rotation;
			}

			public object ToNBT(GameVersion version)
			{
				return new NBTCompound()
				{
					{ "pos", pos.ToIntArray() },
					{ "entity_id", entityId },
					{ "rotation", rotation }
				};
			}

			public void FromNBT(object nbtData)
			{
				var comp = (NBTCompound)nbtData;
				pos = new BlockCoord(comp.Get<int[]>("pos"));
				comp.TryGet("entity_id", out entityId);
				comp.TryGet("rotation", out rotation);
			}
		}

		[NBT]
		public readonly byte[] colors = new byte[16384];
		[NBT]
		public List<BannerMarker> banners = new List<BannerMarker>();
		[NBT]
		public List<FrameMarker> markers = new List<FrameMarker>();

		//[NBT]
		public DimensionID dimension;
		[NBT]
		public int xCenter;
		[NBT]
		public int zCenter;

		[NBT]
		public bool locked = false;
		[NBT]
		public byte scale = 0;
		[NBT]
		public bool trackingPosition = true;
		[NBT]
		public bool unlimitedTracking = false;

		public MapData(DimensionID dimension, int xCenter, int zCenter)
		{
			this.dimension = dimension;
			this.xCenter = xCenter;
			this.zCenter = zCenter;
		}

		public byte GetColorIndex(int x, int z)
		{
			return colors[GetArrayIndex(x, z)];
		}

		public void SetColorIndex(int x, int z, byte color)
		{
			colors[GetArrayIndex(x, z)] = color;
		}

		private int GetArrayIndex(int x, int z)
		{
			if(x < 0 || x > 127 || z < 0 || z > 127) throw new IndexOutOfRangeException();
			return z * 128 + x;
		}

		public object ToNBT(GameVersion version)
		{
			var comp = NBTConverter.WriteToNBT(this, new NBTCompound(), version);
			if(version >= GameVersion.Release_1(16)) comp.Add("dimension", dimension.ID);
			else if(dimension.DimensionIndex.HasValue) comp.Add("dimension", dimension.DimensionIndex ?? -2);
			return comp;
		}

		public void FromNBT(object nbtData)
		{
			var comp = (NBTCompound)nbtData;
			NBTConverter.LoadFromNBT(comp, this);
			if(comp.TryGet("dimension", out int i))
			{
				if(i >= -1 && i <= 1) dimension = DimensionID.FromIndex(i);
				else dimension = DimensionID.Unknown;
			}
			else if(comp.TryGet("dimension", out string s))
			{
				dimension = new DimensionID(s);
			}
		}
	}
}

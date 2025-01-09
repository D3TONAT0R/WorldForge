﻿using System;
using System.Collections.Generic;
using System.IO;
using WorldForge.Coordinates;
using WorldForge.NBT;

namespace WorldForge.Maps
{
	//TODO: saved maps crash the game when loading the world
	public class MapData
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

		public static MapData Load(NBTFile file)
		{
			var data = new MapData(DimensionID.Unknown, 0, 0);
			var comp = file.contents;
			NBTConverter.LoadFromNBT(comp, data);
			if(comp.TryGet("dimension", out int i))
			{
				if(i >= -1 && i <= 1) data.dimension = DimensionID.FromIndex(i);
				else data.dimension = DimensionID.Unknown;
			}
			else if(comp.TryGet("dimension", out string s))
			{
				data.dimension = new DimensionID(s);
			}
			return data;
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

		public void Save(string path, GameVersion targetVersion, bool allowOverwrite = true)
		{
			if(File.Exists(path) && !allowOverwrite) throw new IOException("File already exists: " + path);
			var file = new NBTFile();
			var dv = targetVersion.GetDataVersion();
			if(dv.HasValue) file.contents.Add("DataVersion", dv);
			file.contents.Add("data", ToNBT(GameVersion.LastSupportedVersion));
			file.SaveToFile(path);
		}

		public void SaveMapFile(string worldRoot, int mapId, GameVersion targetVersion, bool allowOverwrite = true)
		{
			if(!Directory.Exists(worldRoot)) throw new DirectoryNotFoundException("World root directory not found");
			Directory.CreateDirectory(Path.Combine(worldRoot, "data"));
			Save(Path.Combine(worldRoot, "data", $"map_{mapId}.dat"), targetVersion, allowOverwrite);
		}

		public NBTCompound ToNBT(GameVersion version)
		{
			var comp = NBTConverter.WriteToNBT(this, new NBTCompound(), version);
			DimensionID dim = dimension.Exists ? dimension : DimensionID.Overworld;
			if(version >= GameVersion.Release_1(16)) comp.Add("dimension", dim.ID);
			else if(dim.DimensionIndex.HasValue) comp.Add("dimension", dim.DimensionIndex ?? -2);
			return comp;
		}
	}
}

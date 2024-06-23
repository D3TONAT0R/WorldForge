using System;
using System.Collections.Generic;
using WorldForge.Coordinates;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public class TileEntityBanner : TileEntity
	{
		public abstract class PatternResource
		{
			public abstract object ToNBT(GameVersion version);

			public static PatternResource CreateFromNBT(object p)
			{
				if(p is string s) return new PatternResourceRef(s);
				else if(p is NBTCompound n) return new PatternResourceAsset(n);
				else throw new ArgumentException();
			}
		}

		public class PatternResourceRef : PatternResource
        {
            public string id = "";

			public PatternResourceRef(string nbtData)
			{
				id = nbtData;
			}

			public override object ToNBT(GameVersion version)
			{
				return id;
			}
		}

		public class PatternResourceAsset : PatternResource
		{
			[NBT("asset_id")]
			public string assetId = "";
			[NBT("translation_key")]
			public string translationKey = "";

			public PatternResourceAsset(NBTCompound nbtData)
			{
				NBTConverter.LoadFromNBT(nbtData, this);
			}

			public override object ToNBT(GameVersion version)
			{
				return NBTConverter.WriteToNBT(this, new NBTCompound(), version);
			}
		}


		public class Pattern : INBTConverter
		{
			public ColorInt color = ColorInt.White;
			public PatternResource pattern = null;

			public void FromNBT(object nbtData)
			{
				var comp = (NBTCompound)nbtData;
				if(comp.TryGet("color", out int c)) color = (ColorInt)c;
				if(comp.Contains("pattern")) pattern = PatternResource.CreateFromNBT(comp.Get("pattern"));
			}

			public object ToNBT(GameVersion version)
			{
				var comp = new NBTCompound();
				comp.Add("color", (int)color);
				if(pattern != null) comp.Add("pattern", pattern.ToNBT(version));
				return comp;
			}
		}

		[NBT("CustomName")]
		public JSONTextComponent customName = null;
		[NBT("patterns")]
		public List<Pattern> patterns = new List<Pattern>();

		public TileEntityBanner() : base("banner")
		{

		}

		public TileEntityBanner(NBTCompound compound, out BlockCoord blockPos) : base(compound, out blockPos)
		{

		}
	}
}

using MCUtils.Coordinates;
using MCUtils.NBT;
using System;

namespace MCUtils.TileEntities
{
	public class TileEntitySign : TileEntity
	{
		public struct SignData
		{
			public string line1;
			public string line2;
			public string line3;
			public string line4;

			//Added in 1.14, defaults to black
			public string color;
			//Added in 1.17, defaults to false
			public bool glowingText;

			public SignData(string line1 = "", string line2 = "", string line3 = "", string line4 = "")
			{
				this.line1 = line1;
				this.line2 = line2;
				this.line3 = line3;
				this.line4 = line4;
				color = "black";
				glowingText = false;
			}

			public void WriteToNBT(NBTCompound nbt, Version version)
			{
				if(version < Version.Release_1(20))
				{
					nbt.Add("Text1", line1 ?? "");
					nbt.Add("Text2", line2 ?? "");
					nbt.Add("Text3", line3 ?? "");
					nbt.Add("Text4", line4 ?? "");
					if(version >= Version.Release_1(14))
					{
						nbt.Add("Color", color);
					}
					if(version >= Version.Release_1(17))
					{
						nbt.Add("GlowingText", glowingText);
					}
				}
				else
				{
					string[] linesJson = new string[4];
					linesJson[0] = $"\"{line1 ?? ""}\"";
					linesJson[1] = $"\"{line2 ?? ""}\"";
					linesJson[2] = $"\"{line3 ?? ""}\"";
					linesJson[3] = $"\"{line4 ?? ""}\"";
					nbt.Add("messages", linesJson);
					nbt.Add("color", color);
					nbt.Add("has_glowing_text", glowingText);
				}
			}

			public void ReadFromNBT(NBTCompound nbt, bool isNewFormat)
			{
				if(!isNewFormat)
				{
					//Pre 1.20 format
					line1 = nbt.Get<string>("Text1");
					line2 = nbt.Get<string>("Text2");
					line3 = nbt.Get<string>("Text3");
					line4 = nbt.Get<string>("Text4");
					nbt.TryGet("Color", out color);
					nbt.TryGet("GlowingText", out glowingText);
				}
				else
				{
					//1.20+ format
					if(nbt.TryGet("messages", out string[] linesJson))
					{
						if(linesJson.Length >= 1) line1 = RemoveJSONQuotes(linesJson[0]);
						if(linesJson.Length >= 2) line1 = RemoveJSONQuotes(linesJson[1]);
						if(linesJson.Length >= 3) line1 = RemoveJSONQuotes(linesJson[2]);
						if(linesJson.Length >= 4) line1 = RemoveJSONQuotes(linesJson[3]);
					}
					nbt.TryGet("color", out color);
					nbt.TryGet("has_glowing_text", out glowingText);
				}
			}

			private string RemoveJSONQuotes(string s)
			{
				if(s.StartsWith("\"")) s = s.Substring(1);
				if(s.EndsWith("\"")) s = s.Substring(0, s.Length - 1);
				return s;
			}
		}

		//Default "front" side of the sign
		public SignData front = new SignData();
		//Added in 1.20
		public SignData back = new SignData();
		//Added in 1.20
		public bool isWaxed;

		public TileEntitySign() : base("sign")
		{

		}

		public TileEntitySign(NBTCompound nbt, out BlockCoord blockPos) : base(nbt, out blockPos)
		{
			bool isNewFormat = nbt.Contains("is_waxed") || nbt.Contains("front_text");
			if(!isNewFormat)
			{
				front.ReadFromNBT(nbt, false);
			}
			else
			{
				nbt.TryTake("is_waxed", out isWaxed);
				if(nbt.TryTake("front_text", out NBTCompound frontNBT))
				{
					front.ReadFromNBT(frontNBT, true);
				}
				if(nbt.TryTake("back_text", out NBTCompound backNBT))
				{
					back.ReadFromNBT(backNBT, true);
				}
			}
		}

		protected override void Serialize(NBTCompound nbt, Version version)
		{
			if(version < Version.Release_1(20))
			{
				front.WriteToNBT(nbt, version);
			}
			else
			{
				nbt.Add("is_waxed", isWaxed);

				var frontNBT = new NBTCompound();
				front.WriteToNBT(frontNBT, version);
				nbt.Add("front_text", frontNBT);

				var backNBT = new NBTCompound();
				front.WriteToNBT(backNBT, version);
				nbt.Add("back_text", backNBT);
			}
		}
	}
}

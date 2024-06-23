using WorldForge.Coordinates;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public partial class TileEntitySign : TileEntity
	{
		public struct SignData
		{
			public JSONTextComponent line1;
			public JSONTextComponent line2;
			public JSONTextComponent line3;
			public JSONTextComponent line4;

			//Added in 1.14, defaults to black
			public string color;
			//Added in 1.17, defaults to false
			public bool glowingText;

			public SignData(string line1Text = "", string line2Text = "", string line3Text = "", string line4Text = "")
			{
				line1 = new JSONTextComponent(line1Text);
				line2 = new JSONTextComponent(line2Text);
				line3 = new JSONTextComponent(line3Text);
				line4 = new JSONTextComponent(line4Text);
				color = "black";
				glowingText = false;
			}

			public void WriteToNBT(NBTCompound nbt, GameVersion version)
			{
				if(version < GameVersion.Release_1(20))
				{
					nbt.Add("Text1", GetSignLineData(line1, version));
					nbt.Add("Text2", GetSignLineData(line2, version));
					nbt.Add("Text3", GetSignLineData(line3, version));
					nbt.Add("Text4", GetSignLineData(line4, version));
					if(version >= GameVersion.Release_1(14))
					{
						nbt.Add("Color", color);
					}
					if(version >= GameVersion.Release_1(17))
					{
						nbt.Add("GlowingText", glowingText);
					}
				}
				else
				{
					string[] linesJson = new string[4];
					linesJson[0] = $"{line1.ToJSON() ?? ""}";
					linesJson[1] = $"{line2.ToJSON() ?? ""}";
					linesJson[2] = $"{line3.ToJSON() ?? ""}";
					linesJson[3] = $"{line4.ToJSON() ?? ""}";
					nbt.Add("messages", linesJson);
					nbt.Add("color", color);
					nbt.Add("has_glowing_text", glowingText);
				}
			}

			private string GetSignLineData(JSONTextComponent text, GameVersion version)
			{
				if(version < GameVersion.Release_1(8))
				{
					return text?.ToLegacyPlainText() ?? "";
				}
				else
				{
					return text.ToJSON() ?? "\"\"";
				}
			}

			public void ReadFromNBT(NBTCompound nbt, bool isNewFormat, GameVersion? version)
			{
				if(version.HasValue && version >= GameVersion.Release_1(20))
				{
					//1.20+ format
					if(nbt.TryGet("messages", out string[] linesJson))
					{
						if(linesJson.Length >= 1) line1 = JSONTextComponent.Parse(linesJson[0]);
						if(linesJson.Length >= 2) line1 = JSONTextComponent.Parse(linesJson[1]);
						if(linesJson.Length >= 3) line1 = JSONTextComponent.Parse(linesJson[2]);
						if(linesJson.Length >= 4) line1 = JSONTextComponent.Parse(linesJson[3]);
					}
					nbt.TryGet("color", out color);
					nbt.TryGet("has_glowing_text", out glowingText);
				}
				else
				{
					//Pre 1.20 format
					//TODO: how to determine pre-1.8 versions? 'version' becomes null prior to 1.9 (due to DataVersion).
					if(version.HasValue && version >= GameVersion.Release_1(8))
					{
						line1 = JSONTextComponent.Parse("Text1");
						line2 = JSONTextComponent.Parse("Text2");
						line3 = JSONTextComponent.Parse("Text3");
						line4 = JSONTextComponent.Parse("Text4");
					}
					else
					{
						line1 = new JSONTextComponent(nbt.Get<string>("Text1"));
						line2 = new JSONTextComponent(nbt.Get<string>("Text2"));
						line3 = new JSONTextComponent(nbt.Get<string>("Text3"));
						line4 = new JSONTextComponent(nbt.Get<string>("Text4"));
					}
					nbt.TryGet("Color", out color);
					nbt.TryGet("GlowingText", out glowingText);
				}
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

		public TileEntitySign(string line1Text = "", string line2Text = "", string line3Text = "", string line4Text = "") : this()
		{
			front = new SignData(line1Text, line2Text, line3Text, line4Text);
		}

		public TileEntitySign(NBTCompound nbt, GameVersion? version, out BlockCoord blockPos) : base(nbt, out blockPos)
		{
			if(version.HasValue && version.Value >= GameVersion.Release_1(20))
			{
				nbt.TryTake("is_waxed", out isWaxed);
				if(nbt.TryTake("front_text", out NBTCompound frontNBT))
				{
					front.ReadFromNBT(frontNBT, true, version);
				}
				if(nbt.TryTake("back_text", out NBTCompound backNBT))
				{
					back.ReadFromNBT(backNBT, true, version);
				}
			}
			else
			{
				front.ReadFromNBT(nbt, false, version);
			}
		}

		protected override void OnWriteToNBT(NBTCompound nbt, GameVersion version)
		{
			if(version < GameVersion.Release_1(20))
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

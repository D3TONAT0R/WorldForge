using MCUtils.Coordinates;
using MCUtils.NBT;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCUtils.TileEntities
{
	public class TileEntitySign : TileEntity
	{
		public class JSONText
		{
			public static Dictionary<string, string> legacyFormattingColorCodes = new Dictionary<string, string>()
			{
				{"black", "§0" },
				{"dark_blue", "§1" },
				{"dark_green", "§2" },
				{"dark_aqua", "§3" },
				{"dark_red", "§4" },
				{"dark_purple", "§5" },
				{"gold", "§6" },
				{"gray", "§7" },
				{"dark_gray", "§8" },
				{"blue", "§9" },
				{"green", "§a" },
				{"aqua", "§b" },
				{"red", "§c" },
				{"light_purple", "§d" },
				{"yellow", "§e" },
				{"white", "§f" }
			};
			public const string legacyObfuscationFormatKey = "§k";
			public const string legacyBoldFormatKey = "§l";
			public const string legacyStrikethroughKey = "§m";
			public const string legacyUnderlinehKey = "§n";
			public const string legacyItalichKey = "§o";
			public const string legacyResetKey = "§r";

			public List<Dictionary<string, object>> data;

			private JSONText()
			{

			}

			public JSONText(string text)
			{
				data = new List<Dictionary<string, object>>
				{
					new Dictionary<string, object>() { { "text", text } }
				};
			}

			public JSONText(string text, string color)
			{
				data = new List<Dictionary<string, object>>
				{
					new Dictionary<string, object>() {
						{ "text", text },
						{ "color", color }
					}
				};
			}

			public static JSONText Parse(string json)
			{
				return new JSONText
				{
					data = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json)
				};
			}

			public string ToJSON()
			{
				if(data.Count == 1 && data[0].Count == 1 && data[0].ContainsKey("text"))
				{
					return $"\"{data[0]["text"]}\"";
				}
				else
				{
					return JArray.FromObject(data).ToString();
				}
			}

			public string ToLegacyPlainText()
			{
				//TODO: recursively check for "text" components
				StringBuilder text = new StringBuilder();
				string lastColorKey = null;
				bool bold = false;
				bool italic = false;
				bool underlined = false;
				bool strikethrough = false;
				bool obfuscated = false;
				List<string> formatKeyQueue = new List<string>();
				foreach(var d in data)
				{

					bool reset = false;
					reset |= ApplyFormatting(formatKeyQueue, d, "bold", legacyBoldFormatKey, ref bold);
					reset |= ApplyFormatting(formatKeyQueue, d, "italic", legacyItalichKey, ref italic);
					reset |= ApplyFormatting(formatKeyQueue, d, "underlined", legacyUnderlinehKey, ref underlined);
					reset |= ApplyFormatting(formatKeyQueue, d, "strikethrough", legacyStrikethroughKey, ref strikethrough);
					reset |= ApplyFormatting(formatKeyQueue, d, "obfuscated", legacyObfuscationFormatKey, ref obfuscated);

					if(reset)
					{
						text.Append(legacyResetKey);
					}

					string newColorKey = null;
					if(d.TryGetValue("color", out var obj))
					{
						var color = obj as string;
						if(!color.StartsWith("#") && legacyFormattingColorCodes.TryGetValue(color, out string key))
						{
							newColorKey = key;
						}
					}
					else
					{
						newColorKey = lastColorKey;
					}

					if(newColorKey != null)
					{
						text.Append(newColorKey);
					}
					lastColorKey = newColorKey;

					if(d.TryGetValue("text", out object s))
					{
						text.Append(s.ToString());
					}
				}
				return text.ToString();
			}

			private bool ApplyFormatting(List<string> queue, Dictionary<string, object> d, string key, string legacyFormatKey, ref bool currentState)
			{
				bool reset = false;
				if(d.TryGetValue(key, out var obj))
				{
					bool b = (string)obj == "true";
					if(b) queue.Add(legacyFormatKey);
					else if(currentState) reset = true;
					currentState = b;
				}
				return reset;
			}
		}

		public struct SignData
		{
			public JSONText line1;
			public JSONText line2;
			public JSONText line3;
			public JSONText line4;

			//Added in 1.14, defaults to black
			public string color;
			//Added in 1.17, defaults to false
			public bool glowingText;

			public SignData(string line1Text = "", string line2Text = "", string line3Text = "", string line4Text = "")
			{
				line1 = new JSONText(line1Text);
				line2 = new JSONText(line2Text);
				line3 = new JSONText(line3Text);
				line4 = new JSONText(line4Text);
				color = "black";
				glowingText = false;
			}

			public void WriteToNBT(NBTCompound nbt, Version version)
			{
				if(version < Version.Release_1(20))
				{
					nbt.Add("Text1", GetSignLineData(line1, version));
					nbt.Add("Text2", GetSignLineData(line2, version));
					nbt.Add("Text3", GetSignLineData(line3, version));
					nbt.Add("Text4", GetSignLineData(line4, version));
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
					linesJson[0] = $"{line1.ToJSON() ?? ""}";
					linesJson[1] = $"{line2.ToJSON() ?? ""}";
					linesJson[2] = $"{line3.ToJSON() ?? ""}";
					linesJson[3] = $"{line4.ToJSON() ?? ""}";
					nbt.Add("messages", linesJson);
					nbt.Add("color", color);
					nbt.Add("has_glowing_text", glowingText);
				}
			}

			private string GetSignLineData(JSONText text, Version version)
			{
				if(version < Version.Release_1(8))
				{
					return text?.ToPlainText() ?? "";
				}
				else
				{
					return text.ToJSON() ?? "\"\"";
				}
			}

			public void ReadFromNBT(NBTCompound nbt, bool isNewFormat, Version? version)
			{
				if(version.HasValue && version >= Version.Release_1(20))
				{
					//1.20+ format
					if(nbt.TryGet("messages", out string[] linesJson))
					{
						if(linesJson.Length >= 1) line1 = JSONText.Parse(linesJson[0]);
						if(linesJson.Length >= 2) line1 = JSONText.Parse(linesJson[1]);
						if(linesJson.Length >= 3) line1 = JSONText.Parse(linesJson[2]);
						if(linesJson.Length >= 4) line1 = JSONText.Parse(linesJson[3]);
					}
					nbt.TryGet("color", out color);
					nbt.TryGet("has_glowing_text", out glowingText);
				}
				else
				{
					//Pre 1.20 format
					//TODO: how to determine pre-1.8 versions? 'version' becomes null prior to 1.9 (due to DataVersion).
					if(version.HasValue && version >= Version.Release_1(8))
					{
						line1 = JSONText.Parse("Text1");
						line2 = JSONText.Parse("Text2");
						line3 = JSONText.Parse("Text3");
						line4 = JSONText.Parse("Text4");
					}
					else
					{
						line1 = new JSONText(nbt.Get<string>("Text1"));
						line2 = new JSONText(nbt.Get<string>("Text2"));
						line3 = new JSONText(nbt.Get<string>("Text3"));
						line4 = new JSONText(nbt.Get<string>("Text4"));
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

		public TileEntitySign(NBTCompound nbt, Version? version, out BlockCoord blockPos) : base(nbt, out blockPos)
		{
			bool isNewFormat = nbt.Contains("is_waxed") || nbt.Contains("front_text");
			if(version.HasValue && version.Value >= Version.Release_1(20))
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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text;
using WorldForge.NBT;

namespace WorldForge
{
	public class JSONTextComponent : INBTConverter
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

		private JSONTextComponent()
		{

		}

		public JSONTextComponent(string text)
		{
			data = new List<Dictionary<string, object>>
			{
				new Dictionary<string, object>() { { "text", text } }
			};
		}

		public JSONTextComponent(string text, string color)
		{
			data = new List<Dictionary<string, object>>
			{
				new Dictionary<string, object>() {
					{ "text", text },
					{ "color", color }
				}
			};
		}

		public static JSONTextComponent Parse(string json)
		{
			return new JSONTextComponent
			{
				data = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json)
			};
		}

		public static JSONTextComponent ParseSingle(string json)
		{
			return new JSONTextComponent
			{
				data = new List<Dictionary<string, object>>()
				{
					JsonConvert.DeserializeObject<Dictionary<string, object>>(json)
				}
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
				bool b;
				if(obj is bool b1) b = b1;
				else if(obj is string s) b = s == "true";
				else b = false;
				if(b) queue.Add(legacyFormatKey);
				else if(currentState) reset = true;
				currentState = b;
			}
			return reset;
		}

		public object ToNBT(GameVersion version)
		{
			return ToJSON();
		}

		public void FromNBT(object nbtData)
		{
			var text = (string)nbtData;
			try
			{
				data = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(text);
			}
			catch(JsonSerializationException)
			{
				data = new List<Dictionary<string, object>>() {
					JsonConvert.DeserializeObject<Dictionary<string, object>>(text)
				};
			}
		}
	}
}

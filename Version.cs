using System;
using System.Collections.Generic;
using System.Text;

namespace MCUtils
{
	public struct Version
	{
		public enum Stage : byte
		{
			Indev = 0,
			Infdev = 1,
			Alpha = 2,
			Beta = 3,
			Release = 4
		}

		public Stage stage;
		public byte major;
		public byte minor;
		public byte patch;

		public static readonly Version FirstVersion = new Version(Stage.Indev, 0, 0, 0);

		public static readonly Version DefaultVersion = Release_1(16);

		public Version(Stage stage, byte major, byte minor, byte patch)
		{
			this.stage = stage;
			this.major = major;
			this.minor = minor;
			this.patch = patch;
		}

		public static Version Alpha_1(byte minor, byte patch = 0)
		{
			return new Version(Stage.Alpha, 1, minor, patch);
		}

		public static Version Beta_1(byte minor, byte patch = 0)
		{
			return new Version(Stage.Beta, 1, minor, patch);
		}

		public static Version Release_1(byte minor, byte patch = 0)
		{
			return new Version(Stage.Release, 1, minor, patch);
		}

		public static Version Parse(string s)
		{
			s = s.Trim().ToLower();
			Stage stage;
			if(char.IsLetter(s[0])) {
				if (s[0] == 'a') stage = Stage.Alpha;
				else if (s[0] == 'b') stage = Stage.Beta;
				else if (s[0] == 'r') stage = Stage.Release;
				else throw new FormatException("Unrecognized stage character: " + s[0]);
				s = s.Substring(1);
			}
			else
			{
				stage = Stage.Release;
			}
			string[] split = s.Split('.');
			byte major = byte.Parse(split[0]);
			byte minor = byte.Parse(split[1]);
			byte patch = split.Length >= 3 ? byte.Parse(split[2]) : (byte)0;
			return new Version(stage, major, minor, patch);
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			if (stage == Stage.Alpha) sb.Append("a");
			else if (stage == Stage.Beta) sb.Append("b");
			sb.Append($"{major}.{minor}.{patch}");
			return sb.ToString();
		}

		public override int GetHashCode()
		{
			return ((byte)stage << 24) + (major << 16) + (minor << 8) + patch;
		}

		public override bool Equals(object obj)
		{
			if(obj is Version v)
			{
				return this == v;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Gets the data version associated with the given version (Warning: only versions after release 1.9 have a data version)
		/// </summary>
		public int? GetDataVersion()
		{
			int? dv = null;
			if(stage == Stage.Release)
			{
				if(major == 1)
				{
					if (minor == 9)
					{
						switch (patch)
						{
							default:
							case 0: dv = 169; break;
							case 1: dv = 175; break;
							case 2: dv = 176; break;
							case 3: dv = 183; break;
							case 4: dv = 184; break;
						}
					}
					if(minor == 10)
					{
						switch(patch)
						{
							default:
							case 0: dv = 510; break;
							case 1: dv = 511; break;
							case 2: dv = 512; break;
						}
					}
					if(minor == 11)
					{
						switch(patch)
						{
							default:
							case 0: dv = 819; break;
							case 1: dv = 921; break;
							case 2: dv = 922; break;
						}
					}
					if(minor == 12)
					{
						switch(patch)
						{
							default:
							case 0: dv = 1139; break;
							case 1: dv = 1240; break;
							case 2: dv = 1343; break;
						}
					}
					if(minor == 13)
					{
						switch(patch)
						{
							default:
							case 0: dv = 1519; break;
							case 1: dv = 1628; break;
							case 2: dv = 1631; break;
						}
					}
					if(minor == 14)
					{
						switch(patch)
						{
							default:
							case 0: dv = 1952; break;
							case 1: dv = 1957; break;
							case 2: dv = 1963; break;
							case 3: dv = 1968; break;
							case 4: dv = 1976; break;
						}
					}
					if(minor == 15)
					{
						switch(patch)
						{
							default:
							case 0: dv = 2225; break;
							case 1: dv = 2227; break;
							case 2: dv = 2230; break;
						}
					}
					if(minor == 16)
					{
						switch(patch)
						{
							default:
							case 0: dv = 2566; break;
							case 1: dv = 2567; break;
							case 2: dv = 2578; break;
							case 3: dv = 2580; break;
							case 4: dv = 2584; break;
							case 5: dv = 2586; break;
						}
					}
					if(minor == 17)
					{
						switch(patch)
						{
							default:
							case 0: dv = 2724; break;
							case 1: dv = 2730; break;
						}
					}
					if(minor == 18)
					{
						switch(patch)
						{
							default:
							case 0: dv = 2860; break;
							case 1: dv = 2865; break;
						}
					}
				}
			}
			return dv;
		}

		public static bool operator ==(Version l, Version r)
		{
			return l.GetHashCode() == r.GetHashCode();
		}

		public static bool operator !=(Version l, Version r)
		{
			return l.GetHashCode() != r.GetHashCode();
		}

		public static bool operator >(Version l, Version r)
		{
			return l.GetHashCode() > r.GetHashCode();
		}

		public static bool operator <(Version l, Version r)
		{
			return l.GetHashCode() < r.GetHashCode();
		}

		public static bool operator <=(Version l, Version r)
		{
			return l.GetHashCode() <= r.GetHashCode();
		}

		public static bool operator >=(Version l, Version r)
		{
			return l.GetHashCode() >= r.GetHashCode();
		}
	}
}

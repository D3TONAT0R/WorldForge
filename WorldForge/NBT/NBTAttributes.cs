using System;
using System.Collections.Generic;
using System.Text;

namespace WorldForge.NBT
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public class NBTAttribute : Attribute
	{

		public string tagName = null;
		public GameVersion addedIn = GameVersion.FirstVersion;
		public GameVersion removedIn = new GameVersion(GameVersion.Stage.Release, 9, 9, 9);

		public NBTAttribute()
		{

		}

		public NBTAttribute(string name)
		{
			tagName = name;
		}

		public NBTAttribute(string name, string addedInVersion) : this(name)
		{
			addedIn = GameVersion.Parse(addedInVersion);
		}

		public NBTAttribute(string name, string addedInVersion, string removedInVersion) : this(name)
		{
			addedIn = GameVersion.Parse(addedInVersion);
			removedIn = GameVersion.Parse(removedInVersion);
		}
	}

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public class NBTCollectionAttribute : Attribute
	{

		public GameVersion addedIn = GameVersion.FirstVersion;
		public GameVersion removedIn = new GameVersion(GameVersion.Stage.Release, 9, 9, 9);

		public NBTCollectionAttribute()
		{

		}

		public NBTCollectionAttribute(string addedInVersion)
		{
			addedIn = GameVersion.Parse(addedInVersion);
		}

		public NBTCollectionAttribute(string addedInVersion, string removedInVersion)
		{
			addedIn = GameVersion.Parse(addedInVersion);
			removedIn = GameVersion.Parse(removedInVersion);
		}
	}
}

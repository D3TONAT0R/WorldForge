using System.Net.NetworkInformation;
using WorldForge.Items;
using WorldForge.NBT;

namespace WorldForge
{
	public class Player
	{
		public enum GameMode : int
		{ 
			Survival = 0, 
			Creative = 1, 
			Adventure = 2, 
			Spectator = 3
		}

		public class Abilities : INBTConverter
		{
			[NBT]
			public bool flying = false;
			[NBT]
			public float flySpeed = 0.05f;
			[NBT]
			public bool instabuild = false;
			[NBT]
			public bool invulnerable = false;
			[NBT]
			public bool mayBuild = true;
			[NBT]
			public bool mayfly = false;
			[NBT]
			public float walkSpeed = 0.1f;

			public object ToNBT(GameVersion version)
			{
				var nbt = new NBTCompound();
				NBTConverter.WriteToNBT(this, nbt, version);
				return nbt;
			}

			public void FromNBT(object nbtData)
			{
				var comp = (NBTCompound)nbtData;
				NBTConverter.LoadFromNBT(comp, this);
			}
		}

		//TODO: find out when this was added
		[NBT("UUID", "1.0.0")]
		public int[] uuid = new int[4];

		//[NBT("Dimension")]
		//This needs to be loaded manually because it can be either a string or an int
		public string dimension = "minecraft:overworld";
		[NBT("Pos")]
		public Vector3 position = new Vector3(0, 0, 0);
		[NBT("Rotation")]
		public Vector2F rotation = new Vector2F(0, 0);
		[NBT("Motion")]
		public Vector3 motion = new Vector3(0, 0, 0);
		[NBT("OnGround")]
		public bool onGround = false;
		[NBT("FallDistance")]
		public float fallDistance = 0;
		[NBT("Fire")]
		public short fire = -20;

		[NBT("abilities", "1.0.0")]
		public Abilities abilities = new Abilities();
		//TODO: find out when this was added
		[NBT("playerGameType", "1.0.0")]
		public GameMode playerGameType = GameMode.Survival;

		[NBT("AttackTime")]
		public short attackTime = 0;
		[NBT("DeathTime")]
		public short deathTime = 0;

		//TODO: find out when this was added
		[NBT("Invulnerable", "1.0.0")]
		public bool invulnerable = false;
		//[NBT("Health")]
		public short health = 20;
		//TODO: find out when this was added
		[NBT("HealF", "1.0.0")]
		public float healF = 20;
		[NBT("HurtTime")]
		public short hurtTime = 0;

		[NBT("Sleeping")]
		public bool sleeping;
		[NBT("SleepTimer")]
		public short sleepTimer = 0;

		[NBT("foodLevel", "b1.8")]
		public int foodLevel = 20;
		[NBT("foodSaturationLevel", "b1.8")]
		public float foodSaturationLevel = 5f;
		[NBT("foodExhaustionLevel", "b1.8")]
		public float foodExhaustionLevel = 0f;
		[NBT("foodTickTimer", "b1.8")]
		public int foodTickTimer = 0;

		[NBT("Air")]
		public short Air = 300;
		//TODO: find out when these were added
		[NBT("AbsorptionAmount", "1.0.0")]
		public float absorptionAmount = 0;
		[NBT("seenCredits", "1.0.0")]
		public bool seenCredits = false;

		[NBT("Inventory")]
		public Inventory inventory = new Inventory();
		//TODO: find out when this was added
		[NBT("SelectedItemSlot", "1.0.0")]
		public int selectedItemSlot = 0;
		[NBT("EnderItems", "1.3.1")]
		public Inventory enderItems = new Inventory();

		//TODO: find out when xp was added
		[NBT("XpP", "1.0.0")]
		public float xpP = 0f;
		[NBT("XpLevel", "1.0.0")]
		public int xpLevel = 0;
		[NBT("XpSeed", "1.0.0")]
		public int xpSeed = 25565;
		[NBT("XpTotal", "1.0.0")]
		public int xpTotal = 0;
		//TODO: find out when this was added
		[NBT("Score")]
		public int Score = 0;

		public Player(Vector3 position)
		{
			this.position = position;
		}

		public Player(NBTCompound nbt)
		{
			NBTConverter.LoadFromNBT(nbt, this);
			object dim = nbt.Get("Dimension");
			if(dim != null)
			{
				if(dim is string dimString) dimension = dimString;
				else dimension = DimensionIndexToID((int)dim);
			}
			object healthValue = nbt.Get("Health");
			if(healthValue != null)
			{
				if(healthValue is short hs) health = hs;
				else health = (short)(float)healthValue;
			}
		}

		public NBTCompound ToNBT(GameVersion version)
		{
			NBTCompound nbt = new NBTCompound();
			NBTConverter.WriteToNBT(this, nbt, version);
			return nbt;
		}

		public static string DimensionIndexToID(int index)
		{
			switch(index)
			{
				case -1: return "minecraft:the_nether";
				case 0: return "minecraft:overworld";
				case 1: return "minecraft:the_end";
				default: return "minecraft:overworld";
			}
		}
	}
}

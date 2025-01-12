using System.Collections.Generic;
using System.Net.NetworkInformation;
using WorldForge.Entities;
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

		public class RecipeBook : INBTConverter
		{
			[NBT("recipes")]
			public List<string> recipes = new List<string>();
			[NBT("toBeDisplayed")]
			public List<string> toBeDisplayed = new List<string>();

			[NBT("isFilteringCraftable")]
			public bool isFilteringCraftable = false;
			[NBT("isBlastingFurnaceFilteringCraftable")]
			public bool isBlastingFurnaceFilteringCraftable = false;
			[NBT("isFurnaceFilteringCraftable")]
			public bool isFurnaceFilteringCraftable = false;
			[NBT("isSmokerFilteringCraftable")]
			public bool isSmokerFilteringCraftable = false;

			[NBT("isGuiOpen")]
			public bool isGuiOpen = false;
			[NBT("isFurnaceGuiOpen")]
			public bool isFurnaceGuiOpen = false;
			[NBT("isBlastingFurnaceGuiOpen")]
			public bool isBlastingFurnaceGuiOpen = false;
			[NBT("isSmokerGuiOpen")]
			public bool isSmokerGuiOpen = false;

			public RecipeBook()
			{

			}

			public RecipeBook(NBTCompound nbt)
			{
				NBTConverter.LoadFromNBT(nbt, this);
			}

			public void FromNBT(object nbtData)
			{
				NBTConverter.LoadFromNBT((NBTCompound)nbtData, this);
			}

			public object ToNBT(GameVersion version)
			{
				return NBTConverter.WriteToNBT(this, new NBTCompound(), version);
			}
		}

		public class WardenSpawnTracker : INBTConverter
		{
			[NBT("cooldown_ticks")]
			public int cooldownTicks = 0;
			[NBT("ticks_since_last_warning")]
			public int ticksSinceLastWarning = 0;
			[NBT("warning_level")]
			public int warningLevel = 0;

			public WardenSpawnTracker()
			{

			}

			public WardenSpawnTracker(NBTCompound nbt)
			{
				NBTConverter.LoadFromNBT(nbt, this);
			}

			public void FromNBT(object nbtData)
			{
				NBTConverter.LoadFromNBT((NBTCompound)nbtData, this);
			}

			public object ToNBT(GameVersion version)
			{
				return NBTConverter.WriteToNBT(this, new NBTCompound(), version);
			}
		}

		//TODO: find out when this was added
		[NBT("UUID", "1.0.0")]
		public int[] uuid = new int[4];

		//[NBT("Dimension")]
		//This needs to be loaded manually because it can be either a string or an int
		public DimensionID dimension = DimensionID.Overworld;
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
		[NBT("FallFlying", "1.10")]
		public bool fallFlying = false;
		[NBT("Fire")]
		public short fire = -20;

		[NBT("Brain", "1.14")]
		public NBTCompound brain = new NBTCompound();
		[NBT("abilities", "1.0.0")]
		public Abilities abilities = new Abilities();
		[NBT("Attributes", "1.0.0")]
		public Attributes attributes = new Attributes();
		//TODO: find out when this was added
		[NBT("playerGameType", "1.0.0")]
		public GameMode playerGameType = GameMode.Survival;
		[NBT("recipeBook", "1.12")]
		public RecipeBook recipeBook = new RecipeBook();
		[NBT("warden_spawn_tracker", "1.19")]
		public WardenSpawnTracker wardenSpawnTracker = new WardenSpawnTracker();

		[NBT("AttackTime", null, "1.8")]
		public short attackTime = 0;
		[NBT("DeathTime")]
		public short deathTime = 0;

		//TODO: find out when this was added
		[NBT("Invulnerable", "1.0.0")]
		public bool invulnerable = false;
		//[NBT("Health")]
		//public short health = 20;
		//[NBT("HealF", "1.0.0")]
		public float health = 20;
		[NBT("HurtTime")]
		public short hurtTime = 0;
		//TODO: determine exact version (somewhere between 1.7.10 and 1.9)
		[NBT("HurtByTimestamp", "1.9")]
		public int hurtByTimestamp = 0;

		[NBT("Sleeping", null, "1.14")]
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

		public Player(NBTCompound nbt, GameVersion version)
		{
			NBTConverter.LoadFromNBT(nbt, this);
			//Versions prior to 1.3 used the head position as the player position
			if(version < GameVersion.Release_1(3))
			{
				position.y -= 1.62f;
			}
			object dim = nbt.Get("Dimension");
			if(dim != null)
			{
				if(dim is string dimString) dimension = new DimensionID(dimString);
				else dimension = DimensionID.FromIndex((int)dim);
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
			//Versions prior to 1.3 used the head position as the player position
			if(version < GameVersion.Release_1(3)) position.y += 1.62f;
			NBTConverter.WriteToNBT(this, nbt, version);
			//Restore position
			if(version < GameVersion.Release_1(3)) position.y -= 1.62f;

			if(version >= GameVersion.Release_1(16)) nbt.Add("Dimension", dimension.ID);
			else nbt.Add("Dimension", dimension.DimensionIndex);

			if(version >= GameVersion.Release_1(9)) nbt.Add("Health", health);
			else
			{
				nbt.Add("Health", (short)health);
				nbt.Add("HealF", health);
			}

			return nbt;
		}
	}
}

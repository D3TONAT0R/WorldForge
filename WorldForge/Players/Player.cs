using System.Collections.Generic;
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

		public class EquipmentSlots
		{
			public ItemStack head;
			public ItemStack chest;
			public ItemStack legs;
			public ItemStack feet;
			public ItemStack offhand;

			public void LoadFromNBT(NBTCompound nbt)
			{
				if (nbt.TryGet("head", out NBTCompound headNBT)) head = new ItemStack(headNBT, out _);
				if (nbt.TryGet("chest", out NBTCompound chestNBT)) chest = new ItemStack(chestNBT, out _);
				if (nbt.TryGet("legs", out NBTCompound legsNBT)) legs = new ItemStack(legsNBT, out _);
				if (nbt.TryGet("feet", out NBTCompound feetNBT)) feet = new ItemStack(feetNBT, out _);
				if (nbt.TryGet("offhand", out NBTCompound offhandNBT)) offhand = new ItemStack(offhandNBT, out _);
			}

			public NBTCompound ToNBT(GameVersion version)
			{
				NBTCompound nbt = new NBTCompound();
				if (head != null && !head.IsNull) nbt.Add("head", head.ToNBT(version));
				if (chest != null && !chest.IsNull) nbt.Add("chest", chest.ToNBT(version));
				if (legs != null && !legs.IsNull) nbt.Add("legs", legs.ToNBT(version));
				if (feet != null && !feet.IsNull) nbt.Add("feet", feet.ToNBT(version));
				if (offhand != null && !offhand.IsNull) nbt.Add("offhand", offhand.ToNBT(version));
				return nbt;
			}
		}

		//TODO: find out when this was added
		[NBT("UUID", "1.0.0")]
		public UUID uuid;

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
		//[NBT("equipment")]
		public EquipmentSlots equipmentSlots = new EquipmentSlots();
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
		public int score = 0;

		public Player(Vector3 position)
		{
			this.position = position;
		}

		public Player(NBTCompound nbt, GameVersion version)
		{
			NBTConverter.LoadFromNBT(nbt, this);
			//Versions prior to 1.3 used the head position as the player position
			if (version < GameVersion.Release_1(3))
			{
				position.y -= 1.62f;
			}
			object dim = nbt.Get("Dimension");
			if (dim != null)
			{
				if (dim is string dimString) dimension = DimensionID.FromID(dimString);
				else dimension = DimensionID.FromIndex((int)dim);
			}
			object healthValue = nbt.Get("Health");
			if (healthValue != null)
			{
				if (healthValue is short hs) health = hs;
				else health = (short)(float)healthValue;
			}
			if (nbt.TryGet("equipment", out NBTCompound equipmentNBT))
			{
				equipmentSlots.LoadFromNBT(equipmentNBT);
			}
			else
			{
				if (inventory.HasItem(103)) equipmentSlots.head = inventory.TakeItem(103);
				if (inventory.HasItem(102)) equipmentSlots.chest = inventory.TakeItem(102);
				if (inventory.HasItem(101)) equipmentSlots.legs = inventory.TakeItem(101);
				if (inventory.HasItem(100)) equipmentSlots.feet = inventory.TakeItem(100);
				if (inventory.HasItem(-106)) equipmentSlots.offhand = inventory.TakeItem(-106);
			}
		}

		public static Player FromFile(string filename, GameVersion gameVersionHint)
		{
			var nbt = new NBTFile(filename);
			var gameVersion = GameVersion.FromDataVersion(nbt.dataVersion);
			return new Player(nbt.contents, gameVersion ?? gameVersionHint);
		}

		public NBTCompound ToNBT(GameVersion version)
		{
			NBTCompound nbt = new NBTCompound();
			//Versions prior to 1.3 used the head position as the player position
			if (version < GameVersion.Release_1(3)) position.y += 1.62f;
			NBTConverter.WriteToNBT(this, nbt, version);
			//Restore position
			if (version < GameVersion.Release_1(3)) position.y -= 1.62f;

			if (version >= GameVersion.Release_1(16)) nbt.Add("Dimension", dimension.ID);
			else nbt.Add("Dimension", dimension.Index ?? 0);

			if (version >= GameVersion.Release_1(9)) nbt.Add("Health", health);
			else
			{
				nbt.Add("Health", (short)health);
				nbt.Add("HealF", health);
			}

			if (version >= GameVersion.Release_1(21, 5))
			{
				nbt.Add("equipment", equipmentSlots.ToNBT(version));
			}
			else
			{
				var inv = nbt.GetAsList("Inventory");
				//Delete existing armor and offhand items from inventory to avoid duplication
				for (int i = inv.Length - 1; i >= 0; i--)
				{
					var comp = inv.Get<NBTCompound>(i);
					int slot = comp.Get<int>("Slot");
					if(slot >= 100 && slot <= 103 || slot == -106)
					{
						inv.RemoveAt(i);
					}
				}
				//Add armor and offhand items from equipment slots
				AddEquipmentToInventoryNBT(inv, equipmentSlots.head, 103, version);
				AddEquipmentToInventoryNBT(inv, equipmentSlots.chest, 102, version);
				AddEquipmentToInventoryNBT(inv, equipmentSlots.legs, 101, version);
				AddEquipmentToInventoryNBT(inv, equipmentSlots.feet, 100, version);
				AddEquipmentToInventoryNBT(inv, equipmentSlots.offhand, -106, version);
			}

			return nbt;
		}

		private void AddEquipmentToInventoryNBT(NBTList inv, ItemStack stack, sbyte slotIndex, GameVersion version)
		{
			if(stack != null && !stack.IsNull && stack.ToNBT(slotIndex, version, out var nbt))
			{
				inv.Add(nbt);
			}
		}

	}
}

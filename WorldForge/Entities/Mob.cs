using System;
using System.Collections.Generic;
using System.Text;
using WorldForge.Coordinates;
using WorldForge.Items;
using WorldForge.NBT;

namespace WorldForge.Entities
{
	public abstract class Mob : Entity
	{
		public class LeashTarget : INBTConverter
		{
			public BlockCoord? blockCoordinate = null;
			public UUID entityUUID = null;

			public bool IsEntity => entityUUID != null;

			public LeashTarget(BlockCoord blockCoordinate)
			{
				this.blockCoordinate = blockCoordinate;
			}

			public LeashTarget(UUID entityUUID)
			{
				this.entityUUID = entityUUID;
			}

			public void FromNBT(object nbtData)
			{
				if(nbtData is int[] pos)
				{
					blockCoordinate = new BlockCoord(pos[0], pos[1], pos[2]);
				}
				else if(nbtData is NBTCompound entitiyComp)
				{
					entityUUID = new UUID(entitiyComp.Get<int[]>("UUID"));
				}
			}

			public object ToNBT(GameVersion version)
			{
				if(IsEntity)
				{
					return new NBTCompound()
					{
						{ "UUID", entityUUID.ToNBT(version) }
					};
				}
				else
				{
					return new int[] { blockCoordinate.Value.x, blockCoordinate.Value.y, blockCoordinate.Value.z };
				}
			}
		}

		[NBT("AbsorptionAmount")]
		public float absorptionAmount = 0;
		[NBT("active_effects")]
		public List<NBTCompound> activeEffects = new List<NBTCompound>();
		[NBT("ArmorDropChances")]
		public List<float> armorDropChances = new List<float>();
		[NBT("ArmorItems")]
		public List<ItemStack> armorItems = new List<ItemStack>();
		[NBT("Attributes")]
		public List<Attribute> attributes = new List<Attribute>();
		[NBT("body_armor_drop_chance")]
		public float bodyArmorDropChance = 0;
		[NBT("body_armor_item")]
		public ItemStack bodyArmorItem = null;
		[NBT("Brain")]
		public NBTCompound brain = null;

		[NBT("CanPickUpLoot")]
		public bool canPickUpLoot = false;
		[NBT("DeathLootTable")]
		public string deathLootTable = null;
		[NBT("DeathLootTableSeed")]
		public long deathLootTableSeed = 0;

		[NBT("DeathTime")]
		public int deathTime = 0;
		[NBT("FallFlying")]
		public bool fallFlying = false;

		[NBT("Health")]
		public float health = 0;
		[NBT("HurtByTimestamp")]
		public int hurtByTimestamp = 0;
		[NBT("HurtTime")]
		public short hurtTime = 0;

		[NBT("HandDropChances")]
		public List<float> handDropChances = new List<float>();
		[NBT("HandItems")]
		public List<ItemStack> handItems = new List<ItemStack>();

		[NBT("LeftHanded")]
		public bool leftHanded = false;

		[NBT("leash")]
		public LeashTarget leashTarget = null;

		[NBT("NoAI")]
		public bool noAI = false;
		[NBT("PersistenceRequired")]
		public bool persistenceRequired = false;

		[NBT("Team")]
		public string team = null;

		public BlockCoord? sleepCoordinate;

		public Mob(NBTCompound compound) : base(compound)
		{
			if(compound.Contains("SleepingX"))
			{
				sleepCoordinate = new BlockCoord(
					compound.Get<int>("SleepingX"), 
					compound.Get<int>("SleepingY"), 
					compound.Get<int>("SleepingZ"));
			}
		}

		public Mob(string id, Vector3 position) : base(id, position)
		{

		}

		public override NBTCompound ToNBT(GameVersion version)
		{
			var comp = base.ToNBT(version);
			if(sleepCoordinate.HasValue)
			{
				comp.Add("SleepingX", sleepCoordinate.Value.x);
				comp.Add("SleepingY", sleepCoordinate.Value.y);
				comp.Add("SleepingZ", sleepCoordinate.Value.z);
			}
			return comp;
		}
	}
}

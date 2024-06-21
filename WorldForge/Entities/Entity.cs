using System;
using System.Collections.Generic;
using WorldForge.NBT;

namespace WorldForge.Entities
{
	public abstract class Entity
	{

		public NBTCompound NBTData
		{
			get;
			private set;
		}

		[NBT("id")]
		public string id = null;
		[NBT("UUID")]
		public UUID uuid = default;

		[NBT("Pos")]
		public Vector3 position = default;
		[NBT("Motion")]
		public Vector3 motion = default;
		[NBT("Rotation")]
		public Vector2F rotation = default;
		[NBT("OnGround")]
		public bool onGround = false;
		[NBT("FallDistance")]
		public float fallDistance = 0f;
		[NBT("NoGravity")]
		public bool noGravity = false;
		[NBT("PortalCooldown")]
		public int portalCooldown = 0;

		[NBT("Invulnerable")]
		public bool invulnerable = false;
		[NBT("Air")]
		public short air = 300;
		[NBT("Fire")]
		public short fire = -20;
		[NBT("HasVisualFire")]
		public bool hasVisualFire = false;
		[NBT("TicksFrozen")]
		public int ticksFrozen = 0;
		[NBT("Silent")]
		public bool silent = false;
		[NBT("Glowing")]
		public bool glowing = false;
		[NBT("Passengers")]
		public List<Entity> passengers = new List<Entity>();

		[NBT("CustomName")]
		public string customNameJSON = null;
		[NBT("CustomNameVisible")]
		public bool customNameVisible = false;

		[NBT("Tags")]
		public List<string> tags = new List<string>();

		public int BlockPosX => (int)Math.Floor(position.x);
		public int BlockPosY => (int)Math.Floor(position.y);
		public int BlockPosZ => (int)Math.Floor(position.z);

		public Entity(NBTCompound compound)
		{
			NBTData = compound;
			NBTConverter.LoadFromNBT(compound, this);
		}

		public static Entity CreateFromNBT(NBTCompound nbt)
		{
			var id = nbt.Get<string>("id");
			switch(id)
			{
				case "minecraft:creeper": return new MobCreeper(nbt);
				case "minecraft:pig": return new MobPig(nbt);
				case "minecraft:skeleton": return new MobSkeleton(nbt);
				case "minecraft:zombie": return new MobZombie(nbt);
				case "minecraft:spider": return new MobSpider(nbt);
				case "minecraft:sheep": return new MobSheep(nbt);
				case "minecraft:giant": return new MobGiant(nbt);
				case "minecraft:cow": return new MobCow(nbt);
				case "minecraft:slime": return new MobSlime(nbt);
				case "minecraft:chicken": return new MobChicken(nbt);
				case "minecraft:ghast": return new MobGhast(nbt);
				case "minecraft:zombie_pigman": return new MobZombiePigman(nbt);
				case "minecraft:squid": return new MobSquid(nbt);
				case "minecraft:wolf": return new MobWolf(nbt);
				case "minecraft:cave_spider": return new MobCaveSpider(nbt);
				case "minecraft:enderman": return new MobEnderman(nbt);
				case "minecraft:silverfish": return new MobSilverfish(nbt);
				default: return new GenericEntity(nbt);
			}
		}

		public Entity(string id, Vector3 position)
		{
			this.id = id;
			this.position = position;
		}

		public virtual NBTCompound ToNBT(GameVersion version)
		{
			return NBTConverter.WriteToNBT(this, new NBTCompound(), version);
		}
	}
}

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
				case "minecraft:blaze": return new MobBlaze(nbt);
				case "minecraft:magma_cube": return new MobMagmaCube(nbt);
				case "minecraft:mooshroom": return new MobMooshroom(nbt);
				case "minecraft:snow_golem": return new MobSnowGolem(nbt);
				case "minecraft:villager": return new MobVillager(nbt);
				case "minecraft:ender_dragon": return new MobEnderDragon(nbt);
				case "minecraft:ocelot": return new MobOcelot(nbt);
				case "minecraft:iron_golem": return new MobIronGolem(nbt);
				case "minecraft:zombie_villager": return new MobZombieVillager(nbt);
				case "minecraft:wither": return new MobWither(nbt);
				case "minecraft:bat": return new MobBat(nbt);
				case "minecraft:witch": return new MobWitch(nbt);
				case "minecraft:horse": return new MobHorse(nbt);
				case "minecraft:donkey": return new MobDonkey(nbt);
				case "minecraft:mule": return new MobMule(nbt);
				case "minecraft:skeleton_horse": return new MobSkeletonHorse(nbt);
				case "minecraft:zombie_horse": return new MobZombieHorse(nbt);
				case "minecraft:endermite": return new MobEndermite(nbt);
				case "minecraft:guardian": return new MobGuardian(nbt);
				case "minecraft:elder_guardian": return new MobElderGuardian(nbt);
				case "minecraft:rabbit": return new MobRabbit(nbt);
				case "minecraft:shulker": return new MobShulker(nbt);
				case "minecraft:husk": return new MobHusk(nbt);
				case "minecraft:polar_bear": return new MobPolarBear(nbt);
				case "minecraft:stray": return new MobStray(nbt);
				case "minecraft:llama": return new MobLlama(nbt);
				case "minecraft:vindicator": return new MobVindicator(nbt);
				case "minecraft:evoker": return new MobEvoker(nbt);
				case "minecraft:vex": return new MobVex(nbt);
				case "minecraft:parrot": return new MobParrot(nbt);
				case "minecraft:illusioner": return new MobIllusioner(nbt);
				case "minecraft:phantom": return new MobPhantom(nbt);
				case "minecraft:turtle": return new MobTurtle(nbt);
				case "minecraft:cod": return new MobCod(nbt);
				case "minecraft:salmon": return new MobSalmon(nbt);
				case "minecraft:pufferfish": return new MobPufferfish(nbt);
				case "minecraft:tropical_fish": return new MobTropicalFish(nbt);
				case "minecraft:drowned": return new MobDrowned(nbt);
				case "minecraft:dolphin": return new MobDolphin(nbt);
				case "minecraft:panda": return new MobPanda(nbt);
				case "minecraft:pillager": return new MobPillager(nbt);
				case "minecraft:ravager": return new MobRavager(nbt);
				case "minecraft:cat": return new MobCat(nbt);
				case "minecraft:trader_llama": return new MobTraderLlama(nbt);
				case "minecraft:wandering_trader": return new MobWanderingTrader(nbt);
				case "minecraft:fox": return new MobFox(nbt);
				case "minecraft:bee": return new MobBee(nbt);
				case "minecraft:hoglin": return new MobHoglin(nbt);
				case "minecraft:piglin": return new MobPiglin(nbt);
				case "minecraft:strider": return new MobStrider(nbt);
				case "minecraft:zoglin": return new MobZoglin(nbt);
				case "minecraft:piglin_brute": return new MobPiglinBrute(nbt);
				case "minecraft:axolotl": return new MobAxolotl(nbt);
				case "minecraft:glow_squid": return new MobGlowSquid(nbt);
				case "minecraft:goat": return new MobGoat(nbt);
				case "minecraft:warden": return new MobWarden(nbt);
				case "minecraft:frog": return new MobFrog(nbt);
				case "minecraft:tadpole": return new MobTadpole(nbt);
				case "minecraft:allay": return new MobAllay(nbt);
				case "minecraft:camel": return new MobCamel(nbt);
				case "minecraft:sniffer": return new MobSniffer(nbt);
				case "minecraft:breeze": return new MobBreeze(nbt);
				case "minecraft:armadillo": return new MobArmadillo(nbt);
				case "minecraft:bogged": return new MobBogged(nbt);

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

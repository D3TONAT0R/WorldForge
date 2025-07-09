using System;
using WorldForge.Coordinates;
using WorldForge.Items;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public abstract class TileEntity
	{
		public string id;
		public NBTCompound otherNBTData;

		protected TileEntity(string id)
		{
			this.id = id;
			otherNBTData = new NBTCompound();
		}

		protected TileEntity(NBTCompound nbt, out BlockCoord blockPos)
		{
			id = nbt.Take<string>("id");
			blockPos = new BlockCoord(nbt.Take<int>("x"), nbt.Take<int>("y"), nbt.Take<int>("z"));
			NBTConverter.LoadFromNBT(nbt, this, true);
			otherNBTData = nbt;
		}

		public static TileEntity CreateFromNBT(NBTCompound nbt, GameVersion? version, out BlockCoord blockPos)
		{
			string id = nbt.Get<string>("id");
			var shortId = id.Replace("minecraft:", "");
			switch(shortId)
			{
				case "chest":
				case "trapped_chest":
				case "barrel":
				case "shulker_box":
				case "Chest":
					return new TileEntityContainer(nbt, 27, out blockPos);
				case "dispenser":
				case "dropper":
				case "Trap":
					return new TileEntityContainer(nbt, 9, out blockPos);
				case "hopper":
					return new TileEntityContainer(nbt, 5, out blockPos);
				case "sign":
				case "Sign":
					return new TileEntitySign(nbt, version, out blockPos);
				case "beacon":
					return new TileEntityBeacon(nbt, out blockPos);
				case "bee_nest":
				case "beehive":
					return new TileEntityBeehive(nbt, out blockPos);
				case "furnace":
				case "blast_furnace":
				case "smoker":
				case "Furnace":
					return new TileEntityFurnace(nbt, out blockPos);
				case "campfire":
				case "soul_campfire":
					return new TileEntityCampfire(nbt, out blockPos);
				case "comparator":
					return new TileEntityComparator(nbt, out blockPos);
				case "conduit":
					return new TileEntityConduit(nbt, out blockPos);
				case "end_gateway":
					return new TileEntityEndGateway(nbt, out blockPos);
				case "jigsaw":
					return new TileEntityJigsaw(nbt, out blockPos);
				case "jukebox":
				case "RecordPlayer":
					return new TileEntityJukebox(nbt, out blockPos);
				case "lectern":
					return new TileEntityLectern(nbt, out blockPos);
				case "mob_spawner":
				case "MobSpawner":
					return new TileEntitySpawner(nbt, out blockPos);
				case "moving_piston":
					//TODO: check if this is the correct block id for this.
					return new TileEntityPiston(nbt, out blockPos);
				case "banner":
					return new TileEntityBanner(nbt, out blockPos);
				case "command_block":
				case "Control":
					return new TileEntityCommandBlock(nbt, out blockPos);
				case "skull":
					return new TileEntitySkull(nbt, out blockPos);
				case "structure_block":
					return new TileEntityStructureBlock(nbt, out blockPos);
				case "chiseled_bookshelf":
					return new TileEntityChiseledBookshelf(nbt, out blockPos);
				case "decorated_pot":
					return new TileEntityDecoratedPot(nbt, out blockPos);
				case "trial_spawner":
					return new TileEntityTrialSpawner(nbt, out blockPos);
				case "vault":
					return new TileEntityVault(nbt, out blockPos);

				case "end_portal":
				case "EndPortal":
					return new TileEntityEndPortal(nbt, out blockPos);
				case "ender_chest":
				case "EnderChest":
					return new TileEntityEnderChest(nbt, out blockPos);
				case "enchanting_table":
				case "EnchantTable":
					return new TileEntityEnchantingTable(nbt, out blockPos);
				case "bell":
					return new TileEntityBell(nbt, out blockPos);
				case "bed":
				case "Bed":
					return new TileEntityBed(nbt, out blockPos);

				default:
					Logger.Verbose($"Unknown TileEntity ID: {id}. Creating generic TileEntity.");
					return new TileEntityGeneric(nbt, out blockPos);
			}
		}

		public static TileEntity CreateFor(BlockID blockType)
		{
			if(blockType.IsVanillaBlock)
			{
				if(blockType.ID.id.EndsWith("shulker_box"))
				{
					return CreateShulkerBox();
				}
				if(blockType.ID.id.EndsWith("banner"))
				{
					return new TileEntityBanner();
				}
				if(blockType.ID.id.EndsWith("command_block"))
				{
					return new TileEntityCommandBlock();
				}
				if(blockType.ID.id.EndsWith("head") || blockType.ID.id.EndsWith("skull"))
				{
					return new TileEntitySkull();
				}
				switch(blockType.ID.id)
				{
					case "chest":
						return CreateChest();
					case "trapped_chest":
						return CreateTrappedChest();
					case "barrel":
						return CreateBarrel();
					case "dispenser":
						return CreateDispenser();
					case "dropper":
						return CreateDropper();
					case "hopper":
						return CreateHopper();
					case "sign":
						return new TileEntitySign();
					case "beacon":
						return new TileEntityBeacon();
					case "bee_nest":
					case "beehive":
						return new TileEntityBeehive(blockType.ID.id);
					case "furnace":
					case "blast_furnace":
					case "smoker":
						return new TileEntityFurnace(blockType.ID.id);
					case "campfire":
					case "soul_campfire":
						return new TileEntityCampfire(blockType.ID.id);
					case "comparator":
						return new TileEntityComparator();
					case "conduit":
						return new TileEntityConduit();
					case "end_gateway":
						return new TileEntityEndGateway();
					case "jigsaw":
						return new TileEntityJigsaw();
					case "jukebox":
						return new TileEntityJukebox();
					case "lectern":
						return new TileEntityLectern();
					case "mob_spawner":
						return new TileEntitySpawner();
					case "moving_piston":
						//TODO: check if this is the correct block id for this.
						return new TileEntityPiston();
					case "structure_block":
						return new TileEntityStructureBlock();
					case "chiseled_bookshelf":
						return new TileEntityChiseledBookshelf();
				}
				Logger.Verbose($"Unknown TileEntity ID for block type {blockType.ID.id}, creating generic TileEntity.");
				return new TileEntityGeneric(blockType.ID.id);
			}
			else
			{
				Logger.Verbose($"Modded block type {blockType.ID.id} detected, creating generic TileEntity.");
				return new TileEntityGeneric(blockType.ID.id);
			}
		}

		public static TileEntityContainer CreateChest(params (sbyte, ItemStack)[] content) => new TileEntityContainer("chest", 27, content);
		public static TileEntityContainer CreateTrappedChest(params (sbyte, ItemStack)[] content) => new TileEntityContainer("trapped_chest", 27, content);
		public static TileEntityContainer CreateBarrel(params (sbyte, ItemStack)[] content) => new TileEntityContainer("barrel", 27, content);
		public static TileEntityContainer CreateDispenser(params (sbyte, ItemStack)[] content) => new TileEntityContainer("dispenser", 9, content);
		public static TileEntityContainer CreateDropper(params (sbyte, ItemStack)[] content) => new TileEntityContainer("dropper", 9, content);
		public static TileEntityContainer CreateHopper(params (sbyte, ItemStack)[] content) => new TileEntityContainer("hopper", 5, content);
		public static TileEntityContainer CreateShulkerBox(params (sbyte, ItemStack)[] content) => new TileEntityContainer("shulker_box", 27, content);

		public static TileEntityFurnace CreateFurnace() => new TileEntityFurnace("furnace");
		public static TileEntityFurnace CreateBlastFurnace() => new TileEntityFurnace("blast_furnace");
		public static TileEntityFurnace CreateSmoker() => new TileEntityFurnace("smoker");

		public NBTCompound ToNBT(GameVersion version, BlockCoord blockPos)
		{
			string resolvedId = ResolveTileEntityID(version);
			if(resolvedId == null) return null;
			if(version < GetAddedVersion(resolvedId)) return null;
			NBTCompound nbt = new NBTCompound
			{
				{ "id", resolvedId },
				{ "x", blockPos.x },
				{ "y", blockPos.y },
				{ "z", blockPos.z }
			};
			NBTConverter.WriteToNBT(this, nbt, version);
			OnWriteToNBT(nbt, version);
			otherNBTData.Merge(nbt, false);
			return nbt;
		}

		protected virtual void OnWriteToNBT(NBTCompound nbt, GameVersion version)
		{

		}

		public virtual string ResolveTileEntityID(GameVersion version)
		{
			return id;
		}

		public static GameVersion GetAddedVersion(string id)
		{
			var shortID = id.Replace("minecraft:", "");
			switch(shortID)
			{
				case "chest":
				case "Chest":
					return GameVersion.FirstVersion;
				case "shulker_box":
					return GameVersion.Release_1(11);
				case "barrel":
					return GameVersion.Release_1(14);
				case "trapped_chest":
					return GameVersion.Release_1(5);
				case "dispenser":
				case "Trap":
					return GameVersion.Beta_1(2);
				case "dropper":
					return GameVersion.Release_1(5);
				case "banner":
					return GameVersion.Release_1(8);
				case "beacon":
					return GameVersion.Release_1(4, 2);
				case "bee_nest":
				case "beehive":
					return GameVersion.Release_1(15);
				case "bell":
					return GameVersion.Release_1(14);
				case "bed":
				case "Bed":
					return GameVersion.FirstVersion;
				case "brewing_stand":
				case "Cauldron":
					return GameVersion.Release_1(0);
				case "campfire":
				case "soul_campfire":
					return GameVersion.Release_1(14);
				case "chiseled_bookshelf":
					return GameVersion.Release_1(19, 3);
				case "command_block":
				case "Control":
					return GameVersion.Release_1(4, 2);
				case "comparator":
					return GameVersion.Release_1(5);
				case "conduit":
					return GameVersion.Release_1(13);
				case "crafter":
					return GameVersion.Release_1(21);
				case "daylight_detector":
				case "DLDetector":
					return GameVersion.Release_1(5);
				case "decorated_pot":
					return GameVersion.Release_1(19, 4);
				case "enchanting_table":
				case "EnchantTable":
					return GameVersion.Release_1(0);
				case "ender_chest":
				case "EnderChest":
					return GameVersion.Release_1(3, 1);
				case "end_gateway":
				case "EndGateway":
					return GameVersion.Release_1(9);
				case "end_portal":
				case "AirPortal":
					return GameVersion.Release_1(0);
				case "furnace":
				case "Furnace":
					return GameVersion.FirstVersion;
				case "blast_furnace":
					return GameVersion.Release_1(14);
				case "smoker":
					return GameVersion.Release_1(14);
				case "jigsaw":
					return GameVersion.Release_1(14);
				case "jukebox":
				case "RecordPlayer":
					return GameVersion.FirstVersion;
				case "lectern":
					return GameVersion.Release_1(14);
				case "piston":
				case "Piston":
					return GameVersion.Beta_1(7);
				case "skulk_catalyst":
					return GameVersion.Release_1(19);
				case "skulk_sensor":
					return GameVersion.Release_1(17);
				case "skulk_shrieker":
					return GameVersion.Release_1(17);
				case "sign":
				case "Sign":
					return GameVersion.FirstVersion;
				case "skull":
					return GameVersion.Release_1(4, 2);
				case "mob_spawner":
				case "MobSpawner":
					return GameVersion.FirstVersion;
				case "structure_block":
				case "Structure":
					return GameVersion.Release_1(9);
				case "suspicious_gravel":
					return GameVersion.Release_1(20);
				case "suspicious_sand":
					return GameVersion.Release_1(19, 4);
				case "trial_spawner":
					return GameVersion.Release_1(21);
				case "vault":
					return GameVersion.Release_1(21);
				default:
					return GameVersion.FirstVersion;
			}
		}
	}
}

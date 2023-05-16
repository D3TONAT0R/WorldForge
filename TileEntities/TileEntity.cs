using MCUtils.Coordinates;
using MCUtils.NBT;

namespace MCUtils.TileEntities
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

		public static TileEntity CreateFromNBT(NBTCompound nbt, out BlockCoord blockPos)
		{
			string id = nbt.Get<string>("id");
			switch(id)
			{
				//Non-matching tile entities (block names differ from tile entity ID
				case "shulker_box":
					return new TileEntityContainer(nbt, 27, out blockPos);
				case "banner":
					return new TileEntityBeacon(nbt, out blockPos);
				case "command_block":
					return new TileEntityCommandBlock(nbt, out blockPos);
				case "skull":
					return new TileEntitySkull(nbt, out blockPos);

				//Matching tile entities
				case "chest":
				case "trapped_chest":
				case "barrel":
					return new TileEntityContainer(nbt, 27, out blockPos);
				case "dispenser":
				case "dropper":
					return new TileEntityContainer(nbt, 9, out blockPos);
				case "hopper":
					return new TileEntityContainer(nbt, 5, out blockPos);
				case "beacon":
					return new TileEntityBeacon(nbt, out blockPos);
				case "bee_nest":
				case "beehive":
					return new TileEntityBeehive(nbt, out blockPos);
				case "furnace":
				case "blast_furnace":
				case "smoker":
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
					return new TileEntityJukebox(nbt, out blockPos);
				case "lectern":
					return new TileEntityLectern(nbt, out blockPos);
				case "mob_spawner":
					return new TileEntitySpawner(nbt, out blockPos);
				case "moving_piston":
					//TODO: check if this is the correct block id for this.
					return new TileEntityPiston(nbt, out blockPos);
				case "structure_block":
					return new TileEntityStructureBlock(nbt, out blockPos);
				case "chiseled_bookshelf":
					return new TileEntityChiseledBookshelf(nbt, out blockPos);

				default:
					return new TileEntityGeneric(nbt, out blockPos);
			}
		}

		public static TileEntity CreateFor(ProtoBlock blockType)
		{
			if(blockType.IsVanillaBlock)
			{
				if(blockType.shortID.EndsWith("shulker_box"))
				{
					return CreateShulkerBox();
				}
				if(blockType.shortID.EndsWith("banner"))
				{
					return new TileEntityBanner();
				}
				if(blockType.shortID.EndsWith("command_block"))
				{
					return new TileEntityCommandBlock();
				}
				if(blockType.shortID.EndsWith("head") || blockType.shortID.EndsWith("skull"))
				{
					return new TileEntitySkull();
				}
				switch(blockType.shortID)
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
					case "beacon":
						return new TileEntityBeacon();
					case "bee_nest":
					case "beehive":
						return new TileEntityBeehive(blockType.shortID);
					case "furnace":
					case "blast_furnace":
					case "smoker":
						return new TileEntityFurnace(blockType.shortID);
					case "campfire":
					case "soul_campfire":
						return new TileEntityCampfire(blockType.shortID);
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
				return new TileEntityGeneric(blockType.shortID);
			}
			else
			{
				return new TileEntityGeneric(blockType.ID);
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

		public NBTCompound ToNBT(Version version, BlockCoord blockPos)
		{
			NBTCompound nbt = new NBTCompound
			{
				{ "id", ResolveEntityID(version) },
				{ "x", blockPos.x },
				{ "y", blockPos.y },
				{ "z", blockPos.z }
			};
			NBTConverter.WriteToNBT(this, nbt, version);
			Serialize(nbt, version);
			otherNBTData.Merge(nbt, false);
			return nbt;
		}

		protected abstract void Serialize(NBTCompound nbt, Version version);

		protected virtual string ResolveEntityID(Version version)
		{
			return id;
		}
	}
}

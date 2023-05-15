using MCUtils.Coordinates;
using MCUtils.NBT;

namespace MCUtils.TileEntities
{
	public abstract class TileEntity
	{
		public string id;
		public BlockCoord blockPos;
		public NBTCompound otherNBTData;

		protected TileEntity(string id, BlockCoord blockPos)
		{
			this.id = id;
			this.blockPos = blockPos;
			otherNBTData = new NBTCompound();
		}

		protected TileEntity(NBTCompound nbt)
		{
			id = nbt.Take<string>("id");
			blockPos = new BlockCoord(nbt.Take<int>("x"), nbt.Take<int>("y"), nbt.Take<int>("z"));
			otherNBTData = nbt;
		}

		public static TileEntity CreateFromNBT(NBTCompound nbt)
		{
			string id = nbt.Get<string>("id");
			switch(id)
			{
				//Non-matching tile entities (block names differ from tile entity ID
				case "shulker_box":
					return new TileEntityContainer(nbt, 27);
				case "banner":
					return new TileEntityBeacon(nbt);
				case "command_block":
					return new TileEntityCommandBlock(nbt);
				case "skull":
					return new TileEntitySkull(nbt);

				//Matching tile entities
				case "chest":
				case "trapped_chest":
				case "barrel":
					return new TileEntityContainer(nbt, 27);
				case "dispenser":
				case "dropper":
					return new TileEntityContainer(nbt, 9);
				case "hopper":
					return new TileEntityContainer(nbt, 5);
				case "beacon":
					return new TileEntityBeacon(nbt);
				case "bee_nest":
				case "beehive":
					return new TileEntityBeehive(nbt);
				case "furnace":
				case "blast_furnace":
				case "smoker":
					return new TileEntityFurnace(nbt);
				case "campfire":
				case "soul_campfire":
					return new TileEntityCampfire(nbt);
				case "comparator":
					return new TileEntityComparator(nbt);
				case "conduit":
					return new TileEntityConduit(nbt);
				case "end_gateway":
					return new TileEntityEndGateway(nbt);
				case "jigsaw":
					return new TileEntityJigsaw(nbt);
				case "jukebox":
					return new TileEntityJukebox(nbt);
				case "lectern":
					return new TileEntityLectern(nbt);
				case "mob_spawner":
					return new TileEntitySpawner(nbt);
				case "moving_piston":
					//TODO: check if this is the correct block id for this.
					return new TileEntityPiston(nbt);
				case "structure_block":
					return new TileEntityStructureBlock(nbt);
				case "chiseled_bookshelf":
					return new TileEntityChiseledBookshelf(nbt);

				default:
					return new TileEntityGeneric(nbt);
			}
		}

		public static TileEntity CreateFor(ProtoBlock blockType, BlockCoord blockPos)
		{
			if(blockType.IsVanillaBlock)
			{
				if(blockType.shortID.EndsWith("shulker_box"))
				{
					return new TileEntityContainer("shulker_box", blockPos, 27);
				}
				if(blockType.shortID.EndsWith("banner"))
				{
					return new TileEntityBanner(blockPos);
				}
				if(blockType.shortID.EndsWith("command_block"))
				{
					return new TileEntityCommandBlock(blockPos);
				}
				if(blockType.shortID.EndsWith("head") || blockType.shortID.EndsWith("skull"))
				{
					return new TileEntitySkull(blockPos);
				}
				switch(blockType.shortID)
				{
					case "chest":
					case "trapped_chest":
					case "barrel":
						return new TileEntityContainer(blockType.shortID, blockPos, 27);
					case "dispenser":
					case "dropper":
						return new TileEntityContainer(blockType.shortID, blockPos, 9);
					case "hopper":
						return new TileEntityContainer(blockType.shortID, blockPos, 5);
					case "beacon":
						return new TileEntityBeacon(blockPos);
					case "bee_nest":
					case "beehive":
						return new TileEntityBeehive(blockType.shortID, blockPos);
					case "furnace":
					case "blast_furnace":
					case "smoker":
						return new TileEntityFurnace(blockType.shortID, blockPos);
					case "campfire":
					case "soul_campfire":
						return new TileEntityCampfire(blockType.shortID, blockPos);
					case "comparator":
						return new TileEntityComparator(blockPos);
					case "conduit":
						return new TileEntityConduit(blockPos);
					case "end_gateway":
						return new TileEntityEndGateway(blockPos);
					case "jigsaw":
						return new TileEntityJigsaw(blockPos);
					case "jukebox":
						return new TileEntityJukebox(blockPos);
					case "lectern":
						return new TileEntityLectern(blockPos);
					case "mob_spawner":
						return new TileEntitySpawner(blockPos);
					case "moving_piston":
						//TODO: check if this is the correct block id for this.
						return new TileEntityPiston(blockPos);
					case "structure_block":
						return new TileEntityStructureBlock(blockPos);
					case "chiseled_bookshelf":
						return new TileEntityChiseledBookshelf(blockPos);
				}
				return new TileEntityGeneric(blockType.shortID, blockPos);
			}
			else
			{
				return new TileEntityGeneric(blockType.ID, blockPos);
			}
		}

		public NBTCompound ToNBT(Version version)
		{
			NBTCompound nbt = new NBTCompound
			{
				{ "id", id },
				{ "x", blockPos.x },
				{ "y", blockPos.y },
				{ "z", blockPos.z }
			};
			Serialize(nbt, version);
			otherNBTData.Merge(nbt, false);
			return nbt;
		}

		protected abstract void Serialize(NBTCompound nbt, Version version);
	}
}

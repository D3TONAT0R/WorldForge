using System;
using System.Collections.Generic;
using WorldForge.Coordinates;
using WorldForge.Items;
using WorldForge.NBT;

namespace WorldForge.TileEntities
{
	public class TileEntityContainer : TileEntity
	{
		[NBT("Items")]
		public Inventory items = new Inventory();
		[NBT("CustomName")]
		public string customName = null;
		[NBT("Lock")]
		public string lockItemName = null;

		[NBTCollection]
		public LootTableOptions lootTableOptions = new LootTableOptions();

		public readonly int maxSlotCount;

		public override GameVersion AddedInVersion {
			get
			{
				var shortId = id.Replace("minecraft:", "");
				switch(shortId)
				{
					case "chest":
					case "Chest":
						return GameVersion.FirstVersion;
					case "dispenser":
					case "Trap":
						return GameVersion.Beta_1(2);
					case "dropper":
					case "Dropper":
						return GameVersion.Release_1(5);
					default:
						return GameVersion.FirstVersion;
				}
			}
		
		}

		public TileEntityContainer(string id, int maxSlotCount, params (sbyte, ItemStack)[] content) : base(id)
		{
			this.maxSlotCount = maxSlotCount;
			foreach(var c in content)
			{
				items.SetItem(c.Item1, c.Item2);
			}
		}

		public TileEntityContainer(NBTCompound compound, int maxSlotCount, out BlockCoord blockPos) : base(compound, out blockPos)
		{
			this.maxSlotCount = maxSlotCount;
		}

		protected override string ResolveTileEntityID(GameVersion version)
		{
			if(version >= GameVersion.Release_1(11))
			{
				return id;
			}
			else
			{
				var shortId = id.Replace("minecraft:", "");
				if(shortId == "chest") return "Chest";
				if(shortId == "dispenser") return "Trap";
				return shortId;
			}
		}
	}
}

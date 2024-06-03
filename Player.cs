using WorldForge.Items;
using WorldForge.NBT;

namespace WorldForge
{
    public class Player
	{
		public enum GameMode { Survival, Creative, Adventure, Spectator }

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

		[NBT("UUID")]
		public int[] uuid = new int[4];

		[NBT("Dimension")]
		public string dimension = "minecraft:overworld";
		[NBT("Pos")]
		public Vector3 position = new Vector3(0, 0, 0);
		[NBT("Rotation")]
		public Vector2 rotation = new Vector2(0, 0);
		[NBT("Motion")]
		public Vector3 motion = new Vector3(0, 0, 0);

		[NBT("abilities")]
		public Abilities abilities = new Abilities();
		[NBT("playerGameType")]
		public GameMode playerGameType = GameMode.Survival;
		[NBT("Invulnerable")]
		public bool invulnerable = false;
		[NBT("Health")]
		public float health = 20f;
		[NBT("foodLevel")]
		public int foodLevel = 20;
		[NBT("foodSaturationLevel")]
		public float foodSaturationLevel = 5f;
		[NBT("foodExhaustionLevel")]
		public float foodExhaustionLevel = 0f;
		[NBT("Air")]
		public short Air = 300;
		[NBT("AbsorptionAmount")]
		public float absorptionAmount = 0;
		[NBT("seenCredits")]
		public bool seenCredits = false;

		[NBT("Inventory")]
		public Inventory inventory = new Inventory();
		[NBT("SelectedItemSlot")]
		public int selectedItemSlot = 0;
		[NBT("EnderItems")]
		public Inventory enderItems = new Inventory();

		[NBT("XpP")]
		public float xpP = 0f;
		[NBT("XpLevel")]
		public int xpLevel = 0;
		[NBT("XpSeed")]
		public int xpSeed = 25565;
		[NBT("XpTotal")]
		public int xpTotal = 0;
		[NBT("Score")]
		public int Score = 0;

		public Player(Vector3 position)
		{
			this.position = position;
		}

		public Player(NBTCompound nbt)
		{
			NBTConverter.LoadFromNBT(nbt, this);
		}
	}
}

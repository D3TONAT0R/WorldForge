using MCUtils.NBT;

namespace MCUtils
{
	public class Player
	{
		public enum GameMode { Survival, Creative, Adventure, Spectator }

		[NBT("UUID")]
		public int[] uuid;

		[NBT("Dimension")]
		public string dimension;
		[NBT("Pos")]
		public Vector3 position;
		[NBT("Rotation")]
		public Vector2 rotation;
		[NBT("Motion")]
		public Vector3 motion;

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
		public Inventory inventory;
		[NBT("SelectedItemSlot")]
		public int selectedItemSlot;
		[NBT("EnderItems")]
		public Inventory enderItems;

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
			NBTFieldManager.LoadFromNBT(nbt, this);
		}
	}
}

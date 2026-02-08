using System;
using System.Collections.Generic;
using System.Text;
using WorldForge.Items;

namespace WorldForge
{
	/// <summary>
	/// Contains player data for a player in the world.
	/// </summary>
	public class PlayerData
	{
		public readonly UUID uuid;

		public Player player;
		public PlayerStats stats;
		public PlayerAdvancements advancements;

		public PlayerData(string rootDirectory, UUID uuid, GameVersion gameVersionHint)
		{
			this.uuid = uuid;
			string playerDataFile = System.IO.Path.Combine(rootDirectory, "playerdata", uuid.ToString(true) + ".dat");
			string playerStatsFile = System.IO.Path.Combine(rootDirectory, "stats", uuid.ToString(true) + ".json");
			string playerAdvancementsFile = System.IO.Path.Combine(rootDirectory, "advancements", uuid.ToString(true) + ".json");
			player = Player.FromFile(playerDataFile, gameVersionHint);
			if (System.IO.File.Exists(playerStatsFile)) stats = PlayerStats.FromFile(playerStatsFile);
			if (System.IO.File.Exists(playerAdvancementsFile)) advancements = PlayerAdvancements.FromFile(playerAdvancementsFile);
		}

		public PlayerData(Player player, PlayerStats stats, PlayerAdvancements advancements)
		{
			this.player = player;
			this.stats = stats;
			this.advancements = advancements;
		}
	}
}

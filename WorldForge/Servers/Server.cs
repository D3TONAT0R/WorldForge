using System.IO;

namespace WorldForge
{
	public class Server
	{
		public ServerProperties ServerProperties { get; private set; }

		public World World { get; private set; }

		public static Server FromDirectory(string rootDirectory, GameVersion? versionHint)
		{
			var server = new Server();
			server.ServerProperties = ServerProperties.Load(System.IO.Path.Combine(rootDirectory, "server.properties"));
			var worldsRootDirectory = GetWorldRootDirectory(rootDirectory);
			var worldName = server.ServerProperties.LevelName;
			var mainWorld = Path.Combine(worldsRootDirectory, worldName);
			server.World = World.Load(mainWorld, versionHint);
			foreach(var dimension in Directory.GetDirectories(worldsRootDirectory))
			{
				var dimensionName = Path.GetFileName(dimension);
				if(dimensionName == worldName)
				{
					continue; //Already loaded the main world.
				}
				DimensionID dimensionID;
				if(dimensionName == worldName + "_nether")
				{
					dimensionID = DimensionID.Nether;
				}
				else if(dimensionName == worldName + "_the_end")
				{
					dimensionID = DimensionID.TheEnd;
				}
				else
				{
					dimensionID = DimensionID.Temporary("custom:" + dimensionName);
				}
				server.World.Dimensions.Add(dimensionID, Dimension.Load(server.World, worldsRootDirectory, dimensionName, dimensionID, versionHint));
			}
			return server;
		}

		private static string GetWorldRootDirectory(string rootDirectory)
		{
			string worldsRootDirectory = rootDirectory;
			//Check if bukkit.yml exists, if so, use the world directory specified in there.
			string bukkitYmlPath = System.IO.Path.Combine(rootDirectory, "bukkit.yml");
			if(File.Exists(bukkitYmlPath))
			{
				var lines = File.ReadAllLines(bukkitYmlPath);
				foreach(var line in lines)
				{
					if (line.TrimStart().StartsWith("world-container:"))
					{
						var worldContainer = line.Split(':')[1].Trim().Replace("\"", "");
						worldsRootDirectory = System.IO.Path.Combine(rootDirectory, worldContainer);
					}
				}
			}
			return worldsRootDirectory;
		}
	}
}
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
			var worldsRootDirectory = GetWorldRootDirectory(rootDirectory, out var isCustomDirectory);
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
				if(!isCustomDirectory)
				{
					//Check if directory name starts with world name in normal world folders.
					//In custom world folders, assume all directories are dimensions.
					if (!dimensionName.StartsWith(worldName + "_"))
					{
						continue; //Not a dimension folder.
					}
				}
				var dim = Dimension.LoadServerDimension(server.World, worldsRootDirectory, dimensionName, out var dimensionID);
				server.World.Dimensions.Add(dimensionID, dim);
			}
			return server;
		}

		private static string GetWorldRootDirectory(string rootDirectory, out bool isCustomDirectory)
		{
			isCustomDirectory = false;
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
						worldsRootDirectory = Path.Combine(rootDirectory, worldContainer);
						isCustomDirectory = true;
					}
				}
			}
			return worldsRootDirectory;
		}
	}
}
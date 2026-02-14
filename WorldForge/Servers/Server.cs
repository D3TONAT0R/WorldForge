using System.Collections.Generic;
using System.IO;

namespace WorldForge
{
	public class Server
	{
		public ServerProperties ServerProperties { get; private set; }

		public World MainWorld { get; private set; }

		public Dictionary<string, World> Worlds { get; } = new Dictionary<string, World>();

		public static Server FromDirectory(string rootDirectory, GameVersion? versionHint)
		{
			var server = new Server();
			server.ServerProperties = ServerProperties.Load(System.IO.Path.Combine(rootDirectory, "server.properties"));
			var worldsRootDirectory = GetWorldRootDirectory(rootDirectory, out var isCustomDirectory);
			var worldName = server.ServerProperties.LevelName;
			var mainWorld = Path.Combine(worldsRootDirectory, worldName);
			server.MainWorld = World.Load(mainWorld, versionHint);
			server.Worlds.Add(worldName, server.MainWorld);
			foreach (var dir in Directory.GetDirectories(worldsRootDirectory))
			{
				var dirName = Path.GetFileName(dir);
				if(dirName == worldName)
				{
					continue; //Already loaded the main world.
				}
				/*
				if(!isCustomDirectory)
				{
					//Check if directory name starts with world name in normal world folders.
					//In custom world folders, assume all directories are dimensions.
					if (!dirName.StartsWith(worldName + "_"))
					{
						continue; //Not a dimension folder.
					}
				}
				*/
				//Check for the existence of level.dat to confirm it's a world directory.
				if (!File.Exists(Path.Combine(dir, "level.dat")))
				{
					continue; //Not a world directory.
				}
				var world = World.Load(dir, versionHint);
				server.Worlds.Add(world.WorldName, world);
				//var subdir = Path.Combine("..", dirName);
				//var dim = Dimension.LoadServerDimension(server.MainWorld, subdir, out var dimensionID);
				//server.MainWorld.Dimensions.Add(dimensionID, dim);
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
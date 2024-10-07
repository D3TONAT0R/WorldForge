using System;
using System.IO;
using System.Threading.Tasks;
using WorldForge.Regions;

namespace WorldForge.IO
{
	public static class RegionSerializer
	{

		///<summary>Generates a full .mca file stream for use in Minecraft.</summary>
		public static void WriteRegionToStream(Region region, FileStream stream, GameVersion version, bool writeProgressBar = false)
		{
			DateTime time = System.DateTime.Now;
			int[] locations = new int[1024];
			byte[] sizes = new byte[1024];
			for(int i = 0; i < 8192; i++)
			{
				stream.WriteByte(0);
			}

			var chunkSerializer = ChunkSerializer.GetForVersion(version);

			MemoryStream[] serializedChunks = new MemoryStream[1024];
			for(int z = 0; z < 32; z++)
			{
				Parallel.For(0, 32, (int x) =>
				{
					int i = z * 32 + x;
					var chunk = region.chunks[x, z];
					if(chunk != null)
					{
						var memStream = new MemoryStream(4096);
						var chunkData = chunkSerializer.CreateChunkNBT(chunk);

						var dv = version.GetDataVersion();
						if(dv.HasValue) chunkData.contents.Add("DataVersion", dv.Value);

						byte[] compressed = chunkData.WriteBytesZlib();
						var cLength = Converter.ReverseEndianness(BitConverter.GetBytes(compressed.Length));
						memStream.Write(cLength, 0, cLength.Length);
						memStream.WriteByte(2);
						memStream.Write(compressed, 0, compressed.Length);
					}
				});
				if(writeProgressBar) WorldForgeConsole.WriteProgress($"Writing chunks to stream [{z * 32}/{1024}]", (z * 32f) / 1024f);
			}

			for(int i = 0; i < 1024; i++)
			{
				if (serializedChunks[i] == null) continue;

				var serialized = serializedChunks[i];
				var padding = stream.Length % 4096;
				//Pad the data to the next 4096 byte mark
				if(padding > 0)
				{
					byte[] paddingBytes = new byte[4096 - padding];
					stream.Write(paddingBytes, 0, paddingBytes.Length);
				}

				byte size = (byte)Math.Ceiling(serialized.Length / 4096d);
				if(sizes[i] == 0) throw new InvalidOperationException("0 byte sized chunk detected.");
				locations[i] = (int)(stream.Position / 4096);
				//sizes[i] = (byte)((int)(stream.Position / 4096) - locations[i]);
				sizes[i] = size;
			}

			stream.Position = 0;
			for(int i = 0; i < 1024; i++)
			{
				byte[] offsetBytes = Converter.ReverseEndianness(BitConverter.GetBytes(locations[i]));
				stream.WriteByte(offsetBytes[1]);
				stream.WriteByte(offsetBytes[2]);
				stream.WriteByte(offsetBytes[3]);
				stream.WriteByte(sizes[i]);
			}
			DateTime time2 = System.DateTime.Now;
			TimeSpan len = time2.Subtract(time);
			WorldForgeConsole.WriteLine("Generating MCA took " + Math.Round(len.TotalSeconds * 100f) / 100f + "s");
		}
	}
}

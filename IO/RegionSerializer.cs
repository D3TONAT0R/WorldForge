using Ionic.Zlib;
using System;
using System.Collections.Generic;
using System.IO;

namespace MCUtils.IO
{
	public static class RegionSerializer
	{

		///<summary>Generates a full .mca file stream for use in Minecraft.</summary>
		public static void WriteRegionToStream(Region region, FileStream stream, Version version, bool writeProgressBar = false)
		{
			DateTime time = System.DateTime.Now;
			int[] locations = new int[1024];
			byte[] sizes = new byte[1024];
			for (int i = 0; i < 8192; i++)
			{
				stream.WriteByte(0);
			}

			var chunkSerializer = ChunkSerializer.CreateForVersion(version);

			for (int z = 0; z < 32; z++)
			{
				for (int x = 0; x < 32; x++)
				{
					int i = z * 32 + x;
					var chunk = region.chunks[x, z];
					if (chunk != null)
					{
						locations[i] = (int)(stream.Position / 4096);

						var chunkData = chunkSerializer.CreateChunkNBT(chunk, region);

						var dv = version.GetDataVersion();
						if (dv.HasValue) chunkData.contents.Add("DataVersion", dv.Value);

						byte[] compressed = ZlibStream.CompressBuffer(chunkData.WriteBytesZlib());
						var cLength = Converter.ReverseEndianness(BitConverter.GetBytes(compressed.Length));
						stream.Write(cLength, 0, cLength.Length);
						stream.WriteByte(2);
						stream.Write(compressed, 0, compressed.Length);
						var padding = stream.Length % 4096;
						//Pad the data to the next 4096 byte mark
						if (padding > 0)
						{
							byte[] paddingBytes = new byte[4096 - padding];
							stream.Write(paddingBytes, 0, paddingBytes.Length);
						}
						sizes[i] = (byte)((int)(stream.Position / 4096) - locations[i]);
						if (sizes[i] == 0) throw new InvalidOperationException("0 byte sized chunk detected.");
					}
					else
					{
						locations[i] = 0;
						sizes[i] = 0;
					}
				}
				if (writeProgressBar) MCUtilsConsole.WriteProgress(string.Format("Writing chunks to stream [{0}/{1}]", z * 32, 1024), (z * 32f) / 1024f);
			}
			stream.Position = 0;
			for (int i = 0; i < 1024; i++)
			{
				byte[] offsetBytes = Converter.ReverseEndianness(BitConverter.GetBytes(locations[i]));
				stream.WriteByte(offsetBytes[1]);
				stream.WriteByte(offsetBytes[2]);
				stream.WriteByte(offsetBytes[3]);
				stream.WriteByte(sizes[i]);
			}
			DateTime time2 = System.DateTime.Now;
			TimeSpan len = time2.Subtract(time);
			MCUtilsConsole.WriteLine("Generating MCA took " + Math.Round(len.TotalSeconds * 100f) / 100f + "s");
		}
	}
}

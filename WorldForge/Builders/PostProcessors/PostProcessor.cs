using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using WorldForge.Chunks;
using WorldForge.Coordinates;
using WorldForge.Regions;

namespace WorldForge.Builders.PostProcessors
{
	[Flags]
	public enum PostProcessType
	{
		None = 0,
		Block = 1 << 0,
		Surface = 1 << 1
	}

	public enum Priority
	{
		First = 0,
		AfterFirst = 1,
		BeforeDefault = 2,
		Default = 3,
		AfterDefault = 4,
		BeforeLast = 5,
		Last = 6
	}

	public abstract class PostProcessor
	{
		protected static ThreadLocal<Random> random = new ThreadLocal<Random>(() => new Random());

		public virtual Priority OrderPriority => Priority.Default;

		public virtual bool UseMultithreading => true;

		public abstract PostProcessType PostProcessorType { get; }

		public virtual int LowerHeightLimit => int.MinValue;
		public virtual int UpperHeightLimit => int.MaxValue;

		public virtual int PassCount => 1;

		public long Seed { get; private set; }

		private readonly int typeHash;

		public Map<float> mask;

		public PostProcessContext Context { get; set; }

		protected PostProcessor()
		{
			typeHash = GetType().GetHashCode();
		}

		protected PostProcessor(string rootPath, XElement xml) : this()
		{
			var maskElem = xml.Element("mask");
			if(maskElem != null && rootPath != null)
			{
				string maskPath = Path.Combine(rootPath, maskElem.Value);
				var channelAttr = maskElem.Attribute("channel");
				ColorChannel channel;
				string attr = channelAttr?.Value.ToLower();
				switch(attr)
				{
					case "r":
					case "red": channel = ColorChannel.Red; break;
					case "g":
					case "green": channel = ColorChannel.Green; break;
					case "b":
					case "blue": channel = ColorChannel.Blue; break;
					case "a":
					case "alpha": channel = ColorChannel.Alpha; break;
					default: channel = ColorChannel.Red; break;
				}
				mask = Map<float>.CreateSingleChannelMap(maskPath, channel);
			}
		}

		protected Map<float> LoadWeightmap(string rootPath, XElement xml, out XElement weightmapXml)
		{
			weightmapXml = xml.Element("weightmap");
			if(weightmapXml != null)
			{
				string mapFileName = Path.Combine(rootPath, weightmapXml.Attribute("file").Value);
				var weightmap = Map<float>.CreateRGBAMap(mapFileName);
				return weightmap;
			}
			else
			{
				return null;
			}
		}

		public void ProcessBlock(BlockCoord pos, int pass)
		{
			float maskValue = 0;
			if(!mask?.TryGetValue(pos, 0, out maskValue) ?? true) maskValue = 1;
			if(maskValue > 0)
			{
				OnProcessBlock(Context.Dimension, pos, pass, maskValue);
			}
		}

		public void ProcessSurface(BlockCoord pos, int pass)
		{
			float maskValue = 0;
			if(!mask?.TryGetValue(pos, 0, out maskValue) ?? true) maskValue = 1; 
			if(maskValue > 0)
			{
				OnProcessSurface(Context.Dimension, pos, pass, maskValue);
			}
		}

		public void ProcessRegion(Region reg, int pass)
		{
			OnProcessRegion(reg, pass);
		}

		protected void ProcessWeightmapLayersSurface(Dictionary<int, Layer> layers, Map<float> map, Dimension dim, BlockCoord pos, int pass, float mask)
		{
			foreach(var l in layers)
			{
				float layerMask = mask;
				if(l.Key > -1)
				{
					layerMask *= map.GetValue(pos, l.Key);
				}
				if(layerMask > 0.001f)
				{
					l.Value.ProcessBlockColumn(dim, random.Value, pos, layerMask);
				}
			}
		}

		protected virtual void OnBegin()
		{

		}

		protected virtual void OnProcessBlock(Dimension dimension, BlockCoord pos, int pass, float mask)
		{

		}

		protected virtual void OnProcessSurface(Dimension dimension, BlockCoord pos, int pass, float mask)
		{

		}

		protected virtual void OnProcessRegion(Region reg, int pass)
		{

		}

		protected virtual void OnFinish()
		{

		}

		public virtual void OnCreateWorldFiles(string worldFolder)
		{

		}

		public virtual void Process(PostProcessContext context)
		{
			Context = context;
			GenerateSeed(context, 0);
			OnBegin();
			var boundary = context.Boundary;
			var chunkMin = new BlockCoord(boundary.xMin, 0, boundary.zMin).Chunk;
			var chunkMax = new BlockCoord(boundary.xMax, 0, boundary.zMax).Chunk;
			chunkMax.x++;
			chunkMax.z++;
			var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = UseMultithreading ? -1 : 1 };
			for(int pass = 0; pass < PassCount; pass++)
			{
				GenerateSeed(context, pass);
				if(PostProcessorType.HasFlag(PostProcessType.Block))
				{
					//Iterate the postprocessors over every block
					//Run parallel tasks for each chunk to avoid locking
					Parallel.ForEach(context.Dimension.EnumerateChunks(chunkMin, chunkMax), parallelOptions, chunk =>
					{
						int bx = chunk.WorldSpaceCoord.BlockCoord.x;
						int bz = chunk.WorldSpaceCoord.BlockCoord.z;
						for(int z = 0; z < 16; z++)
						{
							for(int x = 0; x < 16; x++)
							{
								TryProcessBlock(chunk, bx, x, bz, z, pass);
							}
						}
					});
				}

				GenerateSeed(context, pass + 313);
				if(PostProcessorType.HasFlag(PostProcessType.Surface))
				{
					//Iterate the postprocessors over every surface block
					//Run parallel tasks for each chunk to avoid locking
					Parallel.ForEach(context.Dimension.EnumerateChunks(chunkMin, chunkMax), parallelOptions, chunk =>
					{
						int bx = chunk.WorldSpaceCoord.BlockCoord.x;
						int bz = chunk.WorldSpaceCoord.BlockCoord.z;
						for(int z = 0; z < 16; z++)
						{
							for(int x = 0; x < 16; x++)
							{
								TryProcessSurface(chunk, bx, x, bz, z, pass);
							}
						}
					});
				}

				GenerateSeed(context, pass + 791);
				//Run every postprocessor once for every region
				int p = pass;
				Parallel.ForEach(context.Dimension.regions.Values, parallelOptions, reg =>
				{
					ProcessRegion(reg, p);
				});
			}
			OnFinish();
			Context = null;
		}

		public void TryProcessSurface(Chunk chunk, int bx, int x, int bz, int z, int pass)
		{
			//TODO: remember height so each processor uses the same height
			var y = chunk.GetHighestBlock(x, z, HeightmapType.SolidBlocksNoLiquid);
			if(y < LowerHeightLimit || y > UpperHeightLimit) return;
			ProcessSurface(new BlockCoord(bx + x, y, bz + z), pass);
		}

		public void TryProcessBlock(Chunk chunk, int bx, int x, int bz, int z, int pass)
		{
			int min = Math.Max(chunk.LowestSection * 16, LowerHeightLimit);
			int max = Math.Min(chunk.HighestSection * 16 + 15, UpperHeightLimit);
			for(int y = min; y <= max; y++)
			{
				ProcessBlock(new BlockCoord(bx + x, y, bz + z), pass);
			}
		}


		private void GenerateSeed(PostProcessContext context, int offset)
		{
			Seed = context.BaseSeed + typeHash + offset * 2039;
		}
	}
}
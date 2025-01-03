using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using WorldForge.Coordinates;
using WorldForge.Regions;

namespace WorldForge.Builders.PostProcessors
{

	public enum PostProcessType
	{
		RegionOnly,
		Block,
		Surface,
		Both
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

		public virtual bool Multithreading => true;

		public abstract PostProcessType PostProcessorType { get; }

		public virtual int BlockProcessYMin => 0;
		public virtual int BlockProcessYMax => 255;

		public virtual int PassCount => 1;

		public long Seed { get; private set; }

		private readonly int typeHash;

		protected int worldOriginOffsetX;
		protected int worldOriginOffsetZ;

		public Weightmap<float> mask;

		public PostProcessContext Context { get; private set; }

		protected PostProcessor()
		{
			typeHash = GetType().GetHashCode();
		}

		protected PostProcessor(string rootPath, XElement xml, int offsetX, int offsetZ, int sizeX, int sizeZ) : this()
		{
			worldOriginOffsetX = offsetX;
			worldOriginOffsetZ = offsetZ;
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
				mask = Weightmap<float>.CreateSingleChannelMap(maskPath, channel, 0, 0, sizeX, sizeZ);
			}
		}

		protected Weightmap<float> LoadWeightmap(string rootPath, XElement xml, int offsetX, int offsetZ, int sizeX, int sizeZ, out XElement weightmapXml)
		{
			weightmapXml = xml.Element("weightmap");
			if(weightmapXml != null)
			{
				string mapFileName = Path.Combine(rootPath, weightmapXml.Attribute("file").Value);
				var weightmap = Weightmap<float>.CreateRGBAMap(mapFileName, offsetX, offsetZ, sizeX, sizeZ);
				return weightmap;
			}
			else
			{
				return null;
			}
		}

		public void ProcessBlock(BlockCoord pos, int pass)
		{
			float maskValue = mask != null ? mask.GetValue(pos.x - worldOriginOffsetX, pos.z - worldOriginOffsetZ) : 1;
			if(maskValue > 0)
			{
				OnProcessBlock(Context.Dimension, pos, pass, maskValue);
			}
		}

		public void ProcessSurface(BlockCoord pos, int pass)
		{
			float maskValue = mask != null ? mask.GetValue(pos.x - worldOriginOffsetX, pos.z - worldOriginOffsetZ) : 1;
			if(maskValue > 0)
			{
				OnProcessSurface(Context.Dimension, pos, pass, maskValue);
			}
		}

		public void ProcessRegion(Region reg, int pass)
		{
			OnProcessRegion(reg, pass);
		}

		protected void ProcessWeightmapLayersSurface(Dictionary<int, Layer> layers, Weightmap<float> weightmap, Dimension dim, BlockCoord pos, int pass, float mask)
		{
			foreach(var l in layers)
			{
				float layerMask = mask;
				if(l.Key > -1)
				{
					layerMask *= weightmap.GetValue(pos.x, pos.z, l.Key);
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

		public void Process(PostProcessContext context)
		{
			Context = context;
			GenerateSeed(context, 0);
			OnBegin();
			var boundary = context.Boundary;
			var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Multithreading ? -1 : 1 };
			for(int pass = 0; pass < PassCount; pass++)
			{
				GenerateSeed(context, pass);
				if(PostProcessorType == PostProcessType.Block || PostProcessorType == PostProcessType.Both)
				{
					//Iterate the postprocessors over every block
					Parallel.For(boundary.zMin, boundary.zMax + 1, parallelOptions, z =>
					{
						for(int x = boundary.xMin; x <= boundary.xMax; x++)
						{
							for(int y = BlockProcessYMin; y <= BlockProcessYMax; y++)
							{
								ProcessBlock(new BlockCoord(x, y, z), pass);
							}
						}
					});
				}

				GenerateSeed(context, pass + 313);
				if(PostProcessorType == PostProcessType.Surface || PostProcessorType == PostProcessType.Both)
				{
					//Iterate the postprocessors over every surface block
					Parallel.For(boundary.zMin, boundary.zMax + 1, parallelOptions, z =>
					{
						for(int x = boundary.xMin; x <= boundary.xMax; x++)
						{
							//TODO: remember height so each processor uses the same height
							ProcessSurface(new BlockCoord(x, context.Dimension.GetHighestBlock(x, z, HeightmapType.SolidBlocksNoLiquid), z), pass);
						}
						//UpdateProgressBar(processorIndex, "Decorating surface", name, (x + 1) / (float)heightmapLengthX, pass, post.NumberOfPasses);
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

		private void GenerateSeed(PostProcessContext context, int offset)
		{
			Seed = context.BaseSeed + typeHash + offset * 2039;
		}
	}
}
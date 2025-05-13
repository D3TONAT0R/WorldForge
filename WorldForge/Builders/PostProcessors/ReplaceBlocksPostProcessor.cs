using System.Collections.Generic;
using System.Xml.Linq;
using WorldForge.Coordinates;

namespace WorldForge.Builders.PostProcessors
{
	public class ReplaceBlocksPostProcessor : PostProcessor
	{
		public class Mapping
		{
			public BlockState from;
			public BlockState to;
			public bool compareProperties;

			public Mapping(BlockState from, BlockState to, bool compareProperties = false)
			{
				this.from = from;
				this.to = to;
				this.compareProperties = compareProperties;
			}

			public bool Check(BlockState state)
			{
				if(state == null) return false;
				return from.Compare(state, compareProperties);
			}
		}

		public List<Mapping> mappings = new List<Mapping>();

		public override PostProcessType PostProcessorType => PostProcessType.Block;

		public ReplaceBlocksPostProcessor(params (BlockState, BlockState, bool)[] blocks)
		{
			mappings = new List<Mapping>();
			foreach(var (from, to, compareProperties) in blocks)
			{
				mappings.Add(new Mapping(from, to, compareProperties));
			}
		}

		public ReplaceBlocksPostProcessor(params (BlockID, BlockID)[] blocks)
		{
			mappings = new List<Mapping>();
			foreach(var (from, to) in blocks)
			{
				mappings.Add(new Mapping(BlockState.Simple(from), BlockState.Simple(to), false));
			}
		}

		public ReplaceBlocksPostProcessor(XElement xml, string rootPath) : base(rootPath, xml)
		{
			mappings = new List<Mapping>();
			foreach(var mapping in xml.Elements("mapping"))
			{
				BlockState from = BlockState.Parse(mapping.Attribute("from").Value);
				BlockState to = BlockState.Parse(mapping.Attribute("to").Value);
				bool compareProperties = mapping.Attribute("compareProperties")?.Value == "true";
				mappings.Add(new Mapping(from, to, compareProperties));
			}
		}

		protected override void OnProcessBlock(Dimension dimension, BlockCoord pos, int pass, float mask)
		{
			foreach(var mapping in mappings)
			{
				if(mapping.Check(dimension.GetBlockState(pos)))
				{
					dimension.SetBlock(pos, mapping.to);
					break;
				}
			}
		}
	}
}
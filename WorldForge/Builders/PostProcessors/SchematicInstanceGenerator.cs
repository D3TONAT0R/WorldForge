using System;
using System.Xml.Linq;
using WorldForge.Coordinates;
using WorldForge.Structures;

namespace WorldForge.Builders.PostProcessors
{
	public class SchematicInstanceGenerator : PostProcessor
	{

		private float probabilityPerChunk;
		private ISchematic schematic;
		private BlockState block;
		private bool isPlant;

		public override PostProcessType PostProcessorType => PostProcessType.Surface;

		public SchematicInstanceGenerator(ISchematic schem, float probabilityPerChunk, bool doPlantCheck)
		{
			this.probabilityPerChunk = probabilityPerChunk;
			schematic = schem;
			isPlant = doPlantCheck;
		}

		public SchematicInstanceGenerator(string blockID, float probabilityPerChunk, bool doPlantCheck)
		{
			this.probabilityPerChunk = probabilityPerChunk;
			block = new BlockState(blockID);
			isPlant = doPlantCheck;
		}

		public SchematicInstanceGenerator(string rootPath, XElement xml) : base(rootPath, xml)
		{
			var amount = 1f;
			xml.TryParseFloatAttribute("amount", ref amount);
			isPlant = true;
			xml.TryParseBoolAttribute("plant-check", ref isPlant);
			var schem = xml.Attribute("schem");
			var block = xml.Attribute("block");
			if(schem != null)
			{
				schematic = new SchematicRef(schem.Value);
			}
			else if(block != null)
			{
				this.block = new BlockState(block.Value);
			}
			else
			{
				throw new ArgumentException("block/schematic generator has missing arguments (must have either 'block' or 'schem')");
			}
		}

		protected override void OnProcessSurface(Dimension dimension, BlockCoord pos, int pass, float mask)
		{
			if(isPlant && (!Blocks.IsPlantSustaining(dimension.GetBlock(pos)) || !dimension.IsAirOrNull(pos.Above))) return;

			if(SeededRandom.Probability(probabilityPerChunk / 128f, Seed, pos))
			{
				if(schematic != null)
				{
					schematic.GetSchematic(Context).Build(dimension, pos.Above, Seed, false);
				}
				else
				{
					dimension.SetBlock(pos.Above, block);
				}
			}
		}
	}
}
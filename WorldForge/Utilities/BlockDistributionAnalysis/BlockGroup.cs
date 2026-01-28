using System.Collections.Generic;
using System.Linq;

namespace WorldForge.Utilities.BlockDistributionAnalysis
{
    public class BlockGroup
    {
        public string name;
        public BlockID[] blocks;

        public BlockGroup(string name, params BlockID[] blocks)
        {
            this.name = name;
            this.blocks = blocks;
        }

        public static BlockGroup Parse(string s)
        {
            var split = s.Split(';');
            var blocks = new List<BlockID>(split[1].Split(',').Select(s1 => BlockList.Find(s1)));
            blocks.RemoveAll(b => b == null);
            return new BlockGroup(split[0], blocks.ToArray());
        }
    }
}
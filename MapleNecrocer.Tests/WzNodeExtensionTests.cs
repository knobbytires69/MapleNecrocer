using MapleNecrocer;
using WzComparerR2.WzLib;
using Xunit;

namespace MapleNecrocer.Tests
{
    public class WzNodeExtensionTests
    {
        [Fact]
        public void GetBmp_NullNode_ReturnsNull()
        {
            // The reported crash was a NullReferenceException from GetNodeA returning null
            // for "Character/<dir><id>.img"; GetBmp must not throw for a null node.
            var bmp = Wz_NodeExtension3.GetBmp(null, "info/icon");
            Assert.Null(bmp);
        }

        [Fact]
        public void GetBmp_MissingPath_OnEmptyTree_ReturnsNull()
        {
            var root = new Wz_Node("root");
            var bmp = Wz_NodeExtension3.GetBmp(root, "info/icon");
            Assert.Null(bmp);
        }

        [Fact]
        public void GetBmp_MissingSubPath_ReturnsNull()
        {
            // The img exists but the sub-node (e.g. info/icon) is missing.
            var root = new Wz_Node("root");
            var info = new Wz_Node("info");
            root.Nodes.Add(info);

            var bmp = Wz_NodeExtension3.GetBmp(root, "info/icon");
            Assert.Null(bmp);
        }
    }
}

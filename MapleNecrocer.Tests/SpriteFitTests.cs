using MapleNecrocer;
using Xunit;

namespace MapleNecrocer.Tests
{
    public class SpriteFitTests
    {
        [Theory]
        [InlineData(200, 400, 512, 512, 1f)]     // fits -> no scaling
        [InlineData(600, 400, 512, 512, 510f / 600f)] // too wide -> scale by width
        [InlineData(400, 600, 512, 512, 510f / 600f)] // too tall -> scale by height
        [InlineData(600, 600, 512, 512, 510f / 600f)] // both too big -> min ratio
        [InlineData(512, 512, 512, 512, 510f / 512f)] // exactly frame size -> margin applied
        [InlineData(0, 400, 512, 512, 1f)]       // zero width -> no scaling
        [InlineData(200, 0, 512, 512, 1f)]       // zero height -> no scaling
        [InlineData(100, 100, 128, 128, 1f)]     // smaller target still fits
        public void FitScale_FitsOrScales(int w, int h, int maxW, int maxH, float expected)
        {
            Assert.Equal(expected, SpriteFit.FitScale(w, h, maxW, maxH), 3);
        }

        [Fact]
        public void FitScale_NeverUpscales()
        {
            // A small sprite must not be enlarged to fill a large frame.
            Assert.Equal(1f, SpriteFit.FitScale(64, 64, 512, 512));
        }

        [Fact]
        public void FootprintOverflows_WhenPositionPushesOut()
        {
            // 512-wide sprite positioned off to the right -> overflows even though
            // width <= frame width.
            Assert.True(SpriteFit.FootprintOverflows(256, 0, 512, 100, 512, 512));
        }

        [Fact]
        public void FootprintOverflows_FalseWhenInsideFrame()
        {
            Assert.False(SpriteFit.FootprintOverflows(100, 100, 200, 300, 512, 512));
        }

        [Fact]
        public void FootprintOverflows_TrueWhenNegative()
        {
            Assert.True(SpriteFit.FootprintOverflows(-10, 0, 200, 300, 512, 512));
        }
    }
}

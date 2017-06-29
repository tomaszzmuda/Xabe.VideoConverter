using Xunit;

namespace Xabe.VideoConverter.Test
{
    public class
        NameNormalizerTests
    {
        [Theory]
        [InlineData("13.Reasons.Why.S01E02.720p.WEBRip.X264-DEFLATE[ettv].mp4", "13 Reasons Why S01E02")]
        [InlineData("Children.of.the.Corn.1984.720p.BluRay.x264-DON.mp4", "Children of the Corn (1984)")]
        [InlineData("Lucifer S01E01 HDTV XviD-AFG.mp4", "Lucifer S01E01")]
        [InlineData("The.Fate.of.the.Furious.2017.720p.HDTC.x264.ShAaNiG.mp4", "The Fate of the Furious (2017)")]
        [InlineData("[pseudo] Rick and Morty S01E01 - Pilot [1080p] [h.265].mkv", "Rick and Morty S01E01")]
        [InlineData("Fear.the.Walking.Dead.S03E04.720p.HDTV.x264-FLEET.mkv", "Fear the Walking Dead S03E04")]
        [InlineData("John.Wick.Chapter.2.2017.HDRip.XVid..Line-NoGrp.avi", "John Wick Chapter 2 (2017)")]
        [InlineData("byger-20030708-liten.avi", "byger-20030708-liten")]
        public void NormalizeName(string input, string expectedOutput)
        {
            string normalizedName = NameNormalizer.GetNormalizedName(input);
            Assert.Equal(normalizedName, expectedOutput);
        }
    }
}

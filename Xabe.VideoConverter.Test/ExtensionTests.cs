using Xunit;

namespace Xabe.VideoConverter.Test
{
    public class ExtensionTests
    {
        [Theory]
        [InlineData("13.Reasons.Why.S01E01.720p.WEBRip.X264-DEFLATE[ettv].mp4", true)]
        [InlineData("Beauty.And.The.Beast.2017.1080p.HDRip.X264.AC3-EVO[EtHD].mp4", false)]
        [InlineData("Hush.2016.HDRip.XviD.AC3-EVO.mp4", false)]
        [InlineData("Lucifer S01E01 HDTV XviD-AFG.mp4", true)]
        public void IsTvShow(string input, bool expectedResult)
        {
            bool isTvShow = input.IsTvShow();
            Assert.Equal(isTvShow, expectedResult);
        }

        [Theory]
        [InlineData("13 Reasons Why S01E01", "13 Reasons Why")]
        [InlineData("Lucifer S01E01", "Lucifer")]
        public void RemoveTvShowInfo(string input, string expectedResult)
        {
            string nameWithoutInfo = input.RemoveTvShowInfo();
            Assert.Equal(nameWithoutInfo, expectedResult);
        }

        [Theory]
        [InlineData("13 Reasons Why S01E01", 1)]
        [InlineData("Lucifer S04E01", 4)]
        public void GetSeason(string input, int expectedSeason)
        {
            int season = input.GetSeason();
            Assert.Equal(season, expectedSeason);
        }

        [Theory]
        [InlineData("13 Reasons Why S01E01", "S01E01")]
        [InlineData("Lucifer S04E01", "S04E01")]
        public void GetTvShowInfo(string input, string expectedResult)
        {
            string tvInfo = input.GetTvShowInfo();
            Assert.Equal(tvInfo, expectedResult);
        }

        [Theory]
        [InlineData("Beauty And The Beast 2017.mp4", "2017")]
        [InlineData("Hush 2016.mp4", "2016")]
        public void GetYear(string input, string expectedYear)
        {
            string year = input.GetYear();
            Assert.Equal(year, expectedYear);
        }

        [Theory]
        [InlineData("Beauty And The Beast 2017.avi", "Beauty And The Beast 2017.mp4")]
        [InlineData("Hush 2016.mkv", "Hush 2016.mp4")]
        public void ChangeExtension(string input, string expectedResult)
        {
            string newName = input.ChangeExtension();
            Assert.Equal(newName, expectedResult);
        }
    }
}

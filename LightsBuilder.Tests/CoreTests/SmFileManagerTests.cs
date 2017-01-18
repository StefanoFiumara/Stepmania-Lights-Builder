using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LightsBuilder.Core.Data;
using LightsBuilder.Core.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LightsBuilder.Tests.CoreTests
{
    [TestClass]
    public class SmFileManagerTests
    {
        private SmFileManager SmFile { get; set; }

        [TestInitialize]
        public void Setup()
        {
            this.SmFile = new SmFileManager(new FileInfo("../../TestData/BUTTERFLY.sm"));
        }

        [TestCleanup]
        public void CleanUp()
        {
            var backupFiles = Directory.GetFiles("../../TestData/").Where(f => f.EndsWith(".backup")).Select(f => new FileInfo(f));

            foreach (var backupFile in backupFiles)
            {
                var correspondingSmFile = new FileInfo(backupFile.FullName.Replace(".backup", string.Empty));

                if(correspondingSmFile.Exists) File.Delete(correspondingSmFile.FullName);

                File.Copy(backupFile.FullName, correspondingSmFile.FullName, true);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void NonExistentFileTest()
        {
            this.SmFile = new SmFileManager("TestData/wrongFile.sm");
        }

        [TestMethod]
        public void GetTitleAttributeTest()
        {
            string value = this.SmFile.GetAttribute(SmFileAttribute.TITLE);

            Assert.IsTrue(string.IsNullOrEmpty(value) == false);
        }

        [TestMethod]
        public void GetNonExistentAttributeTest()
        {
            string value = this.SmFile.GetAttribute(SmFileAttribute.TIMESIGNATURES);

            Assert.IsTrue(string.IsNullOrEmpty(value));
        }

        [TestMethod]
        public void GetDifficultyRatingTest()
        {
            int rating = this.SmFile.GetDifficultyRating(PlayStyle.Single, SongDifficulty.Challenge);

            Assert.IsTrue(rating == 8);
        }

        [TestMethod]
        public void GetNonExistentDifficultyRatingTest()
        {
            int rating = this.SmFile.GetDifficultyRating(PlayStyle.Double, SongDifficulty.Beginner);

            Assert.IsTrue(rating == 0);
        }


        [TestMethod]
        public void GetAllDifficultyRatingsTest()
        {
            Dictionary<SongDifficulty, int> ratings = this.SmFile.GetAllDifficultyRatings(PlayStyle.Single);

            Assert.IsTrue(ratings.Keys.Count == 5);
        }

        [TestMethod]
        public void GetHighestDifficultyTest()
        {
            var songDifficulty = this.SmFile.GetHighestChartedDifficulty(PlayStyle.Single);

            Assert.IsTrue(songDifficulty == SongDifficulty.Challenge);
        }

        [TestMethod]
        public void GetChartDataTest()
        {
            var data = this.SmFile.GetChartData(PlayStyle.Single, SongDifficulty.Hard);

            Assert.IsTrue(data.Count == 399);
        }

        [TestMethod]
        public void AddLightsDataTest()
        {
            this.SmFile.AddLightsData();

            var lightsData = this.SmFile.GetChartData(PlayStyle.Lights);

            Assert.IsTrue(lightsData != null);
        }

        [TestMethod]
        public void SaveChartDataTest()
        {
            var hasLightsDataBeforeSave = this.SmFile.GetChartData(PlayStyle.Lights) != null;

            this.SmFile.AddLightsData();

            this.SmFile.SaveChanges();

            this.SmFile = new SmFileManager(new FileInfo("../../TestData/BUTTERFLY.sm"));

            var hasLightsDataAfterSave = this.SmFile.GetChartData(PlayStyle.Lights) != null;

            Assert.IsFalse(hasLightsDataBeforeSave);
            Assert.IsTrue(hasLightsDataAfterSave);
        }
    }
}

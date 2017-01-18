using System;
using System.Linq;
using LightsBuilder.Core.Parsers;
using LightsBuilder.Core.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LightsBuilder.Tests.CoreTests
{
    [TestClass]
    public class StepmaniaStructureParserTests
    {
        private const string TEST_SONG_DIRECTORY = "../../TestData/TestSongPack";
        private const string TEST_NON_EXISTENT_DIRECTORY = "../../TestData/thisdirdoesnotexist";
        private const string TEST_EMPTY_DIRECTORY = "../../TestData/TestEmptyDir";
        private const string TEST_MULTIPLE_DIFFICULTIES = "../../TestData/TestSongPackMultipleDifficulties";
        private const string TEST_NO_EXPERT_DIFFICULTY = "../../TestData/TestSongPackNoExpert";

        private SongsDirectoryParser Parser { get; set; }

        [TestInitialize]
        public void Setup()
        {
            this.Parser = new SongsDirectoryParser();
        }

        #region ParseStepmaniaGroupDirectoryTests

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ParseNonExistentDirectory()
        {
            this.Parser.ExtractSongGroupData(TEST_NON_EXISTENT_DIRECTORY);
        }

        [TestMethod]
        public void ParseEmptyDirectory()
        {
            var result = this.Parser.ExtractSongGroupData(TEST_EMPTY_DIRECTORY).ToList();

            Assert.IsTrue(result.Count == 0);
        }

        [TestMethod]
        public void ParseSongDirectoryOnlyExpert()
        {
            //Parse the Easy As Pie 1 song pack, contains 15 song folders.
            var result = this.Parser.ExtractSongGroupData(TEST_SONG_DIRECTORY).ToList();

            Assert.IsTrue(result.Count == 15);

            //Verify song data was populated
            foreach (var songData in result)
            {
                Assert.IsTrue(songData.IsValid());
            }
        }

        [TestMethod]
        public void ParseSongDirectoryMultipleDifficulties()
        {
            //Parse the Skittles Selection song pack, contains 21 song folders.
            var result = this.Parser.ExtractSongGroupData(TEST_MULTIPLE_DIFFICULTIES).ToList();

            Assert.IsTrue(result.Count == 21);

            //Verify song data was populated
            foreach (var songData in result)
            {
                Assert.IsTrue(songData.IsValid());
            }
        }

        [TestMethod]
        public void ParseSongDirectoryNoExpertDifficulty()
        {
            //Parse the DDR 1st Mix song pack, contains 9 song folders.
            var result = this.Parser.ExtractSongGroupData(TEST_NO_EXPERT_DIFFICULTY).ToList();

            Assert.IsTrue(result.Count == 9);

            //Verify song data was populated
            foreach (var songData in result)
            {
                Assert.IsTrue(songData.IsValid());
            }
        }

        #endregion

        [TestMethod]
        public void ParseStepmaniaSongsFolder()
        {
            var result = this.Parser.ExtractSongsDirectoryData("../../TestData");

            var groupedResult = result.GroupBy(song => song.SongGroup).ToDictionary(k => k.Key, e => e.ToList());

            //We parsed three song groups, verify that songs are grouped appropriately
            Assert.IsTrue(groupedResult.Count == 3);
        }

        [TestMethod]
        public void TestSavedSongGroupNameChange()
        {
            //Parse the Skittles Selection song pack, contains 21 song folders.
            var result = this.Parser.ExtractSongGroupData(TEST_MULTIPLE_DIFFICULTIES).ToList();

            Assert.IsTrue(result.Count == 21);

            foreach (var songData in result)
            {
                Assert.IsTrue(songData.SongGroup == "TestSongPackMultipleDifficulties");
            }

            var repo = new SongDataRepository("../../TestData/TemporaryRepo.xml");

            foreach (var songData in result)
            {
                repo.Add(songData);
            }

            var loadedData = repo.GetAll();

            foreach (var songData in loadedData)
            {
                Assert.IsTrue(songData.SongGroup == "TestSongPackMultipleDifficulties");
            }
        }
    }
}
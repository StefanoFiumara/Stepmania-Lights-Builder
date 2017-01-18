using System;
using System.IO;
using System.Linq;
using LightsBuilder.Core.Data;
using LightsBuilder.Core.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LightsBuilder.Tests.CoreTests
{
    [TestClass]
    public class DataAccessTests
    {
        private IRepository<SongData> Repository { get; set; }
        private const string TEST_XML_FILE_NAME = "../../TestData/TestSongRepository.xml";

        [TestInitialize]
        public void Setup()
        {
            this.Repository = new SongDataRepository(TEST_XML_FILE_NAME);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(TEST_XML_FILE_NAME))
            {
                File.Delete(TEST_XML_FILE_NAME);
            }
        }

        [TestMethod]
        public void LoadEmptyFileTest()
        {
            var emptyData = this.Repository.GetAll().ToList();

            Assert.IsTrue(emptyData.Count == 0);
        }

        [TestMethod]
        public void AddSongDataTest()
        {
            var data = new SongData();

            this.Repository.Add(data);
            var allSongs = this.Repository.GetAll().ToList();

            Assert.IsTrue(allSongs.Count != 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddNullSongDataTest()
        {
            this.Repository.Add(null);
        }

        [TestMethod]
        public void RemoveSongDataTest()
        {
            var data = new SongData();

            this.Repository.Add(data);
            var allSongs = this.Repository.GetAll().ToList();
            Assert.IsTrue(allSongs.Count != 0);

            this.Repository.Remove(data);

            allSongs = this.Repository.GetAll().ToList();
            Assert.IsTrue(allSongs.Count == 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemoveNullSongDataTest()
        {
            this.Repository.Remove(null);
        }

        [TestMethod]
        public void UpdateSongDataTest()
        {
            var data = new SongData();

            this.Repository.Add(data);

            data.SongName = "Uber Rave";

            this.Repository.Update(data);

            var storedData = this.Repository.GetAll().First(song => song.Id == data.Id);

            Assert.IsTrue(data.SongName == storedData.SongName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UpdateNullSongDataTest()
        {
            this.Repository.Update(null);
        }
    }
}

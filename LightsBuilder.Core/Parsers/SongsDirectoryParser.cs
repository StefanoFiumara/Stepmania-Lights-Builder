using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LightsBuilder.Core.Data;

namespace LightsBuilder.Core.Parsers
{
    public class SongsDirectoryParser
    {
        public List<SmFileManager> GetSmFiles(string stepmaniaSongsFolderPath)
        {
            if (Directory.Exists(stepmaniaSongsFolderPath) == false)
                throw new ArgumentException("Directory passed to GetSmFiles does not exist");

            var songGroupDirectories = Directory.GetDirectories(stepmaniaSongsFolderPath);

            var result = new ConcurrentBag<SmFileManager>();

            Parallel.ForEach(songGroupDirectories, directory =>
            {
                var smFiles = this.GetSmFilesForGroup(directory);

                foreach (var smFile in smFiles)
                {
                    result.Add(smFile);
                }
            });

            return result.ToList();
        }

        private List<SmFileManager> GetSmFilesForGroup(string songGroupFolderPath)
        {
            if (Directory.Exists(songGroupFolderPath) == false)
                throw new ArgumentException("Directory passed to GetSmFilesForGroup does not exist");

            var songFolders = Directory.GetDirectories(songGroupFolderPath);

            var result = new ConcurrentBag<SmFileManager>();

            Parallel.ForEach(songFolders, folder =>
            {
                string smFilePath = Directory.GetFiles(folder).SingleOrDefault(file => file.EndsWith(".sm"));
                if (smFilePath == null) return;

                var smFile = new SmFileManager(smFilePath);
                result.Add(smFile);
            });

            return result.ToList();
        }

        public List<SongData> ExtractSongGroupData(string songGroupFolderPath)
        {
            if (Directory.Exists(songGroupFolderPath) == false)
                throw new ArgumentException("Directory passed to ExtractSongGroupData does not exist");

            var songFolders = Directory.GetDirectories(songGroupFolderPath);

            var result = new ConcurrentBag<SongData>();

            Parallel.ForEach(songFolders, folder =>
            {
                var songData = this.ExtractSongData(folder);
                if (songData != null) result.Add(songData);
            });

            return result.ToList();
        }

        public List<SongData> ExtractSongsDirectoryData(string stepmaniaSongsFolderPath)
        {
            if (Directory.Exists(stepmaniaSongsFolderPath) == false)
                throw new ArgumentException("Directory passed to ExtractSongsDirectoryData does not exist");

            var songGroupDirectories = Directory.GetDirectories(stepmaniaSongsFolderPath);

            var result = new ConcurrentBag<SongData>();

            Parallel.ForEach(songGroupDirectories, directory =>
            {
                var parsedSongData = this.ExtractSongGroupData(directory);

                foreach (var song in parsedSongData)
                {
                    result.Add(song);
                }

            });

            return result.ToList();
        }

        private SongData ExtractSongData(string songFolderPath)
        {
            var songData = new SongData();

            string smFilePath = Directory.GetFiles(songFolderPath).SingleOrDefault(file => file.EndsWith(".sm"));

            if (smFilePath == null)
            {
                return null;
            }

            var smFile = new SmFileManager(smFilePath);

            string bannerPath = smFile.GetAttribute(SmFileAttribute.BANNER);
            string relativeBannerPath = Path.Combine(songFolderPath, bannerPath);

            songData.SongName = smFile.GetAttribute(SmFileAttribute.TITLE);
            songData.SongBannerPath = Path.GetFullPath(relativeBannerPath);
            songData.SongGroup = Path.GetFileName(Path.GetDirectoryName(songFolderPath));
            songData.DifficultySingles = smFile.GetAllDifficultyRatings(PlayStyle.Single);
            songData.DifficultyDoubles = smFile.GetAllDifficultyRatings(PlayStyle.Double);

            return songData;
        }
    }
}

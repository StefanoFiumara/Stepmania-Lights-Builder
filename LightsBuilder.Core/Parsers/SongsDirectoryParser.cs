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
        /// <summary>
        /// Searches through the path given and all its subdirectories for *.sm files
        /// </summary>
        /// <param name="rootPath">The root directory in which to begin the search</param>
        /// <returns>A list of SmFileManager objects to manage each chart that was found</returns>
        public List<SmFileManager> FindSmFiles(string rootPath)
        {
            if (Directory.Exists(rootPath) == false)
                throw new ArgumentException("Directory passed to GetSmFiles does not exist");

            return Directory.GetFiles(rootPath, "*.sm", SearchOption.AllDirectories)
                            .Select(p => new SmFileManager(p))
                            .ToList();
        }

        /// <summary>
        /// Searches through the path given and all its subdirectories for *.sm files and extracts their basic metadata.
        /// </summary>
        /// <param name="rootPath">The root directory in which to begin the search</param>
        /// <returns>A list of SongData objects containing basic information about each song</returns>
        public List<SongData> GetSongData(string rootPath)
        {
            if (Directory.Exists(rootPath) == false)
                throw new ArgumentException("Directory passed to ExtractSongsDirectoryData does not exist");

            return Directory.GetFiles(rootPath, "*.sm", SearchOption.AllDirectories)
                            .Select(this.ExtractSongData)
                            .ToList();
        }

        private SongData ExtractSongData(string smFilePath)
        {
            var smFileInfo = new FileInfo(smFilePath);

            if (smFileInfo.Exists == false) return null; 

            var songData = new SongData();

            var smFile = new SmFileManager(smFilePath);

            string bannerPath = smFile.GetAttribute(SmFileAttribute.BANNER);
            string relativeBannerPath = Path.Combine(smFileInfo.DirectoryName, bannerPath);

            songData.SongName = smFile.GetAttribute(SmFileAttribute.TITLE);
            songData.SongBannerPath = Path.GetFullPath(relativeBannerPath);
            songData.SongGroup = smFileInfo.Directory?.Parent?.Name;
            songData.DifficultySingles = smFile.GetAllDifficultyRatings(PlayStyle.Single);
            songData.DifficultyDoubles = smFile.GetAllDifficultyRatings(PlayStyle.Double);

            return songData;
        }
    }
}

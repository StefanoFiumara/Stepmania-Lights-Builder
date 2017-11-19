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
        public IEnumerable<SmFileManager> FindSmFiles(string rootPath)
        {
            if (Directory.Exists(rootPath) == false)
                throw new ArgumentException("Directory passed to FindSmFiles does not exist");

            IEnumerable<SmFileManager> smFiles = Directory.GetFiles(rootPath, "*.sm", SearchOption.AllDirectories)
                .AsParallel()
                .Select(p => new SmFileManager(p));

			IEnumerable<SmFileManager> sscFiles = Directory.GetFiles(rootPath, "*.ssc", SearchOption.AllDirectories)
				.AsParallel()
				.Select(p => new SmFileManager(p));

            return smFiles.Concat(sscFiles);
        }
    }
}

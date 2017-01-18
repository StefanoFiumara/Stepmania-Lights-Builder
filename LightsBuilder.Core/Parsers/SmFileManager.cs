using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LightsBuilder.Core.Data;

namespace LightsBuilder.Core.Parsers
{
    public class SmFileManager
    {
        private List<string> FileContent { get; }

        private readonly FileInfo _smFileInfo;

        public string SongFolderName =>this._smFileInfo?.Directory?.Parent?.Name;

        public SmFileManager(string smFilePath) : this( new FileInfo(smFilePath) )
        {
            
        }

        public SmFileManager(FileInfo smFile)
        {
            this._smFileInfo = smFile;

            if (this._smFileInfo.Exists == false || this._smFileInfo.Extension != ".sm")
            {
                throw new ArgumentException($"The given .sm file path is either invalid or a file was not found. Path: {this._smFileInfo.FullName}");
            }

            this.FileContent = File.ReadAllLines(this._smFileInfo.FullName).ToList();
        }

        public string GetAttribute(SmFileAttribute attribute)
        {
            string attributeName = attribute.ToString();

            string attributeLine = this.FileContent.FirstOrDefault(line => line.Contains($"#{attributeName}:"));

            if (attributeLine != null)
            {
                return attributeLine
                    .Replace($"#{attributeName}:", string.Empty)
                    .TrimEnd(';');
            }

            return string.Empty;
        }

        //TODO: Refactor ChartData into its own class
        public List<string> GetChartData(PlayStyle style, SongDifficulty difficulty = SongDifficulty.Easy)
        {
            if(style == PlayStyle.Lights) difficulty = SongDifficulty.Easy;

            string difficultyName = $"{difficulty}:";
            string styleName = style.ToStyleName();

            for (int i = 0; i < this.FileContent.Count; i++)
            {
                string line = this.FileContent[i];
                //find the line with the difficulty name
                if (line.Contains(difficultyName) == false) continue;

                //check the style
                string styleLine = this.FileContent[i - 2]; //the style is defined two lines above the difficulty
                if (styleLine.Contains(styleName) == false) continue;

                int chartDataStartIndex = i + 3;

                while (string.IsNullOrWhiteSpace(this.FileContent[chartDataStartIndex])) chartDataStartIndex++;

                int chartDataEndIndex = chartDataStartIndex;

                while (this.FileContent[chartDataEndIndex].Contains(';') == false) chartDataEndIndex++;

                return this.FileContent
                           .Skip(chartDataStartIndex)
                           .Take(chartDataEndIndex - chartDataStartIndex)
                           .ToList();
            }

            return null;
        }

        public void AddLightsData()
        {
            if (this.GetChartData(PlayStyle.Lights) != null)
            {
                return;
                //throw new InvalidOperationException("This file already contains Lights Data");
                //TODO: Log this to a file
            }

            var referenceChart = this.GetChartData(PlayStyle.Single, SongDifficulty.Hard) ??
                                 this.GetChartData(PlayStyle.Single, this.GetHighestChartedDifficulty(PlayStyle.Single)) ??
                                 this.GetChartData(PlayStyle.Double, SongDifficulty.Hard) ??
                                 this.GetChartData(PlayStyle.Double, this.GetHighestChartedDifficulty(PlayStyle.Double));

            if (referenceChart == null)
            {
                return;
                //throw new InvalidOperationException($"The chart located at {this._smFileInfo.FullName} does not contain valid chart data for singles or doubles play, so a lights chart cannot be generated.");
                //TODO: Log this to a file
            }
            var referenceChartStyle = referenceChart
                .First(beat => !string.IsNullOrWhiteSpace(beat) && !beat.Contains("//"))
                .Length == 4 ? PlayStyle.Single : PlayStyle.Double;

            var lightsData = new List<string>
            {
                @"//---------------lights-cabinet-----------------",
                @"#NOTES:",
                "\tlights-cabinet:",
                "\t:",
                "\tEasy:",
                "\t1:",
                "\t0.000,0.000,0.000,0.000,0.000:"
            };

            var measures = string.Join("\n", referenceChart).Split(',');
            bool isHolding = false;
            try
            {
                foreach (string measure in measures)
                {
                    var beats = measure.Split('\n');

                    int quarterNoteBeatIndicator = beats.Length/4;
                    int noteIndex = 0;

                    foreach (var beat in beats)
                    {
                        //TODO: Refactor, this whole thing is way too long
                        if (string.IsNullOrWhiteSpace(beat) || beat.Contains("//")) continue;

                        #region -- Marquee Lights --
                        string marqueeLights = beat.Replace('M', '0');  //ignore mines

                        if (referenceChartStyle == PlayStyle.Double)
                        {
                            string convertedMarqueLights = string.Empty;
                            for (int i = 0; i < 4; i++)
                            {
                                char note = '0';

                                int p1 = i;
                                int p2 = i + 4;

                                if (marqueeLights[p1] != '0') note = marqueeLights[p1];
                                if (marqueeLights[p2] != '0') note = marqueeLights[p2];

                                convertedMarqueLights += note;
                            }

                            marqueeLights = convertedMarqueLights;
                        }
                        #endregion

                        #region -- Bass Lights --

                        bool isQuarterBeat = noteIndex % quarterNoteBeatIndicator == 0;
                        bool hasNote       = marqueeLights.Any(c => c != '0');
                        bool isHoldBegin   = marqueeLights.Any(c => c == '2');
                        bool isHoldEnd     = marqueeLights.Any(c => c == '3');
                        bool isJump        = marqueeLights.Count(c => c != '0') >= 2;

                        string bassLights = (hasNote && isQuarterBeat) || isJump ? "11" : "00";

                        if (isHoldBegin && !isHolding)
                        {
                            bassLights = "22"; //hold start
                            isHolding = true;
                        }
                        else if (isHolding)
                        {
                            bassLights = "00"; //ignore beats if there is a hold
                        }

                        if (isHoldEnd && !isHoldBegin)
                        {
                            bassLights = "33"; //hold end
                            isHolding = false;
                        }
                        #endregion

                        lightsData.Add($"{marqueeLights}{bassLights}00");

                        noteIndex++;
                    }

                    lightsData.Add(",");
                }

                lightsData.RemoveAt(lightsData.Count-1); //remove the last comma, replace with semicolon
                lightsData.Add(";");

                this.FileContent.AddRange(lightsData);
            }
            catch (Exception)
            {
                //Swallow
                //TODO: Log the exception to a file
            }
        }

        public void SaveChanges()
        {
            var backupFilePath = this._smFileInfo.FullName + ".backup";

            File.Copy(this._smFileInfo.FullName, backupFilePath, true);

            File.Delete(this._smFileInfo.FullName);

            File.WriteAllLines(this._smFileInfo.FullName, this.FileContent);
        }

        public int GetDifficultyRating(PlayStyle style, SongDifficulty difficulty)
        {
            string difficultyName = $"{difficulty}:";
            string styleName = style.ToStyleName();

            for (int i = 0; i < this.FileContent.Count; i++)
            {
                string line = this.FileContent[i];
                //find the line with the difficulty name
                if (line.Contains(difficultyName) == false) continue;

                //check the style
                string styleLine = this.FileContent[i - 2]; //the style is defined two lines above the difficulty
                if (styleLine.Contains(styleName) == false) continue;

                string difficultyLine = this.FileContent[i + 1]; //The actual difficulty rating is on the following line.

                string ratingString = difficultyLine.TrimStart(' ').TrimEnd(':', ' ');

                int rating = 0;
                double parsedRating;
                if (double.TryParse(ratingString, out parsedRating))
                {
                    rating = (int)parsedRating;
                }

                return rating;
            }

            return 0;
        }

        public Dictionary<SongDifficulty, int> GetAllDifficultyRatings(PlayStyle style)
        {
            return
                Enum.GetValues(typeof(SongDifficulty))
                    .Cast<SongDifficulty>()
                    .ToDictionary(difficulty => difficulty, difficulty => this.GetDifficultyRating(style, difficulty));
        }

        public SongDifficulty GetHighestChartedDifficulty(PlayStyle style)
        {
            return
                this.GetAllDifficultyRatings(style)
                    .OrderByDescending(d => d.Value)
                    .Select(d => d.Key)
                    .First();
        }
    }
}

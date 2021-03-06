﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LightsBuilder.Core.Data;

namespace LightsBuilder.Core.Parsers
{
    public class SmFileManager : IDisposable
    {
        
        private FileInfo SmFileInfo { get;  set; }

        private List<ChartData> StepCharts { get;  set; }

        

        public SongData SongData { get; private set; }

        public SmFileManager(string smFilePath) : this( new FileInfo(smFilePath) )
        {
            
        }

        public SmFileManager(FileInfo smFile)
        {
            this.SmFileInfo = smFile;

            if (this.SmFileInfo.Exists == false || this.SmFileInfo.Extension.ToLower() != ".sm")
            {
                throw new ArgumentException($"The given .sm file path is either invalid or a file was not found. Path: {this.SmFileInfo.FullName}");
            }

            this.StepCharts = this.ExtractChartData();
            this.SongData = this.ExtractSongData();
        }

        private SongData ExtractSongData()
        {
            var songData = new SongData();

            string bannerPath = this.GetAttribute(SmFileAttribute.BANNER);
            if (this.SmFileInfo.DirectoryName != null)
            {
                string relativeBannerPath = Path.Combine(this.SmFileInfo.DirectoryName, bannerPath);
                songData.SongBannerPath = Path.GetFullPath(relativeBannerPath);
            }

            songData.SongName = this.GetAttribute(SmFileAttribute.TITLE);
            songData.SongGroup = this.SmFileInfo.Directory?.Parent?.Name;

            return songData;
        }

        private List<ChartData> ExtractChartData()
        {
            var result = new List<ChartData>();
            var fileContent = File.ReadAllLines(this.SmFileInfo.FullName).ToList();

            for (int i = 0; i < fileContent.Count; i++)
            {
                if (!fileContent[i].Contains("#NOTES:")) continue;

                string styleLine      = fileContent[i + 1];
                string author         = fileContent[i + 2].Trim().TrimEnd(':');
                string difficultyLine = fileContent[i + 3];
                int rating            = (int)double.Parse(fileContent[i + 4].Trim().TrimEnd(':') );

                PlayStyle      style      = EnumExtensions.ToStyleEnum(styleLine);
                SongDifficulty difficulty = EnumExtensions.ToSongDifficultyEnum(difficultyLine);

                int noteDataStartIndex = i + 6;
                //Stupid Edge case
                if (fileContent[i + 5].Trim().EndsWith(":") == false)
                {
                    var nextLine = string.Concat(fileContent[i + 5].Trim().SkipWhile(c=> c != ':').Skip(1));
                    fileContent.Insert(i + 6, nextLine);
                }
                
                int noteDataEndIndex = noteDataStartIndex;

                while (fileContent[noteDataEndIndex].Contains(";") == false) noteDataEndIndex++;

                var noteData =
                    fileContent.Skip(noteDataStartIndex)
                        .Take(noteDataEndIndex - noteDataStartIndex)
                        .ToList();

                var chartData =new ChartData(style, difficulty, rating, author, noteData);

                result.Add(chartData);

                i = noteDataEndIndex;
            }

            return result;
        }

        public string GetAttribute(SmFileAttribute attribute)
        {
            if (this._isDisposed) throw new InvalidOperationException("This Object has already been disposed!");

            string attributeName = attribute.ToString();

            var fileContent = File.ReadAllLines(this.SmFileInfo.FullName);

            string attributeLine = fileContent.FirstOrDefault(line => line.Contains($"#{attributeName}:"));
            
            if (attributeLine != null)
            {
                return attributeLine
                    .Replace($"#{attributeName}:", string.Empty)
                    .TrimEnd(';');
            }

            return string.Empty;
        }

        public ChartData GetChartData(PlayStyle style, SongDifficulty difficulty)
        {
            if (this._isDisposed) throw new InvalidOperationException("This Object has already been disposed!");

            return this.StepCharts.FirstOrDefault(c => c.PlayStyle == style && c.Difficulty == difficulty);
        }

        public SongDifficulty GetHighestChartedDifficulty(PlayStyle style)
        {
            if (this._isDisposed) throw new InvalidOperationException("This Object has already been disposed!");

            return
                this.StepCharts
                    .Where(c => c.PlayStyle == style)
                    .OrderByDescending(c => c.Difficulty)
                    .Select(d => d.Difficulty)
                    .FirstOrDefault();
        }

        public void AddNewStepchart(ChartData chartData)
        {
            if(this._isDisposed) throw new InvalidOperationException("This Object has already been disposed!");

            if(chartData == null)
                throw new ArgumentNullException(nameof(chartData), "Attempted to add null chart data.");

            if (this.GetChartData(chartData.PlayStyle, chartData.Difficulty) != null)
            {
                string exMessage = $"The stepfile for {this.SongData.SongName} already contains a stepchart for {chartData.PlayStyle}-{chartData.Difficulty}";
                throw new InvalidOperationException(exMessage);
            }

            this.StepCharts.Add(chartData);

            var backupFilePath = this.SmFileInfo.FullName + ".backup";
            File.Copy(this.SmFileInfo.FullName, backupFilePath, true);

            File.AppendAllLines(this.SmFileInfo.FullName, chartData.GetRawChartData());
        }
        
        public static ChartData GenerateLightsChart(ChartData referenceChart)
        {
            if (referenceChart == null)
                throw new ArgumentNullException(nameof(referenceChart), "referenceChart cannot be null.");


            var lightChart = new ChartData(PlayStyle.Lights, SongDifficulty.Easy, difficultyRating: 1, chartAuthor: "SMLightsBuilder");

            bool isHolding = false;
            foreach (var referenceMeasure in referenceChart.Measures)
            {
                int quarterNoteBeatIndicator = referenceMeasure.Notes.Count / 4;
                int noteIndex = 0;
                
                var newMeasure = new MeasureData();

                foreach (var note in referenceMeasure.Notes)
                {
                    string marqueeLights = note.StepData.Replace('M', '0'); //ignore mines

                    if (referenceChart.PlayStyle == PlayStyle.Double)
                    {
                        marqueeLights = MapMarqueeLightsForDoubles(marqueeLights);
                    }

                    bool isQuarterBeat = noteIndex%quarterNoteBeatIndicator == 0;
                    bool hasNote = marqueeLights.Any(c => c != '0');
                    bool isHoldBegin = marqueeLights.Any(c => c == '2' || c == '4');
                    bool isHoldEnd = marqueeLights.Any(c => c == '3');
                    bool isJump = marqueeLights.Count(c => c != '0') >= 2;

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

                    var noteData = new NoteData($"{marqueeLights}{bassLights}00");

                    newMeasure.Notes.Add(noteData);
                    noteIndex++;
                }

                lightChart.Measures.Add(newMeasure);
            }

            return lightChart;
        }

        private static string MapMarqueeLightsForDoubles(string marqueeLights)
        {
            string convertedMarqueeLights = string.Empty;
            for (int i = 0; i < 4; i++)
            {
                char note = '0';

                int p1 = i;
                int p2 = i + 4;

                if (marqueeLights[p1] != '0') note = marqueeLights[p1];
                if (marqueeLights[p2] != '0') note = marqueeLights[p2];

                convertedMarqueeLights += note;
            }

            return convertedMarqueeLights;
        }

        private bool _isDisposed;

        public void Dispose()
        {
            this.SmFileInfo = null;
            this.StepCharts = null;
            this.SongData = null;
            this._isDisposed = true;
        }
    }
}

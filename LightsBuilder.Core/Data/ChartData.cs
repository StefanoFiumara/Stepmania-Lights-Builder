using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LightsBuilder.Core.Data
{
    [DebuggerDisplay("{PlayStyle} - {Difficulty} - {DifficultyRating}")]
    abstract public class ChartData
    {
        public ChartFormat ChartFileFormat { get; set; }
        public PlayStyle PlayStyle { get; set; }
        public SongDifficulty Difficulty { get; set; }
        public int DifficultyRating { get; set; }
        public string ChartAuthor { get; set; }

        public List<MeasureData> Measures { get; set; }
        
        public ChartData(PlayStyle playStyle, SongDifficulty difficulty, int difficultyRating, string chartAuthor)
        {
            this.PlayStyle = playStyle;
            this.Difficulty = difficulty;
            this.DifficultyRating = difficultyRating;
            this.ChartAuthor = chartAuthor;

            this.Measures = new List<MeasureData>();
        }

        public ChartData(PlayStyle playStyle, SongDifficulty difficulty, int difficultyRating, string chartAuthor, List<string> rawChartData) 
            : this(playStyle, difficulty, difficultyRating, chartAuthor)
        {
            this.Measures = this.ParseRawChartData(rawChartData);
        }

        protected List<MeasureData> ParseRawChartData(List<string> rawChartData)
        {
            var rawMeasures = string.Join("\n", rawChartData).Split(',');

            return rawMeasures.Select( data => new MeasureData(data) ).ToList();
        }

        abstract public List<string> GetHeader();

        public List<string> GetRawChartData()
        {
            var rawData = GetHeader();

            foreach (var measureData in this.Measures)
            {
                foreach (var noteData in measureData.Notes)
                {
                    rawData.Add(noteData.StepData);
                }
                rawData.Add(",");
            }

            rawData.RemoveAt(rawData.Count - 1); //remove the last comma, replace with semicolon
            rawData.Add(";");

            return rawData;
        }
    }
}
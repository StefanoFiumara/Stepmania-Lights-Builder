using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LightsBuilder.Core.Data
{
    [DebuggerDisplay("{PlayStyle} - {Difficulty} - {DifficultyRating}")]
    public class SscChartData : ChartData
    {
        public string NoteData { get; set; }
        public string ChartName { get; set; }
        public string Description { get; set; }
        public string ChartStyle { get; set; }

		public SscChartData(PlayStyle playStyle, SongDifficulty difficulty, int difficultyRating, string chartAuthor)
            : base(playStyle, difficulty, difficultyRating, chartAuthor)
		{
            this.ChartFileFormat = ChartFormat.ssc;
		}

		public SscChartData(PlayStyle playStyle, SongDifficulty difficulty, int difficultyRating, string chartAuthor, List<string> rawChartData) 
            : base(playStyle, difficulty, difficultyRating, chartAuthor, rawChartData)
        {
            this.ChartFileFormat = ChartFormat.ssc;
		}

		public override List<string> GetHeader()
		{
			return new List<string>
			{
				$"//---------------{this.PlayStyle.ToStyleName()}-----------------",
                $"#NOTEDATA:{this.NoteData};",
                $"#CHARTNAME:{this.ChartName};",
                $"#STEPSTYPE:{this.PlayStyle.ToStyleName()};",
                $"#DESCRIPTION:{this.Description};",
                $"#CHARTSTYLE:{this.ChartStyle};",
                $"#DIFFICULTY:{this.Difficulty};",
                $"#METER:{this.DifficultyRating};",
                $"#RADARVALUES:0.000,0.000,0.000,0.000,0.000:;",
                $"#CREDIT:{this.ChartAuthor};",
				$"#NOTES:"
			};
		}
    }
}
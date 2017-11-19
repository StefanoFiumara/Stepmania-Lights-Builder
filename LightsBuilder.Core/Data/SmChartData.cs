using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LightsBuilder.Core.Data
{
    [DebuggerDisplay("{PlayStyle} - {Difficulty} - {DifficultyRating}")]
    public class SmChartData : ChartData
    {
		public SmChartData(PlayStyle playStyle, SongDifficulty difficulty, int difficultyRating, string chartAuthor)
            : base(playStyle, difficulty, difficultyRating, chartAuthor)
		{
            this.ChartFileFormat = ChartFormat.sm;
		}

		public SmChartData(PlayStyle playStyle, SongDifficulty difficulty, int difficultyRating, string chartAuthor, List<string> rawChartData) 
            : base(playStyle, difficulty, difficultyRating, chartAuthor, rawChartData)
        {
			this.ChartFileFormat = ChartFormat.sm;
		}

		public override List<string> GetHeader()
		{
			return new List<string>
			{
				$"//---------------{this.PlayStyle.ToStyleName()}-----------------",
				$"#NOTES:",
				$"    {this.PlayStyle.ToStyleName()}:",
				$"    {this.ChartAuthor}:",
				$"    {this.Difficulty}:",
				$"    {this.DifficultyRating}:",
				$"    0.000,0.000,0.000,0.000,0.000:"
			};
		}
    }
}
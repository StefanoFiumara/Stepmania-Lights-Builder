using System.Collections.Generic;
using System.Linq;

namespace LightsBuilder.Core.Data
{
    public class MeasureData
    {
        public List<NoteData> Notes { get; set; }

        public MeasureData()
        {
            this.Notes = new List<NoteData>();
        }
        public MeasureData(string measureData)
        {
            this.Notes = this.ParseRawMeasureData(measureData);
        }

        private List<NoteData> ParseRawMeasureData(string measureData)
        {
            return measureData.Split('\n')
                .Select(data => data.Trim())
                .Where(data => !data.Contains(@"//"))
                .Where(data => string.IsNullOrWhiteSpace(data) == false)
                .Select( data => new NoteData(data) )
                .ToList();
        }
    }
}
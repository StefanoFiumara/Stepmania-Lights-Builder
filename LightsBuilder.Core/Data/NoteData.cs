using System.Diagnostics;

namespace LightsBuilder.Core.Data
{
    [DebuggerDisplay("{StepData}")]
    public class NoteData
    {
        public string StepData { get; }

        public NoteData(string stepData)
        {
            this.StepData = stepData;
        }
    }
}
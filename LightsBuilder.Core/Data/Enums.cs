// ReSharper disable InconsistentNaming

using System;
using System.Linq;

namespace LightsBuilder.Core.Data
{
    public enum SmFileAttribute
    {
        TITLE,
        SUBTITLE,
        ARTIST,
        TITLETRANSLIT,
        SUBTITLETRANSLIT,
        ARTISTTRANSLIT,
        GENRE,
        CREDIT,
        BANNER,
        BACKGROUND,
        LYRICSPATH,
        CDTITLE,
        MUSIC,
        OFFSET,
        SAMPLESTART,
        SAMPLELENGTH,
        SELECTABLE,
        BPMS,
        STOPS,
        TIMESIGNATURES,
        BGCHANGES,
        KEYSOUNDS,
    }

    public enum SongDifficulty
    {
        Undefined = 0,
        Beginner,
        Easy,
        Medium,
        Hard,
        Challenge
    }

    public enum PlayStyle
    {
        Undefined = 0,
        Single,
        Double,
        Couple,
        Solo,
        Lights
    }

    public enum ChartFormat
    {
        Undefined = 0,
        sm,
        ssc
    }

    public static class EnumExtensions
    {
        public static string ToStyleName(this PlayStyle style)
        {
            switch (style)
            {
                case PlayStyle.Single:
                    return "dance-single";
                case PlayStyle.Double:
                    return "dance-double";
                case PlayStyle.Couple:
                    return "dance-couple";
                case PlayStyle.Solo:
                    return "dance-solo";
                case PlayStyle.Lights:
                    return "lights-cabinet";
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(style), style, null);
            }
        }

        public static PlayStyle ToStyleEnum(string styleName)
        {
            styleName = styleName.Trim().TrimEnd(':');
            switch (styleName)
            {
                case "dance-single":
                    return PlayStyle.Single;
                case "dance-double":
                    return PlayStyle.Double;
                case "dance-couple":
                    return PlayStyle.Couple;
                case "dance-solo":
                    return PlayStyle.Solo;
                case "lights-cabinet":
                    return PlayStyle.Lights;
                default:
                    return PlayStyle.Undefined;
            }
        }

        public static SongDifficulty ToSongDifficultyEnum(string difficultyName)
        {
            return
                Enum.GetValues(typeof(SongDifficulty))
                    .OfType<SongDifficulty>()
                    .SingleOrDefault(d => difficultyName.Contains(d.ToString()));
        }
    }
}

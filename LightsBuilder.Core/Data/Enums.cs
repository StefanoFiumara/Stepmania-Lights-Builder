// ReSharper disable InconsistentNaming

using System;

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
        KEYSOUNDS
    }

    public enum SongDifficulty
    {
        Beginner,
        Easy,
        Medium,
        Hard,
        Challenge
    }

    public enum PlayStyle
    {
        Single,
        Double,
        Lights
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
                case PlayStyle.Lights:
                    return "lights-cabinet";
                default:
                    throw new ArgumentOutOfRangeException(nameof(style), style, null);
            }
        }
    }
}

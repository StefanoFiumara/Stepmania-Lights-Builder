﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

namespace LightsBuilder.Core.Data
{
    [DataContract]
    public class SongData
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public string SongName { get; set; }

        [DataMember]
        public string SongGroup { get; set; }

        [DataMember]
        public string SongBannerPath { get; set; }

        [DataMember]
        public Dictionary<SongDifficulty, int> DifficultySingles { get; set; }

        [DataMember]
        public Dictionary<SongDifficulty, int> DifficultyDoubles { get; set; }

        public bool IsValid()
        {
            return string.IsNullOrEmpty(this.SongName) == false &&
                   string.IsNullOrEmpty(this.SongGroup) == false &&
                   string.IsNullOrEmpty(this.SongBannerPath) == false &&
                   this.DifficultySingles.Keys.Count == 5 &&
                   this.DifficultyDoubles.Keys.Count == 5;
        }

        public bool IsEqualTo(SongData other)
        {
            bool hasSameNameAndGroup = this.SongName == other.SongName && this.SongGroup == other.SongGroup;

            return hasSameNameAndGroup || this.Id == other.Id;
        }

        public override string ToString()
        {
            return $"{this.SongGroup}-{Path.GetFileNameWithoutExtension(this.SongBannerPath)}";
        }
    }

    [CollectionDataContract, KnownType(typeof(SongData))]
    public class SongDataCollection : List<SongData>
    {
    }
}

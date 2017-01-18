using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using LightsBuilder.Core.Data;

namespace LightsBuilder.Core.Repositories
{
    public class SongDataRepository : IRepository<SongData>
    {
        private FileInfo XmlFile { get; set; }
        private readonly DataContractSerializer _serializer;

        public SongDataRepository(string xmlFileName) : this(new FileInfo(xmlFileName))
        {
            
        }

        public SongDataRepository(FileInfo xmlFile)
        {
            this.XmlFile = xmlFile;
            this._serializer = new DataContractSerializer(typeof(SongDataCollection));

            if (this.XmlFile.Exists == false)
            {
                var newCollection = new SongDataCollection();
                using (var stream = new FileStream(this.XmlFile.FullName, FileMode.Create))
                {
                    this._serializer.WriteObject(stream, newCollection);
                }
            }
            else
            {
                //try to parse the file, in case it's corrupt
                try
                {
                    this.GetAll();
                }
                catch (Exception)
                {
                    //Create a new file to start from scratch, probably log an error
                    var newCollection = new SongDataCollection();
                    using (var stream = new FileStream(this.XmlFile.FullName, FileMode.Create))
                    {
                        this._serializer.WriteObject(stream, newCollection);
                    }
                }
            }
        }

        private void WriteData(IEnumerable<SongData> data)
        {
            File.WriteAllText(this.XmlFile.FullName, string.Empty);

            var collection = new SongDataCollection();
            collection.AddRange(data);

            using (var stream = new FileStream(this.XmlFile.FullName, FileMode.Open))
            {
                this._serializer.WriteObject(stream, collection);
            }
        }

        public List<SongData> GetAll()
        {
            SongDataCollection result;

            using (var stream = new FileStream(this.XmlFile.FullName, FileMode.Open))
            {
                result = (SongDataCollection)this._serializer.ReadObject(stream);
            }

            return result;
        }

        public void Add(SongData entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var allSongData = this.GetAll().ToList();

            if (allSongData.Any(s => s.IsEqualTo(entity)))
            {
                return; //Song is already in repository
            }

            entity.Id = Guid.NewGuid();
            allSongData.Add(entity);

            this.WriteData(allSongData);
        }

        public void AddAll(IEnumerable<SongData> entities)
        {
            var allSongData = this.GetAll().ToList();

            var dataToAdd = new List<SongData>();

            foreach (var entity in entities)
            {
                if (allSongData.Any(s => s.IsEqualTo(entity)))
                {
                    continue;
                }
                entity.Id = Guid.NewGuid();
                dataToAdd.Add(entity);
            }
            allSongData.AddRange(dataToAdd);

            this.WriteData(allSongData);
        }

        public void Remove(SongData entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var allSongs = this.GetAll().ToList();

            var songToRemove = allSongs.SingleOrDefault(song => song.Id == entity.Id);
            if (songToRemove != null)
            {
                allSongs.Remove(songToRemove);
            }

            this.WriteData(allSongs);

        }

        public void Update(SongData entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var allSongs = this.GetAll().ToList();

            var songData = allSongs.SingleOrDefault(song => song.Id == entity.Id);

            if (songData == null)
            {
                throw new ApplicationException($"Could not find song data with ID {entity.Id} in repository");
            }

            //Cheesy
            allSongs.Remove(songData);
            allSongs.Add(entity);

            this.WriteData(allSongs);
        }

        public void AddOrUpdate(SongData entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            if (entity.Id == Guid.Empty) this.Add(entity);
            else this.Update(entity);
        }
    }
}
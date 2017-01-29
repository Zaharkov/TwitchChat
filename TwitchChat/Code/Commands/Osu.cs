using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Configuration;
using Configuration.Entities;

namespace TwitchChat.Code.Commands
{
    public static class OsuCommand
    {
        private static readonly Osu Osu = ConfigHolder.Configs.Osu;

        public static SendMessage GetMusic()
        {
            var ps = Process.GetProcessesByName("osu!");

            if (ps.Length <= 0)
                return SendMessage.GetMessage(Osu.Off);

            var osu = ps[0];
            var title = osu.MainWindowTitle;

            if(title.IndexOf('-') < 0)
                return SendMessage.GetMessage(Osu.Selecting);

            var song = title.Substring(title.IndexOf('-') + 2);

            Beatmap beatmap = null;
            if (!string.IsNullOrEmpty(Osu.PathToOsu))
            { 
                var osuList = new OsuDatabaseLoader().LoadDatabase(Osu.PathToOsu);
                beatmap = osuList.FirstOrDefault(t => $"{t.ArtistRoman} - {t.TitleRoman} [{t.DiffName}]".Equals(song, StringComparison.InvariantCultureIgnoreCase));
            }

            return SendMessage.GetMessage(beatmap == null ? 
                string.Format(Osu.Played, song) :
                string.Format(Osu.PlayedWithMap, song, beatmap.DlLink));
        }
    }

    public class Beatmap
    {
        public bool Collection { get; set; }
        private string _titleUnicode;

        public string TitleUnicode
        {
            get
            {
                return _titleUnicode == string.Empty ? TitleRoman : _titleUnicode;
            }
            set { _titleUnicode = value; }
        }

        private string _artistUnicode;
        public string ArtistUnicode
        {
            get { return _artistUnicode == string.Empty ? ArtistRoman : _artistUnicode; }
            set { _artistUnicode = value; }
        }
        public string TitleRoman { get; set; }
        public string ArtistRoman { get; set; }
        public string Creator { get; set; }
        public string DiffName { get; set; }
        public string Mp3Name { get; set; }
        public string Md5 { get; set; }
        public string OsuFileName { get; set; }
        public string DlLink
        {
            get
            {
                if (MapId == 0 || MapSetId == 0)
                    return StateStr;

                if (MapId != 0)
                    return @"http://osu.ppy.sh/b/" + MapId;
                return @"http://osu.ppy.sh/s/" + MapSetId;
            }
        }

        public Dictionary<int, double> ModPpStars;
        public double MaxBpm { get; set; }
        public double MinBpm { get; set; }


        public string Tags { get; set; }
        public string StateStr;
        private byte _state;
        public byte State
        {
            get { return _state; }
            set
            {
                string val;
                _state = value;
                switch (value)
                {
                    case 0: val = "Not updated"; break;
                    case 1: val = "Unsubmitted"; break;
                    case 2: val = "Pending"; break;
                    case 3: val = "??"; break;
                    case 4: val = "Ranked"; break;
                    case 5: val = "Approved"; break;
                    default: val = "??" + value.ToString(); break;
                }
                StateStr = val;
            }
        }
        public short Circles { get; set; }
        public short Sliders { get; set; }
        public short Spinners { get; set; }
        public DateTime? EditDate { get; set; }
        public float ApproachRate { get; set; }
        public float CircleSize { get; set; }
        public float HpDrainRate { get; set; }
        public float OverallDifficulty { get; set; }
        public double SliderVelocity { get; set; }
        public int DrainingTime { get; set; }
        public int TotalTime { get; set; }
        public int PreviewTime { get; set; }
        public int MapId { get; set; }
        public int MapSetId { get; set; }
        public int ThreadId { get; set; }
        public int MapRating { get; set; }
        public short Offset { get; set; }
        public float StackLeniency { get; set; }
        public byte Mode { get; set; }
        public string Source { get; set; }
        public short AudioOffset { get; set; }
        public string LetterBox { get; set; }
        public bool Played { get; set; }
        public DateTime? LastPlayed { get; set; }
        public bool IsOsz2 { get; set; }
        public string Dir { get; set; }
        public DateTime? LastSync { get; set; }
        public bool DisableHitsounds { get; set; }
        public bool DisableSkin { get; set; }
        public bool DisableSb { get; set; }
        public short BgDim { get; set; }
        public int Somestuff { get; set; }
        public string VideoDir { get; set; }
        public void Dispose()
        {

        }

        public Beatmap()
        {
            InitEmptyValues();
        }
        public Beatmap(Beatmap b)
        {
            Collection = b.Collection;
            TitleUnicode = b.TitleUnicode;
            TitleRoman = b.TitleRoman;
            ArtistUnicode = b.ArtistUnicode;
            ArtistRoman = b.ArtistRoman;
            Creator = b.Creator;
            DiffName = b.DiffName;
            Mp3Name = b.Mp3Name;
            Md5 = b.Md5;
            OsuFileName = b.OsuFileName;
            Tags = b.Tags;
            Somestuff = b.Somestuff;
            State = b.State;
            Circles = b.Circles;
            Sliders = b.Sliders;
            Spinners = b.Spinners;
            EditDate = b.EditDate;
            ApproachRate = b.ApproachRate;
            CircleSize = b.CircleSize;
            HpDrainRate = b.HpDrainRate;
            OverallDifficulty = b.OverallDifficulty;
            SliderVelocity = b.SliderVelocity;
            DrainingTime = b.DrainingTime;
            TotalTime = b.TotalTime;
            PreviewTime = b.PreviewTime;
            MapId = b.MapId;
            MapSetId = b.MapSetId;
            ThreadId = b.ThreadId;
            MapRating = b.MapRating;
            Offset = b.Offset;
            StackLeniency = b.StackLeniency;
            Mode = b.Mode;
            Source = b.Source;
            AudioOffset = b.AudioOffset;
            LetterBox = b.LetterBox;
            Played = b.Played;
            LastPlayed = b.LastPlayed;
            IsOsz2 = b.IsOsz2;
            Dir = b.Dir;
            LastSync = b.LastSync;
            DisableHitsounds = b.DisableHitsounds;
            DisableSkin = b.DisableSkin;
            DisableSb = b.DisableSb;
            BgDim = b.BgDim;
            ModPpStars = b.ModPpStars;
            MaxBpm = b.MaxBpm;
            MinBpm = b.MinBpm;
        }
        public Beatmap(string artist)
        {
            InitEmptyValues();
            ArtistUnicode = artist;
        }
        public Beatmap(string md5, bool collection)
        {
            InitEmptyValues();
            Md5 = md5;
            Collection = collection;
        }
        public void InitEmptyValues()
        {
            ModPpStars = new Dictionary<int, double>();
            Collection = false;
            TitleUnicode = string.Empty;
            TitleRoman = string.Empty;
            ArtistUnicode = string.Empty;
            ArtistRoman = string.Empty;
            Creator = string.Empty;
            DiffName = string.Empty;
            Mp3Name = string.Empty;
            Md5 = string.Empty;
            OsuFileName = string.Empty;
            Tags = string.Empty;
            Somestuff = 0;
            State = 0;
            Circles = 0;
            Sliders = 0;
            Spinners = 0;
            EditDate = null;
            ApproachRate = 0;
            CircleSize = 0;
            HpDrainRate = 0;
            OverallDifficulty = 0;
            SliderVelocity = 0;
            DrainingTime = 0;
            TotalTime = 0;
            PreviewTime = 0;
            MapId = 0;
            MapSetId = 0;
            ThreadId = 0;
            MapRating = 0;
            Offset = 0;
            StackLeniency = 0;
            Mode = 0;
            Source = string.Empty;
            AudioOffset = 0;
            LetterBox = string.Empty;
            Played = false;
            LastPlayed = null;
            IsOsz2 = false;
            Dir = string.Empty;
            LastSync = null;
            DisableHitsounds = false;
            DisableSkin = false;
            DisableSb = false;
            BgDim = 0;
            MinBpm = 0.0f;
            MaxBpm = 0.0f;
        }
    }

    public class OsuDatabaseLoader
    {
        private FileStream _fileStream;
        private BinaryReader _binaryReader;
        private readonly Beatmap _tempBeatmap = new Beatmap();
        public int MapsWithNoId { get; private set; }
        public string Username { get; private set; }
        public int FileDate { get; private set; }
        public int ExpectedNumberOfMapSets { get; private set; }
        public int ExpectedNumOfBeatmaps { get; private set; } = -1;
        private bool _stopProcessing;
        private int _numberOfLoadedBeatmaps;

        private readonly List<Beatmap> _listBeatmaps = new List<Beatmap>();

        public List<Beatmap> LoadDatabase(string fullOsuDbPath)
        {
            if (FileExists(fullOsuDbPath))
            {
                _fileStream = new FileStream(fullOsuDbPath, FileMode.Open, FileAccess.Read);
                _binaryReader = new BinaryReader(_fileStream);
                if (DatabaseContainsData())
                {
                    ReadDatabaseEntries();
                }
                DestoryReader();
            }
            else
            {
                throw new FileNotFoundException();
            }

            return _listBeatmaps;
        }

        private void ReadDatabaseEntries()
        {
            for (_numberOfLoadedBeatmaps = 0; _numberOfLoadedBeatmaps < ExpectedNumOfBeatmaps; _numberOfLoadedBeatmaps++)
            {
                if (_stopProcessing)
                {
                    return;
                }
                ReadNextBeatmap();
            }
        }
        private void ReadNextBeatmap()
        {
            _tempBeatmap.InitEmptyValues();
            try
            {
                ReadMapHeader(_tempBeatmap);
                ReadMapInfo(_tempBeatmap);
                ReadTimingPoints(_tempBeatmap);
                ReadMapMetaData(_tempBeatmap);

                _listBeatmaps.Add(new Beatmap(_tempBeatmap));
            }
            catch (Exception)
            {
                _stopProcessing = true;
            }
        }

        private void ReadMapMetaData(Beatmap beatmap)
        {
            beatmap.MapId = Math.Abs(_binaryReader.ReadInt32());
            if (beatmap.MapId == 0) MapsWithNoId++;

            beatmap.MapSetId = Math.Abs(_binaryReader.ReadInt32());
            beatmap.ThreadId = Math.Abs(_binaryReader.ReadInt32());
            beatmap.MapRating = _binaryReader.ReadInt32();
            beatmap.Offset = _binaryReader.ReadInt16();
            beatmap.StackLeniency = _binaryReader.ReadSingle();
            beatmap.Mode = _binaryReader.ReadByte();
            beatmap.Source = ReadString();
            beatmap.Tags = ReadString();
            beatmap.AudioOffset = _binaryReader.ReadInt16();
            beatmap.LetterBox = ReadString();
            beatmap.Played = !_binaryReader.ReadBoolean();
            beatmap.LastPlayed = GetDate();
            beatmap.IsOsz2 = _binaryReader.ReadBoolean();
            beatmap.Dir = ReadString();
            beatmap.LastSync = GetDate();
            beatmap.DisableHitsounds = _binaryReader.ReadBoolean();
            beatmap.DisableSkin = _binaryReader.ReadBoolean();
            beatmap.DisableSb = _binaryReader.ReadBoolean();
            _binaryReader.ReadBoolean();
            beatmap.BgDim = _binaryReader.ReadInt16();
            //bytes not analysed.
            _binaryReader.BaseStream.Seek(FileDate <= 20160403 ? 4 : 8, SeekOrigin.Current);
        }
        private void ReadTimingPoints(Beatmap beatmap)
        {
            var amountOfTimingPoints = _binaryReader.ReadInt32();
            double minBpm = double.MaxValue,
            maxBpm = double.MinValue;
            for (var i = 0; i < amountOfTimingPoints; i++)
            {
                var bpmDelay = _binaryReader.ReadDouble();
                _binaryReader.ReadDouble();
                var inheritsBpm = _binaryReader.ReadBoolean();
                if (inheritsBpm)
                {
                    if (60000 / bpmDelay < minBpm)
                        minBpm = 60000 / bpmDelay;
                    if (60000 / bpmDelay > maxBpm)
                        maxBpm = 60000 / bpmDelay;
                }
            }
            beatmap.MaxBpm = maxBpm;
            beatmap.MinBpm = minBpm;
        }
        private void ReadMapInfo(Beatmap beatmap)
        {
            beatmap.State = _binaryReader.ReadByte();
            beatmap.Circles = _binaryReader.ReadInt16();
            beatmap.Sliders = _binaryReader.ReadInt16();
            beatmap.Spinners = _binaryReader.ReadInt16();
            beatmap.EditDate = GetDate();
            beatmap.ApproachRate = _binaryReader.ReadSingle();
            beatmap.CircleSize = _binaryReader.ReadSingle();
            beatmap.HpDrainRate = _binaryReader.ReadSingle();
            beatmap.OverallDifficulty = _binaryReader.ReadSingle();
            beatmap.SliderVelocity = _binaryReader.ReadDouble();

            for (int j = 0; j < 4; j++)
            {
                ReadStarsData(beatmap);
            }
            beatmap.DrainingTime = _binaryReader.ReadInt32();
            beatmap.TotalTime = _binaryReader.ReadInt32();
            beatmap.PreviewTime = _binaryReader.ReadInt32();
        }

        private void ReadStarsData(Beatmap beatmap)
        {
            var num = _binaryReader.ReadInt32();
            if (num < 0)
            {
                return;
            }

            for (var j = 0; j < num; j++)
            {
                var modEnum = (int)ConditionalRead();
                var stars = (double)ConditionalRead();
                if (!beatmap.ModPpStars.ContainsKey(modEnum))
                {
                    beatmap.ModPpStars.Add(modEnum, Math.Round(stars, 2));
                }
                else
                {
                    beatmap.ModPpStars[modEnum] = Math.Round(stars, 2);
                }

            }

        }
        private object ConditionalRead()
        {
            switch (_binaryReader.ReadByte())
            {
                case 1:
                    {
                        return _binaryReader.ReadBoolean();
                    }
                case 2:
                    {
                        return _binaryReader.ReadByte();
                    }
                case 3:
                    {
                        return _binaryReader.ReadUInt16();
                    }
                case 4:
                    {
                        return _binaryReader.ReadUInt32();
                    }
                case 5:
                    {
                        return _binaryReader.ReadUInt64();
                    }
                case 6:
                    {
                        return _binaryReader.ReadSByte();
                    }
                case 7:
                    {
                        return _binaryReader.ReadInt16();
                    }
                case 8:
                    {
                        return _binaryReader.ReadInt32();
                    }
                case 9:
                    {
                        return _binaryReader.ReadInt64();
                    }
                case 10:
                    {
                        return _binaryReader.ReadChar();
                    }
                case 11:
                    {
                        return _binaryReader.ReadString();
                    }
                case 12:
                    {
                        return _binaryReader.ReadSingle();
                    }
                case 13:
                    {
                        return _binaryReader.ReadDouble();
                    }
                case 14:
                    {
                        return _binaryReader.ReadDecimal();
                    }
                case 15:
                    {
                        return GetDate();
                    }
                case 16:
                    {
                        int num = _binaryReader.ReadInt32();
                        if (num > 0)
                        {
                            return _binaryReader.ReadBytes(num);
                        }
                        if (num < 0)
                        {
                            return null;
                        }
                        return new byte[0];

                    }
                case 17:
                    {
                        int num = _binaryReader.ReadInt32();
                        if (num > 0)
                        {
                            return _binaryReader.ReadChars(num);
                        }
                        if (num < 0)
                        {
                            return null;
                        }
                        return new char[0];
                    }
                case 18:
                    {
                        throw new NotImplementedException();
                    }
                default:
                    {
                        return null;
                    }
            }
        }
        private DateTime GetDate()
        {
            var ticks = _binaryReader.ReadInt64();
            if (ticks < 0L)
            {
                return new DateTime();
            }
            try
            {
                return new DateTime(ticks, DateTimeKind.Utc);
            }
            catch (Exception)
            {
                _stopProcessing = true;
                return new DateTime();
            }
        }
        private void ReadMapHeader(Beatmap beatmap)
        {
            beatmap.ArtistRoman = ReadString().Trim();
            beatmap.ArtistUnicode = ReadString().Trim();
            beatmap.TitleRoman = ReadString().Trim();
            beatmap.TitleUnicode = ReadString().Trim();
            beatmap.Creator = ReadString().Trim();
            beatmap.DiffName = ReadString().Trim();
            beatmap.Mp3Name = ReadString().Trim();
            beatmap.Md5 = ReadString().Trim();
            beatmap.OsuFileName = ReadString().Trim();
        }
        private string ReadString()
        {
            try
            {
                if (_binaryReader.ReadByte() == 11)
                {
                    return _binaryReader.ReadString();
                }
                return "";
            }
            catch { _stopProcessing = true; return ""; }
        }
        private bool DatabaseContainsData()
        {
            FileDate = _binaryReader.ReadInt32();
            ExpectedNumberOfMapSets = _binaryReader.ReadInt32();
            try
            {
                _binaryReader.ReadBoolean();
                GetDate();
                _binaryReader.BaseStream.Seek(1, SeekOrigin.Current);
                Username = _binaryReader.ReadString();
                ExpectedNumOfBeatmaps = _binaryReader.ReadInt32();
                if (FileDate > 20160403)
                    _binaryReader.BaseStream.Seek(4, SeekOrigin.Current);

                if (ExpectedNumOfBeatmaps < 0)
                {
                    return false;
                }
            }
            catch { return false; }
            return true;
        }
        private void DestoryReader()
        {
            _fileStream.Close();
            _binaryReader.Close();
            _fileStream.Dispose();
            _binaryReader.Dispose();
        }
        private bool FileExists(string fullPath)
        {
            return !string.IsNullOrEmpty(fullPath) && File.Exists(fullPath);
        }
    }
}

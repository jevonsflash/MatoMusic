using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MatoMusic.Infrastructure.Common;
using Microsoft.Maui.Controls;

namespace MatoMusic.Core
{
    public class MusicInfo : ObservableObject, IBasicInfo, IClueObject
    {
        public IMusicInfoManager MusicInfoManager => DependencyService.Get<IMusicInfoManager>();


        public MusicInfo()
        {
            PropertyChanged += MusicInfo_PropertyChanged;
        }

        private void MusicInfo_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IsFavourite))
            {
                if (IsFavourite)
                {
                    MusicInfoManager.CreatePlaylistEntryToMyFavourite(this);
                }
                else
                {
                    MusicInfoManager.DeletePlaylistEntryFromMyFavourite(this);
                }
            }
        }

        public long Id { get; set; }
        public string OnlineId { get; set; }
        public string Title
        {
            get;
            set;
        }
        public string Url
        {
            get;
            set;
        }
        public string AlbumTitle
        {
            get;
            set;
        }
        public string Artist
        {
            get;
            set;
        }

        private bool _isFavourite;
        public bool IsFavourite
        {
            get { return _isFavourite; }

            set
            {
                if (value != _isFavourite)
                {
                    _isFavourite = value;
                    if (IsInitFinished)
                    {
                        RaisePropertyChanged();

                    }
                }
            }
        }

        private bool _isPlaying;

        public bool IsPlaying
        {
            get { return _isPlaying; }
            set
            {
                if (value != _isPlaying)
                {
                    _isPlaying = value;

                    RaisePropertyChanged();


                };
            }
        }

        public string GroupHeader { get; set; }
        public ImageSource AlbumArt { get; set; }
        public string AlbumArtPath { get; set; }
        public ulong Duration { get; set; }
        public string Genre { get; set; }

        public List<string> ClueStrings
        {
            get
            {
                var result = new List<string>();


                result.Add(Title);
                result.Add(Artist);
                result.Add(AlbumTitle);
                return result;
            }
        }

        public string GC { get; set; }

        public bool IsInitFinished;

        /// <summary>
        /// 提供搜索线索
        /// </summary>
        /// <returns>搜索线索字符串</returns>
        public override string ToString()
        {
            return string.Format("{0} {1} {2}", Title, Artist, AlbumTitle);
        }


        public void SetFavourite(bool isF, bool isAffectPlaylist = true)
        {
            if (isAffectPlaylist)
            {
                IsFavourite = isF;
            }
            else
            {
                _isFavourite = isF;
            }
        }
    }
}

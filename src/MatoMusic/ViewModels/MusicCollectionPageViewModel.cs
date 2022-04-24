using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MatoMusic.Core;
using MatoMusic.Core.Models;
using MatoMusic.Core.ViewModel;
//using OnlineMusic.Server;
using MatoMusic.View;


namespace MatoMusic.ViewModel
{
    public class MusicCollectionPageViewModel : MusicRelatedViewModel
    {
        //private OnlineMusicManager _onlineMusicManager;

        public MusicCollectionPageViewModel(MusicCollectionInfo musicsCollectionInfo)
        {
            Init(musicsCollectionInfo);
        }

        public MusicCollectionPageViewModel(MusicCollectionInfo musicsCollectionInfo, List<MenuCellInfo> menus)
        {
            Init(musicsCollectionInfo, menus);
        }

        private async void Init(MusicCollectionInfo musicsCollectionInfo, List<MenuCellInfo> menus = null)
        {
            if (musicsCollectionInfo is BillboardInfo && 
                (musicsCollectionInfo.Musics == null ||
                musicsCollectionInfo.Musics.Count == 0))
            {
                //_onlineMusicManager = new OnlineMusicManager();
                //var onlineMusicId = (musicsCollectionInfo as BillboardInfo).OnlineId;
                //var musicList = await _onlineMusicManager.GetToplistAsync(int.Parse(onlineMusicId));
                //musicsCollectionInfo.Musics = new ObservableCollection<MusicInfo>(musicList);
            }

            this.MusicsCollectionInfo = musicsCollectionInfo;

            this.MusicCollectionArtPath = musicsCollectionInfo?.AlbumArtPath;

            if (menus != null)
            {
                Menus = menus;

            }
            else
            {


                Menus = new List<MenuCellInfo>()
                {

                    new MenuCellInfo() {Title = L("Play"), Code = "Play", Icon = "cdplay"},
                    new MenuCellInfo() {Title = L("AddToQueue2"), Code = "AddMusicCollectionToQueue", Icon = "addtostack"},
                    new MenuCellInfo() {Title = L("AddTo"), Code = "AddMusicCollectionToPlaylist", Icon = "addto"},
                    new MenuCellInfo() {Title = L("AddToFavourite"), Code = "AddToFavourite", Icon = "favouriteadd"}

                };
            }
        }

        private MusicCollectionInfo _musicsCollectionInfo;
        public MusicCollectionInfo MusicsCollectionInfo
        {
            get { return _musicsCollectionInfo; }
            set
            {
                _musicsCollectionInfo = value;
                base.RaisePropertyChanged();
            }
        }

        private List<MenuCellInfo> _menus;

        public List<MenuCellInfo> Menus
        {
            get { return _menus; }
            set
            {
                _menus = value;
                RaisePropertyChanged();
            }
        }

        private string _musicCollectionArtPath;

        public string MusicCollectionArtPath
        {
            get { return _musicCollectionArtPath; }
            set
            {
                _musicCollectionArtPath = value;
                RaisePropertyChanged();
            }
        }
       

    }
}

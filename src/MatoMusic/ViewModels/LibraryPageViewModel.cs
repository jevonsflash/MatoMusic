using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using MatoMusic.Core;
using MatoMusic.Infrastructure;
using MatoMusic.Core.Models;
using MatoMusic.Core.ViewModel;
using MatoMusic.Core.Helper;

namespace MatoMusic.ViewModels
{
    public class LibraryPageViewModel : ViewModelBase
    {

        private readonly IMusicInfoManager musicInfoManager;
        private readonly MusicRelatedViewModel musicRelatedViewModel;

        public LibraryPageViewModel(IMusicInfoManager musicInfoManager, MusicRelatedViewModel musicRelatedViewModel)
        {
            PlayAllCommand = new Command(PlayAllAction, CanPlayExcute);
            QueueAllCommand = new Command(QueueAllAction, CanPlayExcute);
            GoUriCommand = new Command(GoUrlAction, c => true);
            this.PropertyChanged += LibraryPageViewModel_PropertyChanged;
            Init();
            this.musicRelatedViewModel = musicRelatedViewModel;
            this.musicInfoManager = musicInfoManager;

        }

        private void LibraryPageViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AGMusics) && AGMusics != null)
            {
                Canplay = CanPlayExcute(null);

                PlayAllCommand.ChangeCanExecute();
                QueueAllCommand.ChangeCanExecute();
                RaisePropertyChanged(nameof(Musics));
            }
        }

        private async void QueueAllAction(object obj)
        {
            await musicInfoManager.ClearQueue();
            var result = await musicInfoManager.CreateQueueEntrys(Musics);
            if (result)
            {
                await musicRelatedViewModel.RebuildMusicInfos();
                CommonHelper.ShowMsg(L("Msg_HasAddedQueue"));

            }
            else
            {
                CommonHelper.ShowMsg(L("Msg_AlreadyExists"));
            }
        }

        private async void PlayAllAction(object obj)
        {
            await musicInfoManager.ClearQueue();
            var result = await musicInfoManager.CreateQueueEntrys(Musics);
            if (result)
            {
                await musicRelatedViewModel.RebuildMusicInfos();

                var CurrentMusic = await musicInfoManager.GetQueueEntry();
                musicRelatedViewModel.CurrentMusic = CurrentMusic[0];
            }
            else
            {
                CommonHelper.ShowMsg(L("Msg_AlreadyExists"));
            }
        }
        private bool _canPlay;

        /// <summary>
        /// 是否可播放
        /// </summary>
        public bool Canplay
        {
            get { return _canPlay; }
            private set
            {
                _canPlay = value;
                RaisePropertyChanged();
            }
        }
        private void GoUrlAction(object obj)
        {
            CommonHelper.GoUrl(obj);
        }



        public Command GoUriCommand { get; set; }


        private bool CanPlayExcute(object obj)
        {
            var result = Musics.Count > 0;
            return result;

        }


        public List<MusicInfo> Musics { get => AGMusics.Origin; }
        private async void Init()
        {

            AGMusics = await InitMusics();
            AGAlbums = await InitAlbums();
            AGArtists = await InitArtists();


        }

        private AlphaGroupedObservableCollection<MusicInfo> _aGMusics;
        public AlphaGroupedObservableCollection<MusicInfo> AGMusics
        {
            get
            {
                if (_aGMusics == null)
                {

                    _aGMusics = new AlphaGroupedObservableCollection<MusicInfo>();
                }
                return _aGMusics;

            }
            set
            {
                _aGMusics = value;
                RaisePropertyChanged();

            }
        }

        private AlphaGroupedObservableCollection<ArtistInfo> _aGArtists;
        public AlphaGroupedObservableCollection<ArtistInfo> AGArtists
        {
            get
            {
                if (_aGArtists == null)
                {

                    _aGArtists = new AlphaGroupedObservableCollection<ArtistInfo>();
                }
                return _aGArtists;

            }
            set
            {
                _aGArtists = value;
                RaisePropertyChanged();

            }
        }

        private AlphaGroupedObservableCollection<AlbumInfo> _aGAlbums;
        public AlphaGroupedObservableCollection<AlbumInfo> AGAlbums
        {
            get
            {
                if (_aGAlbums == null)
                {

                    _aGAlbums = new AlphaGroupedObservableCollection<AlbumInfo>();
                }
                return _aGAlbums;

            }
            set
            {
                _aGAlbums = value;
                RaisePropertyChanged();

            }
        }
        private bool _isShowGrid;

        public bool IsShowGrid
        {
            get
            {

                return _isShowGrid;

            }
            set
            {
                _isShowGrid = value;
                RaisePropertyChanged();

            }
        }
        private Task<AlphaGroupedObservableCollection<ArtistInfo>> InitArtists()
        {
            var aGArtists = musicInfoManager.GetAlphaGroupedArtistInfo();
            return aGArtists;
        }

        private Task<AlphaGroupedObservableCollection<AlbumInfo>> InitAlbums()
        {
            var aGAlbums = musicInfoManager.GetAlphaGroupedAlbumInfo();
            return aGAlbums;
        }

        private Task<AlphaGroupedObservableCollection<MusicInfo>> InitMusics()
        {
            var aGMusics = musicInfoManager.GetAlphaGroupedMusicInfo();
            return aGMusics;
        }

        public async Task ChangeMusic(MusicInfo musicInfo)
        {
            await musicInfoManager.InsertToEndQueueEntry(musicInfo);

            await musicRelatedViewModel.RebuildMusicInfos();

            musicRelatedViewModel.ChangeMusic(musicInfo);
        }

        public Command PlayAllCommand { get; set; }
        public Command QueueAllCommand { get; set; }

    }
}

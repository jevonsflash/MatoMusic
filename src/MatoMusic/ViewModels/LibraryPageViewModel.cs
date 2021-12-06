using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using MatoMusic.Core;
using MatoMusic.Infrastructure;
using MatoMusic.Core.Models;
using MatoMusic.Core.ViewModel;
using MatoMusic.Core.Helper;
using Abp.Dependency;
using MatoMusic.Core.Services;

namespace MatoMusic.ViewModels
{
    public class LibraryPageViewModel : MusicRelatedViewModel
    {
        public LibraryPageViewModel(IMusicInfoManager musicInfoManager, MusicRelatedService musicRelatedService) : base(musicInfoManager, musicRelatedService)
        {
            PlayAllCommand = new Command(PlayAllAction, CanPlayAllExcute);
            QueueAllCommand = new Command(QueueAllAction, CanPlayAllExcute);
            GoUriCommand = new Command(GoUrlAction, c => true);
            this.PropertyChanged += LibraryPageViewModel_PropertyChanged;
        }

        private void LibraryPageViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AGMusics) && AGMusics != null)
            {
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
                //await RebuildMusicInfos();
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
                //await RebuildMusicInfos();

                var CurrentMusics = await musicInfoManager.GetQueueEntry();
                CurrentMusic = CurrentMusics[0];
            }
            else
            {
                CommonHelper.ShowMsg(L("Msg_AlreadyExists"));
            }
        }
     
        private void GoUrlAction(object obj)
        {
            CommonHelper.GoUrl(obj);
        }



        public Command GoUriCommand { get; set; }


  


        public new List<MusicInfo> Musics { get => AGMusics.Origin; }
        public async Task Init()
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

        public override async void ChangeMusic(MusicInfo musicInfo)
        {
            await musicInfoManager.InsertToEndQueueEntry(musicInfo);

            //await RebuildMusicInfos();

            base.ChangeMusic(musicInfo);
        }

        public Command PlayAllCommand { get; set; }
        public Command QueueAllCommand { get; set; }

    }
}

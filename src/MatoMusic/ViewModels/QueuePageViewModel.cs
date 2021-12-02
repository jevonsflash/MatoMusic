using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using MatoMusic.Core;
using MatoMusic.Core.ViewModel;
using Microsoft.Maui.Controls;
using Abp.Dependency;
using MatoMusic;
using MatoMusic.Core.Helper;

namespace ProjectMato.ViewModel;

public class QueuePageViewModel : ViewModelBase, ISingletonDependency
{
    private PlaylistChoosePage _playlistChoosePage;

    private readonly IMusicInfoManager musicInfoManager;
    private readonly IMusicSystem musicSystem;
    private readonly MusicRelatedViewModel musicRelatedViewModel;

    public QueuePageViewModel(IMusicInfoManager musicInfoManager
        , MusicRelatedViewModel musicRelatedViewModel)
    {
        this.DeleteCommand = new Command(DeleteAction, c => true);
        this.CleanQueueCommand = new Command(CleanQueueAction, CanDoAll);
        this.PlayCommand = new Command(PlayAction, CanDoAll);
        this.FlyBackCommand = new Command(FlyBackAction, CanDoAll);
        this.PlayAllCommand = new Command(PlayAllAction, c => true);
        this.PatchupCommand = new Command(PatchupAction, CanDoAll);
        this.PropertyChanged += QueuePageViewModel_PropertyChanged;
        musicSystem = DependencyService.Get<IMusicSystem>();
        musicSystem.MusicInfoManager = musicInfoManager;
        musicSystem.RebuildMusicInfos(MusicSystem_OnRebuildMusicInfosFinished);
        this.musicInfoManager = musicInfoManager;
        this.musicRelatedViewModel = musicRelatedViewModel;
        this.musicRelatedViewModel.OnMusicChanged += MusicRelatedViewModel_OnMusicChanged;

    }

    private void MusicRelatedViewModel_OnMusicChanged(object sender, EventArgs e)
    {
        foreach (var c in Musics)
        {
            if (c.IsPlaying)
            {
                c.IsPlaying = false;
                break;
            }
        }
        if (musicRelatedViewModel.Canplay)
        {
            var playingMusicId = musicRelatedViewModel.CurrentMusic.Id;

            var currentMusicInfo = Musics.FirstOrDefault(c => c.Id == playingMusicId);
            if (currentMusicInfo != null)
            {
                currentMusicInfo.IsPlaying = true;
            }
        }
    }

    private void FlyBackAction(object obj)
    {
        var playingMusicId = musicRelatedViewModel.CurrentMusic.Id;
        this.CurrentMusic = this.Musics.FirstOrDefault(c => c.Id == playingMusicId);
    }

    private async void PatchupAction(object obj)
    {
        if (this.Musics != null && this.Musics.Count > 0)
        {

            _playlistChoosePage = new PlaylistChoosePage();
            _playlistChoosePage.OnFinished += async (o, c) =>
            {
                if (c != null)
                {

                    var result = await musicInfoManager.CreatePlaylistEntrys(this.Musics.ToList(), c.Id);
                    if (result)
                    {
                        CommonHelper.ShowMsg(string.Format("{0}{1}", L("Msg_HasAdded"), c.Title));
                    }
                    else
                    {
                        CommonHelper.ShowMsg(L("Msg_AddFaild"));
                    }
                }
                //PopupNavigation.PopAsync();
            };

            //await PopupNavigation.PushAsync(_playlistChoosePage);
        }
    }

    private async void QueuePageViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(this.Musics) && Musics != null)
        {
            RaiseAllExecuteChanged();
        }

        else if (e.PropertyName == nameof(CurrentMusic) && CurrentMusic != null)
        {
            if (musicRelatedViewModel.Canplay)
            {
                var playingMusicId = musicRelatedViewModel.CurrentMusic.Id;
                if (this.CurrentMusic.Id != playingMusicId)
                {
                    musicRelatedViewModel.ChangeMusic(CurrentMusic);
                }
            }
            else
            {
                musicRelatedViewModel.ChangeMusic(CurrentMusic);

            }
            await Task.Delay(300);
            this.CurrentMusic = null;
        }
    }


    private void PlayAction(object obj)
    {
    }

    private void MusicSystem_OnRebuildMusicInfosFinished()
    {
        Device.BeginInvokeOnMainThread(() =>
        {
            InitMusics();
            foreach (var c in Musics)
            {
                if (c.IsPlaying)
                {
                    c.IsPlaying = false;
                    break;
                }
            }
            if (musicRelatedViewModel.Canplay)
            {
                var playingMusicId = musicRelatedViewModel.CurrentMusic.Id;

                this.CurrentMusic = this.Musics.FirstOrDefault(c => c.Id == playingMusicId);
                if (CurrentMusic != null)
                {
                    CurrentMusic.IsPlaying = true;
                }
                //ImageService.Instance.InvalidateMemoryCache();

            }
        });

    }

    private void CleanQueueAction(object obj)
    {
        this.Musics.Clear();
        RaiseAllExecuteChanged();

        CommonHelper.ShowMsg(L("Msg_QueueCleaned"));
    }

    private void Musics_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Move)
        {
            var oldIndex = e.OldStartingIndex;
            var newIndex = e.NewStartingIndex;
            musicInfoManager.ReorderQueue(Musics[oldIndex], Musics[newIndex]);
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            musicInfoManager.DeleteMusicInfoFormQueueEntry(e.OldItems[0] as MusicInfo);
        }
        else if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            musicInfoManager.ClearQueue();
        }
        musicSystem.RebuildMusicInfos();

        RaiseAllExecuteChanged();

    }

    public void DeleteAction(object obj)
    {
        Musics.Remove(obj as MusicInfo);
    }


    private MusicInfo _currentMusic;

    public MusicInfo CurrentMusic
    {
        get { return _currentMusic; }
        set
        {
            _currentMusic = value;
            RaisePropertyChanged();
        }
    }


    private ObservableCollection<MusicInfo> musics;


    public ObservableCollection<MusicInfo> Musics
    {
        get
        {
            if (musics == null)
            {
                musics = new ObservableCollection<MusicInfo>();
            }
            return musics;
        }
        set
        {
            musics = value;
            RaisePropertyChanged();
        }
    }



    private bool _isEmpty;


    public bool IsEmpty
    {
        get { return _isEmpty; }
        private set
        {
            _isEmpty = value;
            RaisePropertyChanged();
        }
    }

    private bool CanDoAll(object obj)
    {
        var result = this.Musics.Count > 0;
        return result;

    }

    private async void InitMusics()
    {
        Musics = new ObservableCollection<MusicInfo>(musicSystem.MusicInfos);
        this.Musics.CollectionChanged += Musics_CollectionChanged;
    }
    private async void PlayAllAction(object obj)
    {

        await musicSystem.RebuildMusicInfos(MusicSystem_OnRebuildMusicInfosFinished);

        var isSucc = await musicInfoManager.GetMusicInfos();
        if (!isSucc.IsSucess)
        {
            CommonHelper.ShowNoAuthorized();

        }
        var musicInfos = isSucc.Result;
        Musics = new ObservableCollection<MusicInfo>(musicInfos);
        this.Musics.CollectionChanged += Musics_CollectionChanged;
        var result = await musicInfoManager.CreateQueueEntrys(musicInfos);
        if (result)
        {
            var currentMusic = await musicInfoManager.GetQueueEntry();
            if (currentMusic.Count > 0)
            {
                Random r = new Random();
                var randomIndex = r.Next(currentMusic.Count);
                musicRelatedViewModel.IsShuffle = true;
                musicRelatedViewModel.CurrentMusic = currentMusic[randomIndex];

            }
            CommonHelper.ShowMsg("随机播放中");

        }
        else
        {
            CommonHelper.ShowMsg("失败");
        }

    }

    private void RaiseAllExecuteChanged()
    {
        PlayAllCommand.ChangeCanExecute();
        CleanQueueCommand.ChangeCanExecute();
        PatchupCommand.ChangeCanExecute();
        FlyBackCommand.ChangeCanExecute();
        this.IsEmpty = !CanDoAll(null);
    }

    public Command FlyBackCommand { get; set; }

    public Command DeleteCommand { get; set; }
    public Command CleanQueueCommand { get; set; }
    public Command PlayCommand { get; set; }

    public Command PlayAllCommand { get; private set; }
    public Command PatchupCommand { get; private set; }
}

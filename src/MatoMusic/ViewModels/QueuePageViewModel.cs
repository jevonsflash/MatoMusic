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
using MatoMusic.Services;

namespace MatoMusic.ViewModels;

public class QueuePageViewModel : MusicRelatedViewModel
{
    private PlaylistChoosePage _playlistChoosePage;
    private readonly NavigationService navigationService;


    public QueuePageViewModel(IMusicInfoManager musicInfoManager,
            NavigationService navigationService) : base(musicInfoManager)
    {
        DeleteCommand = new Command(DeleteAction, c => true);
        CleanQueueCommand = new Command(CleanQueueAction, CanDoAll);
        PlayCommand = new Command(PlayAction, CanDoAll);
        FlyBackCommand = new Command(FlyBackAction, CanDoAll);
        PlayAllCommand = new Command(PlayAllAction, c => true);
        PatchupCommand = new Command(PatchupAction, CanDoAll);
        PropertyChanged += QueuePageViewModel_PropertyChanged;
        this.navigationService = navigationService;
        this.OnMusicChanged += MusicRelatedViewModel_OnMusicChanged;
        this.RebuildMusicInfosHandler = MusicSystem_OnRebuildMusicInfosFinished;
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
        if (Canplay)
        {
            var playingMusicId = CurrentMusic.Id;

            var currentMusicInfo = Musics.FirstOrDefault(c => c.Id == playingMusicId);
            if (currentMusicInfo != null)
            {
                currentMusicInfo.IsPlaying = true;
            }
        }
    }

    private void FlyBackAction(object obj)
    {
        var playingMusicId = CurrentMusic.Id;
        CurrentMusic = Musics.FirstOrDefault(c => c.Id == playingMusicId);
    }

    private async void PatchupAction(object obj)
    {
        if (Musics != null && Musics.Count > 0)
        {

            _playlistChoosePage = new PlaylistChoosePage();
            _playlistChoosePage.OnFinished += async (o, c) =>
            {
                if (c != null)
                {

                    var result = await musicInfoManager.CreatePlaylistEntrys(Musics.ToList(), c.Id);
                    if (result)
                    {
                        CommonHelper.ShowMsg(string.Format("{0}{1}", L("Msg_HasAdded"), c.Title));
                    }
                    else
                    {
                        CommonHelper.ShowMsg(L("Msg_AddFaild"));
                    }
                }
                await navigationService.PopAsync();
            };

            await navigationService.PushAsync(_playlistChoosePage);
        }
    }

    private async void QueuePageViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Musics) && Musics != null)
        {
            RaiseAllExecuteChanged();
        }

        else if (e.PropertyName == nameof(CurrentMusic) && CurrentMusic != null)
        {
            if (Canplay)
            {
                var playingMusicId = CurrentMusic.Id;
                if (CurrentMusic.Id != playingMusicId)
                {
                    ChangeMusic(CurrentMusic);
                }
            }
            else
            {
                ChangeMusic(CurrentMusic);

            }
            await Task.Delay(300);
            CurrentMusic = null;
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
            if (Canplay)
            {
                var playingMusicId = CurrentMusic.Id;

                CurrentMusic = Musics.FirstOrDefault(c => c.Id == playingMusicId);
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
        Musics.Clear();
        RaiseAllExecuteChanged();

        CommonHelper.ShowMsg(L("Msg_QueueCleaned"));
    }

    private async void Musics_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Move)
        {
            var oldIndex = e.OldStartingIndex;
            var newIndex = e.NewStartingIndex;
            musicInfoManager.ReorderQueue(Musics[oldIndex], Musics[newIndex]);
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            await musicInfoManager.DeleteMusicInfoFormQueueEntry(e.OldItems[0] as MusicInfo);
        }
        else if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            await musicInfoManager.ClearQueue();
        }
        await RebuildMusicInfos();

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
        var result = Musics.Count > 0;
        return result;

    }

    private void InitMusics()
    {
        Musics = new ObservableCollection<MusicInfo>(Musics);
        Musics.CollectionChanged += Musics_CollectionChanged;
    }
    private async void PlayAllAction(object obj)
    {

        await RebuildMusicInfos(MusicSystem_OnRebuildMusicInfosFinished);

        var isSucc = await musicInfoManager.GetMusicInfos();
        if (!isSucc.IsSucess)
        {
            CommonHelper.ShowNoAuthorized();

        }
        var musicInfos = isSucc.Result;
        Musics = new ObservableCollection<MusicInfo>(musicInfos);
        Musics.CollectionChanged += Musics_CollectionChanged;
        var result = await musicInfoManager.CreateQueueEntrys(musicInfos);
        if (result)
        {
            var currentMusic = await musicInfoManager.GetQueueEntry();
            if (currentMusic.Count > 0)
            {
                Random r = new Random();
                var randomIndex = r.Next(currentMusic.Count);
                IsShuffle = true;
                CurrentMusic = currentMusic[randomIndex];

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
        IsEmpty = !CanDoAll(null);
    }

    public Command FlyBackCommand { get; set; }

    public Command DeleteCommand { get; set; }
    public Command CleanQueueCommand { get; set; }
    public Command PlayCommand { get; set; }

    public Command PlayAllCommand { get; private set; }
    public Command PatchupCommand { get; private set; }
}

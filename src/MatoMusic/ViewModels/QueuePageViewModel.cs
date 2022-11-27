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
using MatoMusic.Core.Services;
using MatoMusic.Core.Interfaces;

namespace MatoMusic.ViewModels;

public class QueuePageViewModel : MusicRelatedViewModel
{
    private PlaylistChoosePage _playlistChoosePage;
    private readonly NavigationService navigationService;


    public QueuePageViewModel(NavigationService navigationService)
    {
        DeleteCommand = new Command(DeleteAction, c => true);
        CleanQueueCommand = new Command(CleanQueueAction, CanPlayAllExcute);
        FlyBackCommand = new Command(FlyBackAction, CanPlayAllExcute);
        PlayAllCommand = new Command(PlayAllAction, (e) => true);
        PatchupCommand = new Command(PatchupAction, CanPlayAllExcute);
        PropertyChanged += QueuePageViewModel_PropertyChanged;
        this.navigationService = navigationService;
        this.OnMusicChanged += MusicRelatedViewModel_OnMusicChanged;
        this.MusicRelatedService.OnBuildMusicInfosFinished+=MusicRelatedService_OnBuildMusicInfosFinished;
    }

    private void MusicRelatedService_OnBuildMusicInfosFinished(object sender, EventArgs e)
    {
        var musicInfos = base.Musics;
        Musics = new ObservableCollection<MusicInfo>(musicInfos);
        Musics.CollectionChanged += Musics_CollectionChanged;
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

            using (var playlistChoosePageWrapper = IocManager.Instance.ResolveAsDisposable<PlaylistChoosePage>(new { musicInfoManager=MusicInfoManager }))
            {
                _playlistChoosePage = playlistChoosePageWrapper.Object;
                _playlistChoosePage.OnFinished += async (o, c) =>
            {
                if (c != null)
                {

                    var result = await MusicInfoManager.CreatePlaylistEntrys(Musics.ToList(), c.Id);
                    if (result)
                    {
                        CommonHelper.ShowMsg(string.Format("{0}{1}", L("Msg_HasAdded"), c.Title));
                    }
                    else
                    {
                        CommonHelper.ShowMsg(L("Msg_AddFaild"));
                    }
                }
                await navigationService.HidePopupAsync(_playlistChoosePage);
            };

                await navigationService.ShowPopupAsync(_playlistChoosePage);
            }
        }
    }

    private void QueuePageViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Musics) && Musics != null)
        {
            RaiseAllExecuteChanged();
        }

        else if (e.PropertyName == nameof(CurrentMusic))
        {
            if (Canplay)
            {
                var playingMusicId = CurrentMusic.Id;
                if (CurrentMusic.Id != playingMusicId)
                {
                    ChangeMusic(CurrentMusic);
                }
            }
        }
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
            MusicInfoManager.ReorderQueue(Musics[oldIndex], Musics[newIndex]);
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            await MusicInfoManager.DeleteMusicInfoFormQueueEntry(e.OldItems[0] as MusicInfo);
        }
        else if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            await MusicInfoManager.ClearQueue();
        }
        //await RebuildMusicInfos();

        RaiseAllExecuteChanged();

    }

    public void DeleteAction(object obj)
    {
        Musics.Remove(obj as MusicInfo);
    }



    private ObservableCollection<MusicInfo> musics;


    public new ObservableCollection<MusicInfo> Musics
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

    private void InitMusics()
    {
        Musics = new ObservableCollection<MusicInfo>(base.Musics);
        Musics.CollectionChanged += Musics_CollectionChanged;
    }
    private async void PlayAllAction(object obj)
    {


        var isSucc = await MusicInfoManager.GetMusicInfos();
        if (!isSucc.IsSucess)
        {
            CommonHelper.ShowNoAuthorized();

        }
        var musicInfos = isSucc.Result;
        var result = await MusicInfoManager.CreateQueueEntrys(musicInfos);
        if (result)
        {
            await RebuildMusicInfos(async () =>
            {
                var currentQueueMusics= await MusicInfoManager.GetQueueEntry();
                Musics = new ObservableCollection<MusicInfo>(currentQueueMusics);
                Musics.CollectionChanged += Musics_CollectionChanged;
                if (currentQueueMusics.Count > 0)
                {
                    Random r = new Random();
                    var randomIndex = r.Next(currentQueueMusics.Count);
                    IsShuffle = true;
                    CurrentMusic = currentQueueMusics[randomIndex];

                }
                CommonHelper.ShowMsg("随机播放中");
            });

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
        IsEmpty = !CanPlayAllExcute(null);
    }

    public Command FlyBackCommand { get; set; }

    public Command DeleteCommand { get; set; }
    public Command CleanQueueCommand { get; set; }
    public Command PlayAllCommand { get; private set; }
    public Command PatchupCommand { get; private set; }
}

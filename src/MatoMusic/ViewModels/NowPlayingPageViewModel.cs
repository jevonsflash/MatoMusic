using System;
using Abp.Dependency;
using Acr.UserDialogs;
using MatoMusic.Core;
using MatoMusic.Core.Interfaces;
using MatoMusic.Core.ViewModel;
using MatoMusic.Infrastructure.Helper;
using Microsoft.Maui.Controls;

namespace MatoMusic.ViewModels
{
    public class NowPlayingPageViewModel : ViewModelBase, ISingletonDependency
    {
        private readonly IMusicSystem musicSystem;
        private readonly IMusicInfoManager musicInfoManager;
        private readonly MusicRelatedViewModel musicRelatedViewModel;

        public NowPlayingPageViewModel(IMusicSystem musicSystem, IMusicInfoManager musicInfoManager, MusicRelatedViewModel musicRelatedViewModel)
        {
            SwitchPannelCommand = new Command(SwitchPannelAction, c => true);
            PlayAllCommand = new Command(PlayAllAction, c => true);
            IsLrcPanel = false;
            this.musicSystem = musicSystem;
            this.musicInfoManager = musicInfoManager;
            this.musicRelatedViewModel = musicRelatedViewModel;
        }

        public MusicRelatedViewModel CurrentMusicRelatedViewModel => this.musicRelatedViewModel;

        private void SwitchPannelAction(object obj)
        {
            IsLrcPanel = !IsLrcPanel;
        }



        private bool _isLrcPanel;

        public bool IsLrcPanel
        {
            get { return _isLrcPanel; }
            set
            {
                if (_isLrcPanel != value)
                {
                    _isLrcPanel = value;
                    RaisePropertyChanged();


                }
            }
        }

        private void MusicSystem_OnRebuildMusicInfosFinished()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                UserDialogs.Instance.HideLoading();

            });

        }

        private async void PlayAllAction(object obj)
        {
            UserDialogs.Instance.ShowLoading();
            musicSystem.RebuildMusicInfos(MusicSystem_OnRebuildMusicInfosFinished);

            var isSucc = await musicInfoManager.GetMusicInfos();
            if (!isSucc.IsSucess)
            {
                CommonHelper.ShowNoAuthorized();

            }
            var musicInfos = isSucc.Result;

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
            UserDialogs.Instance.HideLoading();

        }

        public Command SwitchPannelCommand { get; set; }
        public Command PlayAllCommand { get; private set; }

    }
}

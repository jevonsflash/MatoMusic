using System;
using Abp.Dependency;
using MatoMusic.Core;
using MatoMusic.Core.Helper;
using MatoMusic.Core.ViewModel;
using Microsoft.Maui.Controls;

namespace MatoMusic.ViewModels
{
    public class NowPlayingPageViewModel : MusicRelatedViewModel
    {

        public NowPlayingPageViewModel(IMusicInfoManager musicInfoManager) : base(musicInfoManager)
        {
            SwitchPannelCommand = new Command(SwitchPannelAction, c => true);
            PlayAllCommand = new Command(PlayAllAction, c => true);
            IsLrcPanel = false;
        }


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

            });

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

        public Command SwitchPannelCommand { get; set; }
        public Command PlayAllCommand { get; private set; }

    }
}

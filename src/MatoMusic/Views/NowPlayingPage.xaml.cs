using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Localization;
using MatoMusic.Common;
using MatoMusic.Core;
using MatoMusic.Core.Helper;
using MatoMusic.Core.Interfaces;
using MatoMusic.Core.Localization;
using MatoMusic.Core.Models;
using MatoMusic.Core.Settings;
using MatoMusic.Core.ViewModel;
using MatoMusic.Services;
using MatoMusic.ViewModels;
using Microsoft.Maui.Controls;

namespace MatoMusic
{
    public partial class NowPlayingPage : ContentPageBase, ITransientDependency
    {

        private MusicFunctionPage _musicFunctionPage;
        private PlaylistChoosePage _playlistChoosePage;

        public NowPlayingPage(NowPlayingPageViewModel nowPlayingPageViewModel)
        {
            InitializeComponent();
            this.BindingContext = nowPlayingPageViewModel;
            this.Disappearing += NowPlayingPage_Disappearing;
            this.Appearing += NowPlayingPage_Appearing;

        }

        private void NowPlayingPage_Appearing(object sender, EventArgs e)
        {
            var isHideQueueButton = SettingManager.GetSettingValueForApplication<bool>(CommonSettingNames.IsHideQueueButton);
            this.QueueControlLayout.IsVisible = !isHideQueueButton;
        }

        private void NowPlayingPage_Disappearing(object sender, EventArgs e)
        {
            var viewModel = BindingContext as NowPlayingPageViewModel;
            if (viewModel != null)
                viewModel.IsLrcPanel = false;
        }

        private void OnValueChanged(object sender, ValueChangedEventArgs e)
        {
            var bindableObject = sender as BindableObject;
            if (bindableObject != null)
            {
                var musicRelatedViewModel = bindableObject.BindingContext as MusicRelatedViewModel;
                if (musicRelatedViewModel != null)
                    musicRelatedViewModel.ChangeProgess(e.NewValue);
            }
        }

        private async void Button_OnClicked(object sender, EventArgs e)
        {
            await navigationService.GoPageAsync("QueuePage");
        }


        private async void MoreButton_OnClicked(object sender, EventArgs e)
        {


            var imageButton = sender as BindableObject;
            if (!(imageButton.BindingContext as MusicRelatedViewModel).CanPlayExcute(null))
            {
                return;
            }
            var musicInfo = (imageButton.BindingContext as MusicRelatedViewModel).CurrentMusic;
            var mainMenuCellInfos = new List<MenuCellInfo>()
            {
                new MenuCellInfo() {Title = "添加到..", Code = "AddToPlaylist", Icon = "addto"},
                new MenuCellInfo()
                {
                    Title = (musicInfo as MusicInfo).Artist,
                    Code = "GoArtistPage",
                    Icon = "microphone2"
                },
                new MenuCellInfo()
                {
                    Title = (musicInfo as MusicInfo).AlbumTitle,
                    Code = "GoAlbumPage",
                    Icon = "cd2"
                },


            };
            _musicFunctionPage = new MusicFunctionPage(musicInfo, mainMenuCellInfos);
            _musicFunctionPage.OnFinished += MusicFunctionPage_OnFinished;

            await navigationService.ShowPopupAsync(_musicFunctionPage);

        }

        private async void MusicFunctionPage_OnFinished(object sender, MusicFunctionEventArgs e)
        {
            if (e.MusicInfo == null)
            {
                return;
            }
            await navigationService.PopToRootAsync();
            if (e.MenuCellInfo.Code == "AddToPlaylist")
            {
                using (var playlistChoosePageWrapper = IocManager.Instance.ResolveAsDisposable<PlaylistChoosePage>(new { musicInfoManager = MusicInfoManager }))
                {
                    _playlistChoosePage = playlistChoosePageWrapper.Object;
                    _playlistChoosePage.OnFinished += async (o, c) =>
                {
                    if (c != null)
                    {
                        var result = await MusicInfoManager.CreatePlaylistEntry(e.MusicInfo as MusicInfo, c.Id);
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

            else if (e.MenuCellInfo.Code == "GoAlbumPage")
            {
                List<AlbumInfo> list;
                var isSucc = await MusicInfoManager.GetAlbumInfos();
                if (!isSucc.IsSucess)
                {
                    CommonHelper.ShowNoAuthorized();
                }
                list = isSucc.Result;
                var albumInfo = list.Find(c => c.Title == (e.MusicInfo as MusicInfo).AlbumTitle);
                navigationService.GoNavigate("MusicCollectionPage", new object[] { albumInfo });
            }
            else if (e.MenuCellInfo.Code == "GoArtistPage")
            {
                List<ArtistInfo> list;
                var isSucc = await MusicInfoManager.GetArtistInfos();
                if (!isSucc.IsSucess)
                {
                    CommonHelper.ShowNoAuthorized();

                }
                list = isSucc.Result;
                var artistInfo = list.Find(c => c.Title == (e.MusicInfo as MusicInfo).Artist);
                navigationService.GoNavigate("MusicCollectionPage", new object[] { artistInfo });
            }

        }

        private void LyricView_OnOnClosed(object sender, EventArgs e)
        {
            var nowPlayingPageViewModel = this.BindingContext as NowPlayingPageViewModel;
            if (nowPlayingPageViewModel != null)
                nowPlayingPageViewModel.IsLrcPanel = !nowPlayingPageViewModel.IsLrcPanel;
        }

        private async void GoLibrary_OnClicked(object sender, EventArgs e)
        {
            await navigationService.GoPageAsync("LibraryPage");
        }

        private void BindableObject_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsVisible")
            {
            }
        }

    }
}

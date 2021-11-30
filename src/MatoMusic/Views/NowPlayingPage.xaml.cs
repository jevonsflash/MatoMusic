using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Abp.Configuration;
using Abp.Dependency;
using MatoMusic.Core;
using MatoMusic.Core.Models;
using MatoMusic.Core.Settings;
using MatoMusic.Core.ViewModel;
using MatoMusic.Infrastructure.Helper;
using MatoMusic.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Essentials;

namespace MatoMusic
{
    public partial class NowPlayingPage : ContentPage, ITransientDependency
    {
        public IMusicInfoManager MusicInfoManager => DependencyService.Get<IMusicInfoManager>();

        private MusicFunctionPage _musicFunctionPage;
        private PlaylistChoosePage _playlistChoosePage;
        private readonly ISettingManager settingManager;

        public NowPlayingPage(NowPlayingPageViewModel nowPlayingPageViewModel, ISettingManager settingManager
)
        {
            InitializeComponent();
            this.Disappearing += NowPlayingPage_Disappearing;
            this.SizeChanged += NowPlayingPage_SizeChanged;
            this.Appearing += NowPlayingPage_Appearing;
            this.BindingContext = nowPlayingPageViewModel;
            this.settingManager = settingManager;
        }

        private void NowPlayingPage_Appearing(object sender, EventArgs e)
        {
            var isHideQueueButton = settingManager.GetSettingValueForApplication<bool>(CommonSettingNames.IsHideQueueButton);
            this.QueueControlLayout.IsVisible = !isHideQueueButton;
            var viewModel = BindingContext as NowPlayingPageViewModel;
            if (viewModel != null)
                viewModel.CurrentMusicRelatedViewModel.InitPreviewAndNextMusic();

        }


        private async void NowPlayingPage_SizeChanged(object sender, EventArgs e)
        {
            await Task.Delay(500);
            if (this.Width > 0
                && this.Height > 0
                && PreAlbumArt.Width > 0
                && PreAlbumArt.Height > 0)
            {

                Debug.WriteLine("W:" + this.Width);
                Debug.WriteLine("H:" + this.Height);

                InitPreAlbumArtEdgeThickness();
                InitNextAlbumArtEdgeThickness();
            }


        }

        private void InitNextAlbumArtEdgeThickness()
        {
            int edgeThickness = 22;
            var nextRefwidth = Math.Min(NextAlbumArt.Width, NextAlbumArt.Height);
            var nextTransWidth = (this.Width + nextRefwidth) / 2 - edgeThickness;
            this.NextAlbumArt.TranslateTo(nextTransWidth, this.NextAlbumArt.Y);
            //this.NextAlbumArt.TranslationX = nextTransWidth;
        }

        private void InitPreAlbumArtEdgeThickness()
        {
            var edgeThickness = 22;
            var preRefwidth = Math.Min(PreAlbumArt.Width, PreAlbumArt.Height);
            var preTransWidth = -(this.Width + preRefwidth) / 2 + edgeThickness;
            this.PreAlbumArt.TranslateTo(preTransWidth, this.PreAlbumArt.Y);
            //mthis.PreAlbumArt.TranslationX = preTransWidth;
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

        private void Button_OnClicked(object sender, EventArgs e)
        {
            CommonHelper.GoPage("QueuePage");
        }


        private void MoreButton_OnClicked(object sender, EventArgs e)
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
            //_musicFunctionPage = new MusicFunctionPage(musicInfo, mainMenuCellInfos);
            //_musicFunctionPage.OnFinished += MusicFunctionPage_OnFinished;

            //PopupNavigation.PushAsync(_musicFunctionPage);

        }

        //private async void MusicFunctionPage_OnFinished(object sender, MusicFunctionEventArgs e)
        //{
        //    if (e.MusicInfo == null)
        //    {
        //        return;
        //    }
        //    await PopupNavigation.PopAllAsync();
        //    if (e.MenuCellInfo.Code == "AddToPlaylist")
        //    {
        //        _playlistChoosePage = new PlaylistChoosePage();
        //        _playlistChoosePage.OnFinished += (o, c) =>
        //        {
        //            if (c != null)
        //            {
        //                var result = MusicInfoManager.CreatePlaylistEntry(e.MusicInfo as MusicInfo, c.PlaylistId);
        //                if (result)
        //                {
        //                    CommonHelper.ShowMsg(string.Format("{0}{1}", TranslateExtension.Translate("Msg_HasAdded"), c.Name));
        //                }
        //                else
        //                {
        //                    CommonHelper.ShowMsg(TranslateExtension.Translate("Msg_AddFaild"));
        //                }
        //            }
        //            PopupNavigation.PopAsync();
        //        };
        //        await PopupNavigation.PushAsync(_playlistChoosePage);

        //    }

        //    else if (e.MenuCellInfo.Code == "GoAlbumPage")
        //    {
        //        List<AlbumInfo> list;
        //        var isSucc = await MusicInfoManager.GetAlbumInfos();
        //        if (!isSucc.IsSucess)
        //        {
        //            CommonHelper.ShowNoAuthorized();
        //        }
        //        list = isSucc.Result;
        //        var albumInfo = list.Find(c => c.Title == (e.MusicInfo as MusicInfo).AlbumTitle);
        //        CommonHelper.GoNavigate("MusicCollectionPage", new object[] { albumInfo });
        //    }
        //    else if (e.MenuCellInfo.Code == "GoArtistPage")
        //    {
        //        List<ArtistInfo> list;
        //        var isSucc = await MusicInfoManager.GetArtistInfos();
        //        if (!isSucc.IsSucess)
        //        {
        //            CommonHelper.ShowNoAuthorized();

        //        }
        //        list = isSucc.Result;
        //        var artistInfo = list.Find(c => c.Title == (e.MusicInfo as MusicInfo).Artist);
        //        CommonHelper.GoNavigate("MusicCollectionPage", new object[] { artistInfo });
        //    }

        //}

        private void LyricView_OnOnClosed(object sender, EventArgs e)
        {
            var nowPlayingPageViewModel = this.BindingContext as NowPlayingPageViewModel;
            if (nowPlayingPageViewModel != null)
                nowPlayingPageViewModel.IsLrcPanel = !nowPlayingPageViewModel.IsLrcPanel;
        }

        private void GoLibrary_OnClicked(object sender, EventArgs e)
        {
            CommonHelper.GoPage("LibraryPage");
        }

        private void BindableObject_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //if (e.PropertyName == "IsVisible")
            //{
            //    var aa = (sender as LyricView).IsVisible;
            //    Debug.WriteLine(aa);
            //}
        }

        //private void PreAlbumArt_OnFinish(object sender, CachedImageEvents.FinishEventArgs e)
        //{
        //    InitPreAlbumArtEdgeThickness();
        //}

        //private void NextAlbumArt_OnFinish(object sender, CachedImageEvents.FinishEventArgs e)
        //{
        //    InitNextAlbumArtEdgeThickness();
        //}
    }
}

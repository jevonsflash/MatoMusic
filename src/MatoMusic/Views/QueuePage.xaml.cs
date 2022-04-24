using System;
using System.Collections.Generic;
using Abp.Dependency;
using Abp.Localization;
using MatoMusic.Common;
using MatoMusic.Core;
using MatoMusic.Core.Helper;
using MatoMusic.Core.Models;
using MatoMusic.Infrastructure.Common;
using MatoMusic.Services;
using MatoMusic.ViewModels;
using Microsoft.Maui.Controls;


namespace MatoMusic
{
    public partial class QueuePage : ContentPageBase, ITransientDependency
    {
        private readonly MusicFunctionManager musicFunctionManager;

        public QueuePage(
            QueuePageViewModel queuePageViewModel,
            MusicFunctionManager musicFunctionManager)
        {
            this.musicFunctionManager = musicFunctionManager; 
            InitializeComponent();
            this.BindingContext = queuePageViewModel;
            this.PlayAllLabel.Text = FaIcons.IconRandom;
            this.AddSongLabel2.Text = FaIcons.IconMusic;

        }



        private void ListView_OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            //MusicRelatedViewModel.Current.ChangeMusic(e.SelectedItem as MusicInfo);

        }
        private async void MusicMoreButton_OnClicked(object sender, EventArgs e)
        {
            var musicInfo = (sender as BindableObject).BindingContext;
            var _mainMenuCellInfos = new List<MenuCellInfo>()
            {
                new MenuCellInfo() {Title = L(MatoMusicConsts.LocalizationSourceName,"Remove"), Code = "Delete", Icon = "remove"},
                new MenuCellInfo() {Title = L(MatoMusicConsts.LocalizationSourceName,"AddTo"), Code = "AddToPlaylist", Icon = "addto"},
                new MenuCellInfo() {Title = L(MatoMusicConsts.LocalizationSourceName,"PlayNext"), Code = "NextPlay", Icon = "playnext"},
                //new MenuCellInfo() {Title = L(MatoMusicConsts.LocalizationSourceName,"AddToQueue2"), Code = "AddToQueue", Icon = "addtostack"},
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
            var _musicFunctionPage = new MusicFunctionPage(musicInfo as IBasicInfo, _mainMenuCellInfos);
            _musicFunctionPage.OnFinished += _musicFunctionPage_OnFinished;

            await navigationService.ShowPopupAsync(_musicFunctionPage);

        }

        private void _musicFunctionPage_OnFinished(object sender, MusicFunctionEventArgs e)
        {
            if (e.MenuCellInfo.Code == "Delete")
            {
                var musicInfo = e.MusicInfo;
                var queuePageViewModel = this.BindingContext as QueuePageViewModel;
                if (queuePageViewModel != null)
                    queuePageViewModel.DeleteAction(musicInfo);
            }
            musicFunctionManager.OnMusicFunctionFinished(e);
        }


        private async void Button_OnClicked(object sender, EventArgs e)
        {
            await navigationService.GoPageAsync("LibraryPage");
        }

        private void FavouriteButton_OnClicked(object sender, EventArgs e)
        {
            var musicInfo = (sender as BindableObject)?.BindingContext as MusicInfo;
            if (musicInfo != null)
            {
                musicInfo.IsFavourite = !musicInfo.IsFavourite;
                if (musicInfo.IsFavourite)
                {
                    CommonHelper.ShowMsg("我最喜爱");

                }
            }
        }
    }
}

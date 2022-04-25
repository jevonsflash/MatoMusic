using System;
using System.Collections.Generic;
using System.Drawing;
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

    public partial class MusicPage : ContentPageBase, ITransientDependency
    {
        private readonly MusicFunctionManager musicFunctionManager;


        public MusicPage(LibraryPageViewModel libraryPageViewModel,
            MusicFunctionManager musicFunctionManager)
        {
            this.musicFunctionManager = musicFunctionManager;
            InitializeComponent();
            this.BindingContext = libraryPageViewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await (this.BindingContext as LibraryPageViewModel).Init();
        }


        private void ListView_OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem is ArtistInfo)
            {
                var artistInfo = e.SelectedItem as ArtistInfo;
                navigationService.GoNavigate("MusicCollectionPage", new object[] { artistInfo });

            }
            else if (e.SelectedItem is AlbumInfo)
            {
                var albumInfo = e.SelectedItem as AlbumInfo;
                navigationService.GoNavigate("MusicCollectionPage", new object[] { albumInfo });
            }
                (sender as ListView).SelectedItem = null;
        }

        private void MusicListView_OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var musicInfo = e.SelectedItem as MusicInfo;
            if (musicInfo != null)
            {
                (this.BindingContext as LibraryPageViewModel).ChangeMusic(musicInfo);
            }
        }

        private async void MusicMoreButton_OnClicked(object sender, EventArgs e)
        {
            var musicInfo = (sender as BindableObject).BindingContext;
            var _mainMenuCellInfos = new List<MenuCellInfo>()
            {
                new MenuCellInfo() {Title = L(MatoMusicConsts.LocalizationSourceName,"AddTo"), Code = "AddToPlaylist", Icon = "addto"},
                new MenuCellInfo() {Title = L(MatoMusicConsts.LocalizationSourceName,"PlayNext"), Code = "NextPlay", Icon = "playnext"},
                new MenuCellInfo() {Title = L(MatoMusicConsts.LocalizationSourceName,"AddToQueue2"), Code = "AddToQueue", Icon = "addtostack"},
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
            musicFunctionManager.OnMusicFunctionFinished(e);
        }

        private async void SearchButton_OnClicked(object sender, EventArgs e)
        {
           await navigationService.GoPageAsync("SearchPage");
        }

    }

}


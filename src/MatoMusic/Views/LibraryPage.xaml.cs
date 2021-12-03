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
using Microsoft.Maui.Essentials;

namespace MatoMusic
{

    public partial class LibraryPage : ContentPage, ITransientDependency
    {
        private readonly NavigationService navigationService;
        private readonly ILocalizationManager localizationManager;
        private readonly MusicFunctionManager musicFunctionManager;


        public LibraryPage(LibraryPageViewModel libraryPageViewModel,
            NavigationService navigationService,

            ILocalizationManager localizationManager, MusicFunctionManager musicFunctionManager)
        {
            this.navigationService = navigationService;
            this.localizationManager = localizationManager;
            this.musicFunctionManager = musicFunctionManager; 
            InitializeComponent();
            this.BindingContext = libraryPageViewModel;

            Init();

        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            this.Init();
           await (this.BindingContext as LibraryPageViewModel).init();
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

        private async void MusicListView_OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var musicInfo = e.SelectedItem as MusicInfo;
            if (musicInfo != null)
            {
                await (this.BindingContext as LibraryPageViewModel).ChangeMusic(musicInfo);
            }
        }

        private async void MusicMoreButton_OnClicked(object sender, EventArgs e)
        {
            var musicInfo = (sender as BindableObject).BindingContext;
            var _mainMenuCellInfos = new List<MenuCellInfo>()
            {
                new MenuCellInfo() {Title = localizationManager.GetString(MatoMusicConsts.LocalizationSourceName,"AddTo"), Code = "AddToPlaylist", Icon = "addto"},
                new MenuCellInfo() {Title = localizationManager.GetString(MatoMusicConsts.LocalizationSourceName,"PlayNext"), Code = "NextPlay", Icon = "playnext"},
                new MenuCellInfo() {Title = localizationManager.GetString(MatoMusicConsts.LocalizationSourceName,"AddToQueue2"), Code = "AddToQueue", Icon = "addtostack"},
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

          await  navigationService.PushAsync(_musicFunctionPage);

        }

        private void _musicFunctionPage_OnFinished(object sender, MusicFunctionEventArgs e)
        {
            musicFunctionManager.OnMusicFunctionFinished(e);
        }

        private async void AlbumMoreButton_OnClicked(object sender, EventArgs e)
        {

            var _mainMenuCellInfos = new List<MenuCellInfo>()
            {

                new MenuCellInfo() {Title = string.Format("{0}{1}",localizationManager.GetString(MatoMusicConsts.LocalizationSourceName,"PlayThis"),localizationManager.GetString(MatoMusicConsts.LocalizationSourceName,"Albums")), Code = "Play", Icon = "cdplay"},
                new MenuCellInfo() {Title = localizationManager.GetString(MatoMusicConsts.LocalizationSourceName,"AddToQueue2"), Code = "AddMusicCollectionToQueue", Icon = "addtostack"},
                new MenuCellInfo() {Title = localizationManager.GetString(MatoMusicConsts.LocalizationSourceName,"AddTo"), Code = "AddMusicCollectionToPlaylist", Icon = "addto"},
                new MenuCellInfo() {Title = localizationManager.GetString(MatoMusicConsts.LocalizationSourceName,"AddToFavourite"), Code = "AddToFavourite", Icon = "favouriteadd"}

            };
            var musicInfo = (sender as BindableObject).BindingContext;

            var _musicFunctionPage = new MusicFunctionPage(musicInfo as IBasicInfo, _mainMenuCellInfos);
            _musicFunctionPage.OnFinished += _musicFunctionPage_OnFinished;

           await navigationService.PushAsync(_musicFunctionPage);

        }

        private async void ArtistMoreButton_OnClicked(object sender, EventArgs e)
        {
            var _mainMenuCellInfos = new List<MenuCellInfo>()
            {

                new MenuCellInfo() {Title = string.Format("{0}{1}",localizationManager.GetString(MatoMusicConsts.LocalizationSourceName,"PlayThis"),localizationManager.GetString(MatoMusicConsts.LocalizationSourceName,"Artists")), Code = "Play", Icon = "cdplay"},
                new MenuCellInfo() {Title = localizationManager.GetString(MatoMusicConsts.LocalizationSourceName,"AddToQueue2"), Code = "AddMusicCollectionToQueue", Icon = "addtostack"},
                new MenuCellInfo() {Title = localizationManager.GetString(MatoMusicConsts.LocalizationSourceName,"AddTo"), Code = "AddMusicCollectionToPlaylist", Icon = "addto"},
                new MenuCellInfo() {Title = localizationManager.GetString(MatoMusicConsts.LocalizationSourceName,"AddToFavourite"), Code = "AddToFavourite", Icon = "favouriteadd"}

            };
            var musicInfo = (sender as BindableObject).BindingContext;

            var _musicFunctionPage = new MusicFunctionPage(musicInfo as IBasicInfo, _mainMenuCellInfos);
            _musicFunctionPage.OnFinished += _musicFunctionPage_OnFinished;

           await navigationService.PushAsync(_musicFunctionPage);
        }

        private void SearchButton_OnClicked(object sender, EventArgs e)
        {
            navigationService.GoNavigate("SearchPage");
        }

        private void MusicButton_OnClicked(object sender, EventArgs e)
        {
            MusicLayout.IsVisible = true;
            ArtistLayout.IsVisible = false;
            AlbumLayout.IsVisible = false;
        }

        private void ArtistButton_OnClicked(object sender, EventArgs e)
        {
            MusicLayout.IsVisible = false;
            ArtistLayout.IsVisible = true;
            AlbumLayout.IsVisible = false;

        }

        private void AlbumButton_OnClicked(object sender, EventArgs e)
        {
            MusicLayout.IsVisible = false;
            ArtistLayout.IsVisible = false;
            AlbumLayout.IsVisible = true;

        }

        private void Init()
        {
            MusicLayout.IsVisible = true;
            ArtistLayout.IsVisible = false;
            AlbumLayout.IsVisible = false;
        }
    }

}


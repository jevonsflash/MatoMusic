using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public partial class PlaylistPage : ContentPage, ITransientDependency
    {
        private readonly ILocalizationManager localizationManager;
        private readonly NavigationService navigationService;
        private readonly MusicFunctionManager musicFunctionManager;

        public PlaylistPage(
            PlaylistPageViewModel playlistPageViewModel,
            ILocalizationManager localizationManager,
            NavigationService navigationService,
            MusicFunctionManager musicFunctionManager
            )
        {
            this.localizationManager = localizationManager;
            this.navigationService = navigationService;
            this.musicFunctionManager = musicFunctionManager;
            InitializeComponent();
            this.BindingContext = playlistPageViewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await (this.BindingContext as PlaylistPageViewModel).Init();
        }

        private void ListView_OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var playlist = e.SelectedItem as PlaylistInfo;
            if (playlist != null)
            {
                navigationService.GoNavigate("PlaylistEntryPage", new object[] { playlist });
                (sender as ListView).SelectedItem = null;


            }
        }

        private async Task<bool> OnFinishedChoice(MusicFunctionEventArgs e)
        {
            var result = false;
            var musicCollectionInfo = e.MusicInfo;
            var playlistViewModel = this.BindingContext as PlaylistPageViewModel;

            if (e.MenuCellInfo.Code == "Delete")
            {
                var isConfirmed = false;
                if (playlistViewModel != null)
                {
                    await navigationService.PopToRootAsync();
                    if ((musicCollectionInfo as MusicCollectionInfo).Count == 0)
                    {
                        isConfirmed = true;
                    }
                    else
                    {
                        isConfirmed = await DisplayAlert(
                           "提醒",
                           string.Format("此歌单包含的曲目都将移除，你确认删除 \n{0}吗？\n", musicCollectionInfo.Title),
                           "删除",
                           "不");

                    }

                    if (isConfirmed)
                    {
                        playlistViewModel.DeleteAction(musicCollectionInfo);

                    }

                }
                result = true;
            }
            else if (e.MenuCellInfo.Code == "Rename")
            {
                if (playlistViewModel != null)
                {
                    if (musicCollectionInfo.Title == "我最喜爱")
                    {
                        CommonHelper.ShowMsg(string.Format("无法编辑 {0}", musicCollectionInfo.Title));
                        return true;
                    }
                    await navigationService.PopToRootAsync();

                    var currentPlaylist = playlistViewModel.Playlists.FirstOrDefault(c => c.Id == musicCollectionInfo.Id);

                    currentPlaylist.Title = await CommonHelper.PromptAsync("编辑", currentPlaylist.Title);
                    var playlistPageViewModel = this.BindingContext as PlaylistPageViewModel;
                    playlistPageViewModel.EditAction(currentPlaylist);

                }
                result = true;


            }

            return result;
        }


        private async void CreatePlaylist_OnClicked(object sender, EventArgs e)
        {
            var currentPlaylist = new PlaylistInfo() { IsHidden = false, IsRemovable = true, Title = localizationManager.GetString(MatoMusicConsts.LocalizationSourceName, "NewPlaylist") };
            currentPlaylist.Title = await CommonHelper.PromptAsync("新建歌单");
            var playlistPageViewModel = this.BindingContext as PlaylistPageViewModel;
            playlistPageViewModel.CreateAction(currentPlaylist);
        }


        private async void _musicFunctionPage_OnFinished(object sender, MusicFunctionEventArgs e)
        {
            var isHandled = await OnFinishedChoice(e);
            if (!isHandled)
            {
                musicFunctionManager.OnMusicFunctionFinished(e);

            }
        }

        private async void AlbumMoreButton_OnClicked(object sender, EventArgs e)
        {

            var _mainMenuCellInfos = new List<MenuCellInfo>()
            {

                new MenuCellInfo() {Title = localizationManager.GetString(MatoMusicConsts.LocalizationSourceName, "Remove"), Code = "Delete", Icon = "remove"},
                new MenuCellInfo() {Title = localizationManager.GetString(MatoMusicConsts.LocalizationSourceName, "Rename"), Code = "Rename", Icon = "rename"},
                new MenuCellInfo() {Title = string.Format("{0}{1}",localizationManager.GetString(MatoMusicConsts.LocalizationSourceName, "PlayThis"),localizationManager.GetString(MatoMusicConsts.LocalizationSourceName, "Albums")), Code = "Play", Icon = "cdplay"},
                new MenuCellInfo() {Title = localizationManager.GetString(MatoMusicConsts.LocalizationSourceName, "AddToQueue2"), Code = "AddMusicCollectionToQueue", Icon = "addtostack"},
                new MenuCellInfo() {Title = localizationManager.GetString(MatoMusicConsts.LocalizationSourceName, "AddTo"), Code = "AddMusicCollectionToPlaylist", Icon = "addto"},
                new MenuCellInfo() {Title = localizationManager.GetString(MatoMusicConsts.LocalizationSourceName, "AddToFavourite"), Code = "AddToFavourite", Icon = "favouriteadd"}

            };
            var musicInfo = (sender as BindableObject).BindingContext;

            var _musicFunctionPage = new MusicFunctionPage(musicInfo as IBasicInfo, _mainMenuCellInfos);
            _musicFunctionPage.OnFinished += _musicFunctionPage_OnFinished;

            await navigationService.PushAsync(_musicFunctionPage);

        }

    }
}

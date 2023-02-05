using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Abp.Dependency;
using MatoMusic.Common;
using MatoMusic.Core;
using MatoMusic.Core.Helper;
using MatoMusic.Core.Interfaces;
using MatoMusic.Core.Models;
using MatoMusic.Core.Services;
using MatoMusic.Core.ViewModel;
using MatoMusic.Infrastructure.Common;
using MatoMusic.Services;
using MatoMusic.ViewModel;


namespace MatoMusic
{


    public partial class PlaylistEntryPage : ContentPageBase, ITransientDependency
    {


        public MusicFunctionManager MusicFunctionManager { get; set; }

        private PlaylistFunctionPage _editPlaylistFunctionPage;

        private string pathStr = "ms-appx:///Assets/Images/{0}.png";
        private MusicFunctionPage _musicFunctionPage;

        public PlaylistEntryPage(PlaylistInfo playlist)
        {

            InitializeComponent();

            var menus = new List<MenuCellInfo>()
                {
                    new MenuCellInfo() {Title = L("Play"), Code = "Play", Icon =string.Format(pathStr,"cdplay") },
                    new MenuCellInfo() {Title = L("AddToQueue2"), Code = "AddMusicCollectionToQueue", Icon = string.Format(pathStr,"addtostack")},
                    new MenuCellInfo() {Title = "编辑歌单", Code = "Edit", Icon = ""},
                };
            using (var source =
                  IocManager.Instance.ResolveAsDisposable<PlaylistEntryPageViewModel>(new { playlist, menus }))
            { 
                this.BindingContext = source.Object;

            }


        }

        private async void MusicMoreButton_OnClicked(object sender, EventArgs e)
        {
            var musicInfo = (sender as BindableObject).BindingContext;
            var _mainMenuCellInfos = new List<MenuCellInfo>()
            {
                new MenuCellInfo() {Title = L("Remove"), Code = "Delete", Icon = ""},
                new MenuCellInfo() {Title = L("AddTo"), Code = "AddToPlaylist", Icon = ""},
                new MenuCellInfo() {Title = L("PlayNext"), Code = "NextPlay", Icon = ""},
                new MenuCellInfo() {Title = L("AddToQueue2"), Code = "AddToQueue", Icon = ""},
                new MenuCellInfo()
                {
                    Title = (musicInfo as MusicInfo).Artist,
                    Code = "GoArtistPage",
                    Icon = ""
                },
                new MenuCellInfo()
                {
                    Title = (musicInfo as MusicInfo).AlbumTitle,
                    Code = "GoAlbumPage",
                    Icon = ""
                },


            };
            _musicFunctionPage = new MusicFunctionPage(musicInfo as IBasicInfo, _mainMenuCellInfos);
            _musicFunctionPage.OnFinished += _musicFunctionPage_OnFinished;

            await navigationService.ShowPopupAsync(_musicFunctionPage);

        }

        private void _musicFunctionPage_OnFinished(object sender, MusicFunctionEventArgs e)
        {
            if (e.MenuCellInfo.Code == "Delete")
            {
                var playlistEntryViewModel = this.BindingContext as PlaylistEntryPageViewModel;
                if (playlistEntryViewModel != null)
                {
                    playlistEntryViewModel.DeleteAction(e.MusicInfo);
                }
            }
            MusicFunctionManager.OnMusicFunctionFinished(e);
        }



        private async void _editPlaylistFunctionPage_OnFinished(object sender, CommonFunctionEventArgs e)
        {

            var playlistEntryViewModel = this.BindingContext as PlaylistEntryPageViewModel;
            if (playlistEntryViewModel != null)
            {
                playlistEntryViewModel.MusicsCollectionInfo = e.Info as PlaylistInfo;
            }

            await navigationService.HidePopupAsync(this._editPlaylistFunctionPage);
        }

        private void MusicListView_OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var musicInfo = e.SelectedItem as MusicInfo;
            if (musicInfo != null)
            {
                MusicInfoManager.InsertToEndQueueEntry(musicInfo);
                MusicRelatedService.CurrentMusic=musicInfo;
                MusicControlService.Play(MusicRelatedService.CurrentMusic);

                (sender as ListView).SelectedItem = null;

            }
        }

        private async void Button_OnClicked(object sender, EventArgs e)
        {

            var context = this.BindingContext as PlaylistEntryPageViewModel;
            var MenuCellInfo = (sender as BindableObject).BindingContext as MenuCellInfo;
            if (MenuCellInfo.Code == "Play")
            {

                if (context.MusicsCollectionInfo.Musics != null && context.MusicsCollectionInfo.Musics.Count != 0)
                {


                    await MusicInfoManager.ClearQueue();
                    var result = await MusicInfoManager.CreateQueueEntrys(context.MusicsCollectionInfo.Musics.ToList());
                    if (result)
                    {
                        var CurrentMusic = await MusicInfoManager.GetQueueEntry();
                        MusicRelatedService.CurrentMusic = CurrentMusic[0];//CommonHelper.ShowMsg("成功添加并播放");
                        MusicControlService.Play(MusicRelatedService.CurrentMusic);

                    }
                    else
                    {
                        CommonHelper.ShowMsg(L("Msg_AlreadyExists"));
                    }
                }
                else
                {
                    CommonHelper.ShowMsg(L("NoItem"));

                }
            }

            else if (MenuCellInfo.Code == "AddMusicCollectionToQueue")
            {
                if (context.MusicsCollectionInfo.Musics != null && context.MusicsCollectionInfo.Musics.Count != 0)
                {

                    var result = await MusicInfoManager.InsertToEndQueueEntrys(context.MusicsCollectionInfo.Musics
                        .ToList());
                    if (result)
                    {
                        CommonHelper.ShowMsg(L("Msg_HasAddedQueue2"));
                    }
                    else
                    {
                        CommonHelper.ShowMsg(L("Msg_AlreadyExists"));
                    }
                }
                else
                {
                    CommonHelper.ShowMsg(L("NoItem"));

                }
            }
            else if (MenuCellInfo.Code == "Edit")
            {
                if (context != null && context.MusicsCollectionInfo.Title == "我最喜爱")
                {
                    CommonHelper.ShowMsg(string.Format("无法编辑 {0}", context.MusicsCollectionInfo.Title));
                    return;
                }

                _editPlaylistFunctionPage = new PlaylistFunctionPage(context.MusicsCollectionInfo as PlaylistInfo);
                _editPlaylistFunctionPage.OnFinished += _editPlaylistFunctionPage_OnFinished;
                await navigationService.ShowPopupAsync(_editPlaylistFunctionPage);
            }



        }
    }
}

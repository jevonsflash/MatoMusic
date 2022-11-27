using System;
using Abp.Dependency;
using CommunityToolkit.Maui.Views;
using MatoMusic.Common;
using MatoMusic.Core;
using MatoMusic.Core.Helper;
using MatoMusic.Core.Interfaces;
using MatoMusic.Core.Models;
using Microsoft.Maui.Controls;


namespace MatoMusic
{


    public partial class PlaylistChoosePage : PopupBase, ITransientDependency
    {
        private PlaylistFunctionPage _editPlaylistFunctionPage;
        private readonly IMusicInfoManager musicInfoManager;

        public event EventHandler<Playlist> OnFinished;
        public PlaylistChoosePage(IMusicInfoManager musicInfoManager)
        {
            this.musicInfoManager=musicInfoManager;

            InitializeComponent();
            Init();
        }

        private async void Init()
        {
            var restul = await musicInfoManager.GetPlaylistInfo();
            if (restul.Count == 0 || !restul.Any(c => c.Title == "我最喜爱"))
            {
                var model = new PlaylistInfo() { Id = 0, Title = "我最喜爱", IsHidden = false, IsRemovable = false };
                var entity = ObjectMapper.Map<Playlist>(model);
                await musicInfoManager.CreatePlaylist(entity);
            }
            this.BindingContext = await musicInfoManager.GetPlaylist();
        }

        private void ListView_OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var playlist = e.SelectedItem as Playlist;
            if (playlist != null)
            {
                this.OnFinished?.Invoke(this, playlist);

            }

        }

        private async void CreateButton_OnClicked(object sender, EventArgs e)
        {
            _editPlaylistFunctionPage = new PlaylistFunctionPage(null);
            _editPlaylistFunctionPage.OnFinished += _editPlaylistFunctionPage_OnFinished;
            await navigationService.ShowPopupAsync(_editPlaylistFunctionPage);
        }

        private async void _editPlaylistFunctionPage_OnFinished(object sender, CommonFunctionEventArgs e)
        {
            var playlistInfo = e.Info as PlaylistInfo;

            if (playlistInfo != null && playlistInfo.Title != "我最喜爱" && !string.IsNullOrEmpty(playlistInfo.Title))
            {
                var restul = await musicInfoManager.GetPlaylistInfo();
                if (!restul.Any(c => c.Title == playlistInfo.Title))
                {
                    if (e.Code == "Create")
                    {
                        var entity = ObjectMapper.Map<Playlist>(playlistInfo);

                        if (await musicInfoManager.CreatePlaylist(entity))
                        {

                            CommonHelper.ShowMsg(string.Format("{0} {1}", L("Msg_HasCreated"), playlistInfo.Title));

                        }
                        else
                        {
                            CommonHelper.ShowMsg(string.Format("{0} {1}", L("Msg_AddFailed"), playlistInfo.Title));

                        }

                    }
                    Init();
                    await navigationService.HidePopupAsync(_editPlaylistFunctionPage);
                }
                else
                {
                    CommonHelper.ShowMsg(string.Format("{0} {1}", L("Msg_AlreadyExists"), playlistInfo.Title));
                }
            }
            else
            {
                CommonHelper.ShowMsg(string.Format(L("Msg_Nameillegal")));

            }

        }



    }

}

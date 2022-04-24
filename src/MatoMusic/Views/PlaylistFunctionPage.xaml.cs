using Abp.Dependency;
using CommunityToolkit.Maui.Views;
using MatoMusic.Common;
using MatoMusic.Core.Models;
using MatoMusic.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;


namespace MatoMusic
{


    public partial class PlaylistFunctionPage : PopupBase, ITransientDependency
    {
        private bool _isEdit = false;
        public event EventHandler<CommonFunctionEventArgs> OnFinished;

        public PlaylistFunctionPage(PlaylistInfo info)
        {
            InitializeComponent();
            if (info != null)
            {
                _isEdit = true;
                this.BindingContext = new PlaylistFunctionPageViewModel(new PlaylistInfo()
                {
                     AlbumArt = info.AlbumArt,
                     AlbumArtPath = info.AlbumArtPath,
                     Artist = info.Artist,
                     GroupHeader = info.GroupHeader,
                     Id = info.Id,
                     IsHidden = info.IsHidden,
                     IsRemovable = info.IsRemovable,
                     Musics = info.Musics,
                     PlaylistArt = info.PlaylistArt,
                     Title =info.Title,
                    
                });
                this.LabelCreate.Text = L("EditPlaylist");
            }
            else
            {
                _isEdit = false;
                this.BindingContext = new PlaylistFunctionPageViewModel();
                this.LabelCreate.Text = L("AddPlaylist");
            }
        }


        private void SubmitButtonButton_OnClicked(object sender, EventArgs e)
        {
            var playlistFunctionPageViewModel = this.BindingContext as PlaylistFunctionPageViewModel;

            if (OnFinished != null && playlistFunctionPageViewModel != null)
                OnFinished(this, new CommonFunctionEventArgs(playlistFunctionPageViewModel.PlaylistInfo, _isEdit ? "Edit" : "Create"));
        }
    }

}

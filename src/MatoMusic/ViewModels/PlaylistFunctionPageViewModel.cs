using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MatoMusic.Common;
using MatoMusic.Core.Helper;
using MatoMusic.Core.Models;
using MatoMusic.Core.ViewModel;

namespace MatoMusic.ViewModel
{
    class PlaylistFunctionPageViewModel: ViewModelBase
    {
        public PlaylistFunctionPageViewModel()
        {
            this.SubmitCommand = new Command(new Action<object>(SubmitAction));
			this.PlaylistInfo = new PlaylistInfo() { IsHidden = false, IsRemovable = true, Title = L("NewPlaylist") };
        }

        public PlaylistFunctionPageViewModel(PlaylistInfo playlistInfo) : this()
        {
            this.PlaylistInfo = playlistInfo;
        }
        private PlaylistInfo _playlistInfo;
        public PlaylistInfo PlaylistInfo
        {
            get { return _playlistInfo; }
            set
            {
                _playlistInfo = value;
                base.RaisePropertyChanged();
            }
        }
        private void SubmitAction(object obj)
        {
            //MusicInfoServer.Current.CreatePlaylist(PlaylistInfo);
        }

        public Command SubmitCommand { get; set; }

    }
}

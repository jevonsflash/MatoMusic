using System;
using Abp.Dependency;
using MatoMusic.Core.Helper;
using MatoMusic.Core.Models;
using MatoMusic.ViewModels;
using Microsoft.Maui.Controls;


namespace MatoMusic
{
    public partial class MusicMiniView : ContentViewBase
    {
        public MusicMiniViewViewModel MusicMiniViewViewModel => IocManager.Instance.Resolve<MusicMiniViewViewModel>();
        public MusicMiniView()
        {
            InitializeComponent();
            BindingContext=this.MusicMiniViewViewModel;
        }

        private async void TapGestureRecognizer_OnTapped(object sender, EventArgs e)
        {
            var page = "NowPlayingPage";
            var route = $"///{page}";
            await Shell.Current.GoToAsync(route);
        }
    }
}

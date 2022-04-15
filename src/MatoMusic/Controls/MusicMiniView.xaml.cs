using System;
using MatoMusic.Core.Helper;
using MatoMusic.Core.Models;
using Microsoft.Maui.Controls;


namespace MatoMusic
{
    public partial class MusicMiniView : ContentView
    {
        public MusicMiniView()
        {
            InitializeComponent();
        }

        private async void TapGestureRecognizer_OnTapped(object sender, EventArgs e)
        {
            var page = "NowPlayingPage";
            var route = $"///{page}";
            await Shell.Current.GoToAsync(route);
        }
    }
}

using System;
using MatoMusic.Core.Helper;
using MatoMusic.Core.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Essentials;

namespace MatoMusic
{
    public partial class MusicMiniView : ContentView
    {
        public MusicMiniView()
        {
            InitializeComponent();
        }

        private void TapGestureRecognizer_OnTapped(object sender, EventArgs e)
        {
            CommonHelper.GoPage("NowPlayingPage");
        }
    }
}

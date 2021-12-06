using System;
using System.IO;
using Abp;
using Abp.Castle.Logging.Log4Net;
using Abp.Dependency;
using Castle.Facilities.Logging;
using MatoMusic.Core;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using Application = Microsoft.Maui.Controls.Application;

namespace MatoMusic
{
    public partial class App : Application
    {
        private readonly AbpBootstrapper _abpBootstrapper;

        public App(AbpBootstrapper abpBootstrapper)
        {
            _abpBootstrapper = abpBootstrapper;
            InitializeComponent();
            _abpBootstrapper.Initialize();

            this.Init();
        }

        private void Init()
        {
            var nowPlayingPage = _abpBootstrapper.IocManager.Resolve<NowPlayingPage>();
            var queuePage = _abpBootstrapper.IocManager.Resolve<QueuePage>();
            var musicPage = _abpBootstrapper.IocManager.Resolve<MusicPage>();
            var albumPage = _abpBootstrapper.IocManager.Resolve<AlbumPage>();
            var artistPage = _abpBootstrapper.IocManager.Resolve<ArtistPage>();
            var playlistPage = _abpBootstrapper.IocManager.Resolve<PlaylistPage>();
            this.NowPlayingPageShellContent.Content = nowPlayingPage;
            this.QueuePageShellContent.Content = queuePage;
            this.MusicPageShellContent.Content = musicPage;
            this.ArtistPageShellContent.Content = albumPage;
            this.AlbumPageShellContent.Content = artistPage;
            this.PlaylistPageShellContent.Content = playlistPage;
        }
    }
}

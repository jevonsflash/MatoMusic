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


namespace MatoMusic
{
    public partial class LibraryMainPage : Shell, ITransientDependency
    {
        private readonly IocManager iocManager;

        public LibraryMainPage(IocManager iocManager)
        {
            InitializeComponent();
            this.iocManager = iocManager;
            this.Init();
        }

        private void Init()
        {
            var musicPage = iocManager.Resolve<MusicPage>();
            var albumPage = iocManager.Resolve<AlbumPage>();
            var artistPage = iocManager.Resolve<ArtistPage>();

            this.MusicPageShellContent.Content = musicPage;
            this.ArtistPageShellContent.Content = albumPage;
            this.AlbumPageShellContent.Content = artistPage;
        }
    }
}


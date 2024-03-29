﻿using System;
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
            this.MainPage = abpBootstrapper.IocManager.Resolve(typeof(MainPage)) as MainPage;
        }
    }
}

using System;
using System.Collections.Generic;
using Abp.Dependency;
using MatoMusic.Common;
using MatoMusic.Core;
using MatoMusic.Core.Models;
using MatoMusic.Infrastructure.Common;
using MatoMusic.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Essentials;

namespace MatoMusic
{
    public partial class QueuePage:ContentPage, ITransientDependency
    {

        public QueuePage()
        {

            InitializeComponent();




        }
    }
}

using System;
using System.Collections.Generic;
using Abp.Dependency;
using CommunityToolkit.Maui.Views;
using MatoMusic.Common;
using MatoMusic.Core;
using MatoMusic.Core.Models;
using MatoMusic.Infrastructure.Common;
using MatoMusic.ViewModels;
using Microsoft.Maui.Controls;


namespace MatoMusic
{
    public partial class MusicFunctionPage : PopupBase, ITransientDependency
    {
        public event EventHandler<MusicFunctionEventArgs> OnFinished;

        public MusicFunctionPage()
        {

            InitializeComponent();
        }
        public MusicFunctionPage(IBasicInfo objInfo, IList<MenuCellInfo> menus)
        {
            InitializeComponent();

            this.ObjInfo = objInfo;
            this.Menus = menus;
            this.BindingContext = new MusicFunctionPageViewModel(ObjInfo, Menus);
            if (ObjInfo is MusicInfo)
            {
                //this.FavouritLayout.IsVisible = true;
            }
        }

        private IBasicInfo _objInfo;

        public IBasicInfo ObjInfo
        {
            get { return _objInfo; }
            set { _objInfo = value; }
        }


        private IList<MenuCellInfo> _menus;

        public IList<MenuCellInfo> Menus
        {
            get { return _menus; }
            set { _menus = value; }
        }


        private void ListView_OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {

            OnFinished?.Invoke(this, new MusicFunctionEventArgs(_objInfo, e.SelectedItem as MenuCellInfo));
            this.Close();
        }

    }
}

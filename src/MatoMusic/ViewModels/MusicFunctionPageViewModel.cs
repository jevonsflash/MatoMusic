using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MatoMusic.Core;
using MatoMusic.Core.Helper;
using MatoMusic.Core.Models;
using MatoMusic.Core.ViewModel;
using MatoMusic.Infrastructure.Common;
using Microsoft.Maui.Controls;

namespace MatoMusic.ViewModels
{
    public class MusicFunctionPageViewModel : ViewModelBase
    {
        public MusicFunctionPageViewModel(IBasicInfo musicInfo, IList<MenuCellInfo> mainMenuCellInfos)
        {
            this.CurrentInfo = musicInfo;
            this.MainMenuCellInfos = mainMenuCellInfos;
            this.PropertyChanged += MusicFunctionPageViewModel_PropertyChanged;
            this.FavouriteCommand = new Command(FavouriteAction,CanFavouriteAction);
        }

        private bool CanFavouriteAction(object obj)
        {
            return CurrentInfo is MusicInfo;
        }

        private void FavouriteAction(object obj)
        {
            var musicInfo = CurrentInfo as MusicInfo;
            if (musicInfo != null)
            {
                musicInfo.IsFavourite = !musicInfo.IsFavourite;
                if (musicInfo.IsFavourite)
                {
                    CommonHelper.ShowMsg("我最喜爱");

                }
            }
        }

        private void MusicFunctionPageViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CurrentMenuCellInfo))
            {
                //
            }
        }

        private IBasicInfo _musicInfo;
        public IBasicInfo CurrentInfo
        {
            get { return _musicInfo; }
            set
            {
                _musicInfo = value;
                base.RaisePropertyChanged();
            }
        }


        private IList<MenuCellInfo> _mainMenuCellInfos;

        public IList<MenuCellInfo> MainMenuCellInfos
        {
            get
            {
                if (_mainMenuCellInfos == null)
                {
                    _mainMenuCellInfos = new List<MenuCellInfo>();
                }
                return _mainMenuCellInfos;
            }
            set
            {
                _mainMenuCellInfos = value;
                base.RaisePropertyChanged();


            }
        }

        private MenuCellInfo _currentMenuCellInfo;

        public MenuCellInfo CurrentMenuCellInfo
        {
            get
            {
                if (_currentMenuCellInfo == null)
                {
                    //_currentMenuCellInfo = MainMenuCellInfos[1];
                }
                return _currentMenuCellInfo;
            }
            set
            {
                _currentMenuCellInfo = value;
                base.RaisePropertyChanged();
            }
        }
        public Command FavouriteCommand { get; set; }

    }
}

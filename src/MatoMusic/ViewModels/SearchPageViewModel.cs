using MatoMusic.Core;
using MatoMusic.Core.Helper;
using MatoMusic.Core.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ProjectMato.ViewModel
{
    public class SearchPageViewModel : MusicRelatedViewModel
    {


        public SearchPageViewModel()
        {
            this.ItemSelectedCommand = new Command(ItemSelectedAction);
        }

        private void ItemSelectedAction(object obj)
        {


        }

      
        private MusicInfo _selectedItem;

        public MusicInfo SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                _selectedItem = value;
                base.RaisePropertyChanged();
            }
        }
       
        public Command ItemSelectedCommand { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MatoMusic.Core.ViewModel;

namespace MatoMusic.Core.Services
{
    public class MusicRelatedService : MusicRelatedViewModel
    {
        public MusicRelatedService(IMusicInfoManager musicInfoManager) : base(musicInfoManager)
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace MatoMusic.Core.Helper
{
    public class CommonHelper
    {
        public static void ShowMsg(string msg)
        {

            Application.Current.MainPage.DisplayAlert("提示", msg, "好");
        }

        public static void ShowNoAuthorized()
        {
            Application.Current.MainPage.DisplayAlert("需要权限", "MatoPlayer需要您媒体库的权限，劳烦至「设置」「隐私权」「媒体与AppleMusic」 打开权限,谢谢", "好");
        }
    }

}

using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public static void GoNavigate(string pageName, object[] args = null)
        {
            var page = GetPageInstance(pageName, args);
            Application.Current.MainPage.Navigation.PushAsync(page);
        }

        public static void GoPage(string obj)
        {
            GetPageInstance(obj, null);
        }

        private static Page GetPageInstance(string obj, object[] args, IList<ToolbarItem> barItem = null)
        {
            Page result = null;
            var namespacestr = "MatoMusic";
            Type pageType = Type.GetType(namespacestr + ".View." + obj, false);
            if (pageType != null)
            {
                try
                {
                    var pageObj = Activator.CreateInstance(pageType, args) as Page;

                    if (barItem != null && barItem.Count > 0)
                    {
                        foreach (var toolbarItem in barItem)
                        {
                            pageObj.ToolbarItems.Add(toolbarItem);

                        }
                    }

                    result = pageObj;

                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);

                }
            }
            return result;
        }

        public static void GoUrl(object obj)
        {
            throw new NotImplementedException();
        }
    }



}

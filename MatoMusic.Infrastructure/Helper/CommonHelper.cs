using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acr.UserDialogs;

namespace MatoMusic.Infrastructure.Helper
{
    public class CommonHelper
    {
        public static int[] GetRandomArry(int minval, int maxval)
        {

            int[] arr = new int[maxval - minval + 1];
            int i;
            //初始化数组
            for (i = 0; i <= maxval - minval; i++)
            {
                arr[i] = i + minval;
            }
            //随机数
            Random r = new Random();
            for (int j = maxval - minval; j >= 1; j--)
            {
                int address = r.Next(0, j);
                int tmp = arr[address];
                arr[address] = arr[j];
                arr[j] = tmp;
            }
            //输出
            foreach (int k in arr)
            {
                Debug.WriteLine(k + " ");
            }
            return arr;
        }
        public static void ShowMsg(string msg)
        {
            UserDialogs.Instance.Alert(msg);

        }

        public static void ShowNoAuthorized()
        {
            UserDialogs.Instance.Alert("MatoPlayer需要您媒体库的权限，劳烦至「设置」「隐私权」「媒体与AppleMusic」 打开权限,谢谢", "需要权限");
        }

    }
}

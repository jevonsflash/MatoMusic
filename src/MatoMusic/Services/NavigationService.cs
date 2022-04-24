using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp;
using Abp.Dependency;
using Abp.Domain.Services;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;

namespace MatoMusic.Services
{
    public class NavigationService : AbpServiceBase, ISingletonDependency
    {
        private readonly IIocManager iocManager;

        private INavigation mainPageNavigation => mainShell.Navigation;
        private Shell mainShell => Shell.Current;

        public NavigationService(
            IIocManager iocManager
            )
        {
            this.iocManager = iocManager;
        }

        public void GoNavigate(string pageName, object[] args = null)
        {
            var page = GetPageInstance(pageName, args);
            mainPageNavigation.PushAsync(page);
        }

        public async Task GoNavigateAsync(string pageName, object[] args = null)
        {
            var page = GetPageInstance(pageName, args);
            await mainPageNavigation.PushAsync(page);
        }

        public async Task PopAsync()
        {
            await mainPageNavigation.PopAsync();
        }

        public async Task PopToRootAsync()
        {
            await mainPageNavigation.PopToRootAsync();
        }

        public async Task PushAsync(Page page)
        {
            await mainPageNavigation.PushAsync(page);
        }

        public async Task PushModalAsync(Page page)
        {
            await mainPageNavigation.PushModalAsync(page);
        }

        public async Task GoPageAsync(string obj)
        {
            var route = $"///{obj}";
            await mainShell.GoToAsync(route);
        }

        public async Task ShowPopupAsync(Popup popupPage)
        {
           await Shell.Current.CurrentPage.ShowPopupAsync(popupPage);
        }
        public async Task HidePopupAsync(Popup popupPage)
        {
           popupPage.Close();
        }
        private Page GetPageInstance(string obj, object[] args, IList<ToolbarItem> barItem = null)
        {
            Page result = null;
            var namespacestr = "MatoMusic";
            Type pageType = Type.GetType(namespacestr + "." + obj, false);
            if (pageType != null)
            {
                try
                {
                    var pageObj = iocManager.Resolve(pageType) as Page;

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

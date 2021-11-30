using System;
using System.Reflection;
using System.Resources;
using Abp.Domain.Services;
using Abp.Localization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace ProjectMato
{
    // You exclude the 'Extension' suffix when using in Xaml markup
    [ContentProperty("Text")]
    public class TranslateExtension : DomainService, IMarkupExtension
    {
        const string ResourceId = "ProjectMato.Resx.AppResources";


        public string Text { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Text == null)
                return "";

            ResourceManager temp = new ResourceManager(ResourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);
            var translation = L(Text);
            if (translation == null)
            {
#if DEBUG
                throw new ArgumentException(
                    String.Format("Key '{0}' was not found in resources '{1}'", Text, ResourceId),
                    "Text");
#else
				translation = Text; // HACK: returns the key, which GETS DISPLAYED TO THE USER
#endif
            }
            return translation;
        }



    }
}

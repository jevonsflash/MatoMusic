using System;
using MatoMusic.Core.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Essentials;

namespace MatoMusic
{
    public partial class MenuCell : ViewCell
    {
        public MenuCell()
        {
            InitializeComponent();
        }


        public static readonly BindableProperty ContentTextProperty =
            BindableProperty.Create(nameof(ContentText),
                typeof(string), typeof(MenuCell),
                string.Empty, propertyChanged: ContentTextPropertyChanged);

        private static void ContentTextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as MenuCell).LabelContentText.Text = newValue as string;
        }

        public string ContentText
        {
            get { return (string)GetValue(ContentTextProperty); }
            set { SetValue(ContentTextProperty, value); }
        }

        public static readonly BindableProperty IconTextProperty =
            BindableProperty.Create(nameof(IconText),
                typeof(string), typeof(MenuCell),
                string.Empty, propertyChanged: IconTextPropertyChanged);

        private static void IconTextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as MenuCell).LabelIconText.Text = newValue as string;
        }

        public string IconText
        {
            get { return (string)GetValue(IconTextProperty); }
            set { SetValue(IconTextProperty, value); }
        }
    }
}

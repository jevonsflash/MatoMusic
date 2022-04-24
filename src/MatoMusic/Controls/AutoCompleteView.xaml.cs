using MatoMusic.Infrastructure.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MatoMusic
{
    public partial class AutoCompleteView : ContentView
    {
        /// <summary>
        /// The execute on suggestion click property.
        /// </summary>
        public static readonly BindableProperty ExecuteOnSuggestionClickProperty = BindableProperty.Create(nameof(ExecuteOnSuggestionClick), typeof(bool), typeof(AutoCompleteView), false);

        /// <summary>
        /// The placeholder property.
        /// </summary>
        public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(AutoCompleteView), string.Empty, propertyChanged: PlaceHolderChanged);

        /// <summary>
        /// The search background color property.
        /// </summary>
        public static readonly BindableProperty SearchBackgroundColorProperty = BindableProperty.Create(nameof(SearchBackgroundColor), typeof(Color), typeof(AutoCompleteView), Colors.Red, BindingMode.TwoWay, null, SearchBackgroundColorChanged);

        /// <summary>
        /// The search border color property.
        /// </summary>
        public static readonly BindableProperty SearchBorderColorProperty = BindableProperty.Create(nameof(SearchBorderColor), typeof(Color), typeof(AutoCompleteView), Colors.White, BindingMode.TwoWay, null, SearchBorderColorChanged);


        /// <summary>
        /// The search border width property.
        /// </summary>
        public static readonly BindableProperty SearchBorderWidthProperty = BindableProperty.Create(nameof(SearchBorderWidth), typeof(int), typeof(AutoCompleteView), 1, BindingMode.TwoWay, null, SearchBorderWidthChanged);

        /// <summary>
        /// The search command property.
        /// </summary>
        public static readonly BindableProperty SearchCommandProperty = BindableProperty.Create(nameof(SearchCommand), typeof(ICommand), typeof(AutoCompleteView));

        /// <summary>
        /// The search horizontal options property
        /// </summary>
        public static readonly BindableProperty SearchHorizontalOptionsProperty = BindableProperty.Create(nameof(SearchHorizontalOptions), typeof(LayoutOptions), typeof(AutoCompleteView), LayoutOptions.FillAndExpand, BindingMode.TwoWay, null, SearchHorizontalOptionsChanged);

        /// <summary>
        /// The search text color property.
        /// </summary>
        public static readonly BindableProperty SearchTextColorProperty = BindableProperty.Create(nameof(SearchTextColor), typeof(Color), typeof(AutoCompleteView), Colors.Red, BindingMode.TwoWay, null, SearchTextColorChanged);

        /// <summary>
        /// The search text property.
        /// </summary>
        public static readonly BindableProperty SearchTextProperty = BindableProperty.Create(nameof(SearchText), typeof(string), typeof(AutoCompleteView), "Search", BindingMode.TwoWay, null, SearchTextChanged);

        /// <summary>
        /// The search vertical options property
        /// </summary>
        public static readonly BindableProperty SearchVerticalOptionsProperty = BindableProperty.Create(nameof(SearchVerticalOptions), typeof(LayoutOptions), typeof(AutoCompleteView), LayoutOptions.Center, BindingMode.TwoWay, null, SearchVerticalOptionsChanged);

        /// <summary>
        /// The selected command property.
        /// </summary>
        public static readonly BindableProperty SelectedCommandProperty = BindableProperty.Create(nameof(SelectedCommand), typeof(ICommand), typeof(AutoCompleteView));

        /// <summary>
        /// The selected item property.
        /// </summary>
        public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(AutoCompleteView), null, BindingMode.TwoWay);

        /// <summary>
        /// The show search property.
        /// </summary>
        public static readonly BindableProperty ShowSearchProperty = BindableProperty.Create(nameof(ShowSearchButton), typeof(bool), typeof(AutoCompleteView), true, BindingMode.TwoWay, null, ShowSearchChanged);

        /// <summary>
        /// The suggestion background color property.
        /// </summary>
        public static readonly BindableProperty SuggestionBackgroundColorProperty = BindableProperty.Create(nameof(SuggestionBackgroundColor), typeof(Color), typeof(AutoCompleteView), Colors.Red, BindingMode.TwoWay, null, SuggestionBackgroundColorChanged);

        /// <summary>
        /// The suggestion item data template property.
        /// </summary>
        public static readonly BindableProperty SuggestionItemDataTemplateProperty = BindableProperty.Create(nameof(SuggestionItemDataTemplate), typeof(DataTemplate), typeof(AutoCompleteView), null, BindingMode.TwoWay, null, SuggestionItemDataTemplateChanged);

        /// <summary>
        /// The suggestion height request property.
        /// </summary>
        public static readonly BindableProperty SuggestionsHeightRequestProperty = BindableProperty.Create(nameof(SuggestionsHeightRequest), typeof(double), typeof(AutoCompleteView), 250, BindingMode.TwoWay, null, SuggestionHeightRequestChanged);

        /// <summary>
        /// The suggestions property.
        /// </summary>
        public static readonly BindableProperty SuggestionsProperty = BindableProperty.Create(nameof(Suggestions), typeof(IEnumerable), typeof(AutoCompleteView));

        /// <summary>
        /// The text background color property.
        /// </summary>
        public static readonly BindableProperty TextBackgroundColorProperty = BindableProperty.Create(nameof(TextBackgroundColor), typeof(Color), typeof(AutoCompleteView), Colors.Transparent, BindingMode.TwoWay, null, TextBackgroundColorChanged);

        /// <summary>
        /// The text color property.
        /// </summary>
        public static readonly BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextBackgroundColor), typeof(Color), typeof(AutoCompleteView), Colors.Black, BindingMode.TwoWay, null, TextColorChanged);

        /// <summary>
        /// The text horizontal options property
        /// </summary>
        public static readonly BindableProperty TextHorizontalOptionsProperty = BindableProperty.Create(nameof(TextHorizontalOptions), typeof(LayoutOptions), typeof(AutoCompleteView), LayoutOptions.FillAndExpand, BindingMode.TwoWay, null, TextHorizontalOptionsChanged);

        /// <summary>
        /// The text property.
        /// </summary>
        public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(AutoCompleteView), string.Empty, BindingMode.TwoWay, null, TextValueChanged);

        /// <summary>
        /// The text vertical options property.
        /// </summary>
        public static readonly BindableProperty TextVerticalOptionsProperty =
            BindableProperty.Create(nameof(TextVerticalOptions), typeof(LayoutOptions), typeof(AutoCompleteView), LayoutOptions.Start, BindingMode.TwoWay, null, propertyChanged: TestVerticalOptionsChanged);

        public static readonly BindableProperty DisplayPathProperty =
            BindableProperty.Create(nameof(DisplayPath), typeof(string), typeof(AutoCompleteView), string.Empty);

        private readonly ObservableCollection<IClueObject> _availableSuggestions;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoCompleteView"/> class.
        /// </summary>
        public AutoCompleteView()
        {
            InitializeComponent();
            _availableSuggestions = new ObservableCollection<IClueObject>();

            EntText.HorizontalOptions = TextHorizontalOptions;
            EntText.VerticalOptions = TextVerticalOptions;
            EntText.TextColor = TextColor;
            EntText.BackgroundColor = TextBackgroundColor;


            BtnSearch.VerticalOptions = SearchVerticalOptions;
            BtnSearch.HorizontalOptions = SearchHorizontalOptions;
            BtnSearch.Text = SearchText;



            LstSuggestions.HeightRequest = SuggestionsHeightRequest;
            LstSuggestions.HasUnevenRows = true;
            LstSuggestions.ItemsSource = _availableSuggestions;


            ShowHideListbox(false);
        }

        /// <summary>
        /// Occurs when [selected item changed].
        /// </summary>
        public event EventHandler<SelectedItemChangedEventArgs> SelectedItemChanged;

        /// <summary>
        /// Occurs when [text changed].
        /// </summary>
        public event EventHandler<TextChangedEventArgs> TextChanged;

        /// <summary>
        /// Gets the available Suggestions.
        /// </summary>
        /// <value>The available Suggestions.</value>
        public IEnumerable AvailableSuggestions
        {
            get { return _availableSuggestions; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [execute on sugestion click].
        /// </summary>
        /// <value><c>true</c> if [execute on sugestion click]; otherwise, <c>false</c>.</value>
        public bool ExecuteOnSuggestionClick
        {
            get { return (bool)GetValue(ExecuteOnSuggestionClickProperty); }
            set { SetValue(ExecuteOnSuggestionClickProperty, value); }
        }

        /// <summary>
        /// Gets or sets the placeholder.
        /// </summary>
        /// <value>The placeholder.</value>
        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        /// <summary>
        /// Gets or sets the color of the search background.
        /// </summary>
        /// <value>The color of the search background.</value>
        public Color SearchBackgroundColor
        {
            get { return (Color)GetValue(SearchBackgroundColorProperty); }
            set { SetValue(SearchBackgroundColorProperty, value); }
        }

        /// <summary>
        /// Gets or sets the search border color.
        /// </summary>
        /// <value>The search border brush.</value>
        public Color SearchBorderColor
        {
            get { return (Color)GetValue(SearchBorderColorProperty); }
            set { SetValue(SearchBorderColorProperty, value); }
        }


        /// <summary>
        /// Gets or sets the width of the search border.
        /// </summary>
        /// <value>The width of the search border.</value>
        public int SearchBorderWidth
        {
            get { return (int)GetValue(SearchBorderWidthProperty); }
            set { SetValue(SearchBorderWidthProperty, value); }
        }

        /// <summary>
        /// Gets or sets the search command.
        /// </summary>
        /// <value>The search command.</value>
        public ICommand SearchCommand
        {
            get { return (ICommand)GetValue(SearchCommandProperty); }
            set { SetValue(SearchCommandProperty, value); }
        }

        /// <summary>
        /// Gets or sets the search horizontal options.
        /// </summary>
        /// <value>The search horizontal options.</value>
        public LayoutOptions SearchHorizontalOptions
        {
            get { return (LayoutOptions)GetValue(SearchHorizontalOptionsProperty); }
            set { SetValue(SearchHorizontalOptionsProperty, value); }
        }

        /// <summary>
        /// Gets or sets the search text.
        /// </summary>
        /// <value>The search text.</value>
        public string SearchText
        {
            get { return (string)GetValue(SearchTextProperty); }
            set { SetValue(SearchTextProperty, value); }
        }

        /// <summary>
        /// Gets or sets the color of the search text button.
        /// </summary>
        /// <value>The color of the search text.</value>
        public Color SearchTextColor
        {
            get { return (Color)GetValue(SearchTextColorProperty); }
            set { SetValue(SearchTextColorProperty, value); }
        }

        /// <summary>
        /// Gets or sets the search vertical options.
        /// </summary>
        /// <value>The search vertical options.</value>
        public LayoutOptions SearchVerticalOptions
        {
            get { return (LayoutOptions)GetValue(SearchVerticalOptionsProperty); }
            set { SetValue(SearchVerticalOptionsProperty, value); }
        }


        /// <summary>
        /// Gets or sets the selected command.
        /// </summary>
        /// <value>The selected command.</value>
        public ICommand SelectedCommand
        {
            get { return (ICommand)GetValue(SelectedCommandProperty); }
            set { SetValue(SelectedCommandProperty, value); }
        }

        /// <summary>
        /// Gets or sets the selected item.
        /// </summary>
        /// <value>The selected item.</value>
        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show search button].
        /// </summary>
        /// <value><c>true</c> if [show search button]; otherwise, <c>false</c>.</value>
        public bool ShowSearchButton
        {
            get { return (bool)GetValue(ShowSearchProperty); }
            set { SetValue(ShowSearchProperty, value); }
        }

        /// <summary>
        /// Gets or sets the color of the sugestion background.
        /// </summary>
        /// <value>The color of the sugestion background.</value>
        public Color SuggestionBackgroundColor
        {
            get { return (Color)GetValue(SuggestionBackgroundColorProperty); }
            set { SetValue(SuggestionBackgroundColorProperty, value); }
        }

        /// <summary>
        /// Gets or sets the suggestion item data template.
        /// </summary>
        /// <value>The sugestion item data template.</value>
        public DataTemplate SuggestionItemDataTemplate
        {
            get { return (DataTemplate)GetValue(SuggestionItemDataTemplateProperty); }
            set { SetValue(SuggestionItemDataTemplateProperty, value); }
        }

        /// <summary>
        /// Gets or sets the Suggestions.
        /// </summary>
        /// <value>The Suggestions.</value>
        public IEnumerable Suggestions
        {
            get { return (IEnumerable)GetValue(SuggestionsProperty); }
            set { SetValue(SuggestionsProperty, value); }
        }

        /// <summary>
        /// Gets or sets the height of the suggestion.
        /// </summary>
        /// <value>The height of the suggestion.</value>
        public double SuggestionsHeightRequest
        {
            get { return (double)GetValue(SuggestionsHeightRequestProperty); }
            set { SetValue(SuggestionsHeightRequestProperty, value); }
        }
        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>The text.</value>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>
        /// Gets or sets the color of the text background.
        /// </summary>
        /// <value>The color of the text background.</value>
        public Color TextBackgroundColor
        {
            get { return (Color)GetValue(TextBackgroundColorProperty); }
            set { SetValue(TextBackgroundColorProperty, value); }
        }

        /// <summary>
        /// Gets or sets the color of the text.
        /// </summary>
        /// <value>The color of the text.</value>
        public Color TextColor
        {
            get { return (Color)GetValue(TextColorProperty); }
            set { SetValue(TextColorProperty, value); }
        }

        /// <summary>
        /// Gets or sets the text horizontal options.
        /// </summary>
        /// <value>The text horizontal options.</value>
        public LayoutOptions TextHorizontalOptions
        {
            get { return (LayoutOptions)GetValue(TextHorizontalOptionsProperty); }
            set { SetValue(TextHorizontalOptionsProperty, value); }
        }

        /// <summary>
        /// Gets or sets the text vertical options.
        /// </summary>
        /// <value>The text vertical options.</value>
        public LayoutOptions TextVerticalOptions
        {
            get { return (LayoutOptions)GetValue(TextVerticalOptionsProperty); }
            set { SetValue(TextVerticalOptionsProperty, value); }
        }


        public string DisplayPath
        {
            get { return (string)GetValue(DisplayPathProperty); }
            set { SetValue(DisplayPathProperty, value); }
        }
        /// <summary>
        /// Places the holder changed.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="oldPlaceHolderValue">The old place holder value.</param>
        /// <param name="newPlaceHolderValue">The new place holder value.</param>
        private static void PlaceHolderChanged(BindableObject obj, object oldPlaceHolderValue, object newPlaceHolderValue)
        {
            var autoCompleteView = obj as AutoCompleteView;
            if (autoCompleteView != null)
            {
                autoCompleteView.EntText.Placeholder = (string)newPlaceHolderValue;
            }
        }

        /// <summary>
        /// Searches the background color changed.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        private static void SearchBackgroundColorChanged(BindableObject obj, object oldValue, object newValue)
        {
            var autoCompleteView = obj as AutoCompleteView;
            if (autoCompleteView != null)
            {
                autoCompleteView.StkBase.BackgroundColor = (Color)newValue;
            }
        }

        /// <summary>
        /// Searches the border color changed.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        private static void SearchBorderColorChanged(BindableObject obj, object oldValue, object newValue)
        {
            var autoCompleteView = obj as AutoCompleteView;
            if (autoCompleteView != null)
            {
                autoCompleteView.BtnSearch.BorderColor = (Color)newValue;
            }
        }

        /// <summary>
        /// Searches the border width changed.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        private static void SearchBorderWidthChanged(BindableObject obj, object oldValue, object newValue)
        {
            var autoCompleteView = obj as AutoCompleteView;
            if (autoCompleteView != null)
            {
                autoCompleteView.BtnSearch.BorderWidth = (int)newValue;
            }
        }

        /// <summary>
        /// Searches the horizontal options changed.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        private static void SearchHorizontalOptionsChanged(BindableObject obj, object oldValue, object newValue)
        {
            var autoCompleteView = obj as AutoCompleteView;
            if (autoCompleteView != null)
            {
                autoCompleteView.BtnSearch.HorizontalOptions = (LayoutOptions)newValue;
            }
        }

        /// <summary>
        /// Searches the text changed.
        /// </summary>
        /// <param name="obj">The bindable.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        private static void SearchTextChanged(BindableObject obj, object oldValue, object newValue)
        {
            var autoCompleteView = obj as AutoCompleteView;
            if (autoCompleteView != null)
            {
                autoCompleteView.BtnSearch.Text = (string)newValue;
            }
        }

        /// <summary>
        /// Searches the text color color changed.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        private static void SearchTextColorChanged(BindableObject obj, object oldValue, object newValue)
        {
            var autoCompleteView = obj as AutoCompleteView;
            if (autoCompleteView != null)
            {
                autoCompleteView.BtnSearch.TextColor = (Color)newValue;
            }
        }

        /// <summary>
        /// Searches the vertical options changed.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        private static void SearchVerticalOptionsChanged(BindableObject obj, object oldValue, object newValue)
        {
            var autoCompleteView = obj as AutoCompleteView;
            if (autoCompleteView != null)
            {
                autoCompleteView.BtnSearch.VerticalOptions = (LayoutOptions)newValue;
            }
        }

        /// <summary>
        /// Shows the search changed.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="oldShowSearchValue">if set to <c>true</c> [old show search value].</param>
        /// <param name="newShowSearchValue">if set to <c>true</c> [new show search value].</param>
        private static void ShowSearchChanged(BindableObject obj, object oldShowSearchValue, object newShowSearchValue)
        {
            var autoCompleteView = obj as AutoCompleteView;
            if (autoCompleteView != null)
            {
                autoCompleteView.BtnSearch.IsVisible = (bool)newShowSearchValue;
            }
        }

        /// <summary>
        /// Suggestions the background color changed.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        private static void SuggestionBackgroundColorChanged(BindableObject obj, object oldValue, object newValue)
        {
            var autoCompleteView = obj as AutoCompleteView;
            if (autoCompleteView != null)
            {
                autoCompleteView.LstSuggestions.BackgroundColor = (Color)newValue;
            }
        }

        /// <summary>
        /// Suggestions the height changed.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        private static void SuggestionHeightRequestChanged(BindableObject obj, object oldValue, object newValue)
        {
            var autoCompleteView = obj as AutoCompleteView;
            if (autoCompleteView != null)
            {
                autoCompleteView.LstSuggestions.HeightRequest = (double)newValue;
            }
        }
        /// <summary>
        /// Suggestions the item data template changed.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="oldShowSearchValue">The old show search value.</param>
        /// <param name="newShowSearchValue">The new show search value.</param>
        private static void SuggestionItemDataTemplateChanged(BindableObject obj, object oldShowSearchValue, object newShowSearchValue)
        {
            var autoCompleteView = obj as AutoCompleteView;
            if (autoCompleteView != null)
            {
                autoCompleteView.LstSuggestions.ItemTemplate = (DataTemplate)newShowSearchValue;
            }
        }

        /// <summary>
        /// Tests the vertical options changed.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        private static void TestVerticalOptionsChanged(BindableObject obj, object oldValue, object newValue)
        {
            var autoCompleteView = obj as AutoCompleteView;
            if (autoCompleteView != null)
            {
                autoCompleteView.EntText.VerticalOptions = (LayoutOptions)newValue;
            }
        }

        /// <summary>
        /// Texts the background color changed.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        private static void TextBackgroundColorChanged(BindableObject obj, object oldValue, object newValue)
        {
            var autoCompleteView = obj as AutoCompleteView;
            if (autoCompleteView != null)
            {
                autoCompleteView.EntText.BackgroundColor = (Color)newValue;
            }
        }

        /// <summary>
        /// Texts the color changed.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        private static void TextColorChanged(BindableObject obj, object oldValue, object newValue)
        {
            var autoCompleteView = obj as AutoCompleteView;
            if (autoCompleteView != null)
            {
                autoCompleteView.EntText.TextColor = (Color)newValue;
            }
        }

        /// <summary>
        /// Texts the horizontal options changed.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        private static void TextHorizontalOptionsChanged(BindableObject obj, object oldValue, object newValue)
        {
            var autoCompleteView = obj as AutoCompleteView;
            if (autoCompleteView != null)
            {
                autoCompleteView.EntText.VerticalOptions = (LayoutOptions)newValue;
            }
        }
        /// <summary>
        /// Texts the changed.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="oldPlaceHolderValue">The old place holder value.</param>
        /// <param name="newPlaceHolderValue">The new place holder value.</param>
        private static async void TextValueChanged(BindableObject obj, object oldValue, object newValue)
        {

            var oldPlaceHolderValue = (string)oldValue;

            var newPlaceHolderValue = (string)newValue;

            var control = obj as AutoCompleteView;

            if (control != null)
            {
                control.BtnSearch.IsEnabled = !string.IsNullOrEmpty(newPlaceHolderValue);

                var cleanedNewPlaceHolderValue = Regex.Replace((newPlaceHolderValue ?? string.Empty).ToLowerInvariant(), @"\s+", string.Empty);

                if (!string.IsNullOrEmpty(cleanedNewPlaceHolderValue) && control.Suggestions != null)
                {
                    var filteredSuggestions = await Task.Run(() =>
                    {
                        var filteredSuggestionsList = control.Suggestions.Cast<IClueObject>()
                            .Where(
                                x =>
                                {
                                    var result = false;
                                    foreach (var item in x.ClueStrings)
                                    {
                                        if (string.IsNullOrEmpty(item))
                                        {
                                            continue;
                                        }
                                        if (item.StartsWith(cleanedNewPlaceHolderValue, StringComparison.OrdinalIgnoreCase))
                                        {
                                            result = true;
                                        }
                                    }
                                    return result;
                                })
                            .OrderByDescending(x =>
                            {
                                var result = x.ClueStrings.IndexOf(cleanedNewPlaceHolderValue);
                                return result;
                            }).ToList();
                        return filteredSuggestionsList;
                    });
                    control._availableSuggestions.Clear();
                    if (filteredSuggestions.Count > 0)
                    {

                        foreach (var suggestion in filteredSuggestions)
                        {
                            control._availableSuggestions.Add(suggestion);
                        }

                        control.ShowHideListbox(true);
                    }
                    else
                    {
                        control.ShowHideListbox(false);
                    }

                }
                else
                {
                    if (control._availableSuggestions.Count > 0)
                    {
                        control._availableSuggestions.Clear();
                        control.ShowHideListbox(false);
                    }
                }
            }
        }

        /// <summary>
        /// Called when [selected item changed].
        /// </summary>
        /// <param name="selectedItem">The selected item.</param>
        private void OnSelectedItemChanged(object selectedItem)
        {
            SelectedItem = selectedItem;

            if (SelectedCommand != null)
                SelectedCommand.Execute(selectedItem);

            var handler = SelectedItemChanged;
            var clueObject = selectedItem as IClueObject;

            var selectedIndex = _availableSuggestions.IndexOf(clueObject);

            handler?.Invoke(this, new SelectedItemChangedEventArgs(selectedItem, selectedIndex));
        }

        /// <summary>
        /// Handles the <see cref="E:TextChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        private void OnTextChanged(TextChangedEventArgs e)
        {
            TextChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Shows the hide listbox.
        /// </summary>
        /// <param name="show">if set to <c>true</c> [show].</param>
        private void ShowHideListbox(bool show)
        {
            LstSuggestions.IsVisible = show;
        }

        private void EntText_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Text = e.NewTextValue;
            OnTextChanged(e);
        }

        private void BtnSearch_OnClicked(object sender, EventArgs e)
        {
            if (SearchCommand != null && SearchCommand.CanExecute(Text))
            {
                SearchCommand.Execute(Text);
            }

        }

        private void LstSuggestions_OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            //If not null, bind as specified by Path
            if (!string.IsNullOrEmpty(DisplayPath))
            {
                this.EntText.BindingContext = e.SelectedItem;
                var binding = new Binding(DisplayPath);
                EntText.SetBinding(Entry.TextProperty, binding);

            }
            //If Path is empty, assignment is based on the first result of ClueString
            else
            {
                var clueObject = e.SelectedItem as IClueObject;
                if (clueObject != null)
                {
                    var candidateDisplay = clueObject.ClueStrings.FirstOrDefault();
                    //The first result of ClueString is empty, only ToString, What a tragedy!
                    EntText.Text = !string.IsNullOrEmpty(candidateDisplay) ?
                        candidateDisplay :
                        e.SelectedItem.ToString();
                }
            }

            _availableSuggestions.Clear();
            ShowHideListbox(false);
            OnSelectedItemChanged(e.SelectedItem);

            if (ExecuteOnSuggestionClick
                && SearchCommand != null
                && SearchCommand.CanExecute(Text))
            {
                SearchCommand.Execute(e);
            }
        }
    }
}
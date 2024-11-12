using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using SimpleNewsAPI;
namespace InputEvents;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {
   public MainWindow () {
      // Init App
      InitializeComponent ();
      NewsItems = new ();
      newsListBox.ItemsSource = NewsItems;
      FetchData ();
      // Event handlers
      btnLoadMore.Click += BtnLoadMore_Click;
      this.AddHandler (Button.ClickEvent, new RoutedEventHandler (ButtonClickHandler), false);
      //this.AddHandler (Button.ClickEvent, new RoutedEventHandler (ButtonClickHandler), false);
      // Command binding
      CommandBinding capitalizeCommandBinding
         = new (CapitalizeCommand, ExecuteCustomCommand, CanExecuteCustomCommand);
      this.CommandBindings.Add (capitalizeCommandBinding);
      this.DataContext = this;
   }

   #region RoutedEvents
   private void BtnLoadMore_Click (object sender, RoutedEventArgs e)
      => FetchData ();

   private void ButtonClickHandler (object sender, RoutedEventArgs e)
      => MessageBox.Show ("ButtonClickHandler");
   #endregion

   #region KeyEvents
   private void KeyEvents_PreviewKeyDown (object sender, KeyEventArgs e) {
      if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.A) {
         // Selects all elements
         newsListBox.SelectedItems.Clear ();
         foreach (var elem in newsListBox.Items)
            newsListBox.SelectedItems.Add (elem);
      } else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.C) {
         // Copies selected items - Url link
         CopySelectedItemsLink ();
      } else {
         switch (e.Key) {
            case Key.F: // First Element
               newsListBox.SelectedItems.Clear ();
               newsListBox.SelectedIndex = 0;
               newsListBox.ScrollIntoView (newsListBox.SelectedItem);
               //e.Handled = true;
               break;
            case Key.L: // Last Element
               newsListBox.SelectedItems.Clear ();
               newsListBox.SelectedIndex = newsListBox.Items.Count - 1;
               newsListBox.ScrollIntoView (newsListBox.SelectedItem);
               //e.Handled = true;
               break;
            case Key.Escape: // Reset Selection
               newsListBox.SelectedItems.Clear ();
               break;
         }
      }
   }
   #endregion

   #region MouseEvents
   private void NewsItem_MouseDown (object sender, MouseButtonEventArgs e) {
      newsListBox.SelectedItems.Clear ();
      if (e.LeftButton is (MouseButtonState.Pressed or MouseButtonState.Released)) {
         //MessageBox.Show ("NewsItem_MouseDown");
      }
   }

   private void NewsItemOptions_MouseDown (object sender, MouseButtonEventArgs e) {
      newsListBox.SelectedItems.Clear ();
      //MessageBox.Show ("StackPanel MouseDown - NewsItem - Options");
      if (e.LeftButton == MouseButtonState.Pressed) {
         //MessageBox.Show ("NewsItemOptions_MouseDown");
      }
      e.Handled = false;
   }

   private void NewsItemCopy_MouseDown (object sender, RoutedEventArgs e) {
      e.Handled = false;
      MessageBox.Show ("NewsItemCopy_MouseDown");
   }

   private void NewsItemCopy_Click (object sender, RoutedEventArgs e) {
      //MessageBox.Show ($"Source: {e.Source}\nOriginalSource: {e.OriginalSource}\nHandled: {e.Handled}\nRoutedEvent: {e.RoutedEvent.Name}");
      e.Handled = false;
      MessageBox.Show ("Button Click - Copy");
      newsListBox.SelectedItems.Clear ();
      Button clickedButton = sender as Button;
      if (clickedButton != null) {
         StackPanel parentStackPanel = FindParent<StackPanel> (clickedButton);
         if (parentStackPanel != null) {
            ListBoxItem parentListBoxItem = FindParent<ListBoxItem> (parentStackPanel);
            if (parentListBoxItem != null) {
               int index = newsListBox.ItemContainerGenerator.IndexFromContainer (parentListBoxItem);
               newsListBox.SelectedIndex = index;
               CopySelectedItemsLink ();
            }
         }
      }
   }

   private void ContextMenu_Copy_Click (object sender, RoutedEventArgs e) => CopySelectedItemsLink ();
   #endregion

   #region Command
   public ICommand CapitalizeRelayCommand => new RelayCommand ((x => {
      txtInput.Text = txtInput.Text.ToUpper ();
   }), (x => !string.IsNullOrEmpty (txtInput.Text) && txtInput.Text.All (x => char.IsLetter (x))));

   // Command - Without RelayCommand
   public static readonly RoutedCommand _capitalizeCommand = new ();
   
   public ICommand CapitalizeCommand {
      get { return _capitalizeCommand; }
   }

   private void ExecuteCustomCommand (object sender, ExecutedRoutedEventArgs e) {
      txtInput2.Text = txtInput2.Text.ToUpper ();
   }

   private void CanExecuteCustomCommand (object sender, CanExecuteRoutedEventArgs e) {
      e.CanExecute = !string.IsNullOrEmpty (txtInput2.Text) && txtInput2.Text.All (x => char.IsLetter (x));
   }
   #endregion

   #region Methods
   void CopySelectedItemsLink () {
      if (newsListBox.SelectedItem != null) {
         // Get the content of the selected item and copy it to the clipboard
         string links = "";
         if (newsListBox.SelectedItems.Count > 0) {
            var list = newsListBox.SelectedItems.OfType<NewsItem> ().Select (x => x.Link).ToList ();
            links = string.Join (Environment.NewLine, list);
            Clipboard.SetText (links);
         }
      }
   }
   #endregion

   #region HelperMethods
   private T FindParent<T> (DependencyObject child) where T : DependencyObject {
      while (true) {
         DependencyObject parentObject = VisualTreeHelper.GetParent (child);
         if (parentObject == null) return null;
         if (parentObject is T parent) return parent;
         child = parentObject;
      }
   }
   #endregion

   #region NewsAPI
   public ObservableCollection<NewsItem> NewsItems { get; set; }
   string NextPage { get; set; } = string.Empty;
   bool IsFetching = false;
   private async void FetchData () {
      if (IsFetching) return;
      IsFetching = true; btnLoadMore.IsEnabled = false;
      var result = await NewsAPI.FetchTestData ();
      if (result.IsSuccess) {
         NextPage = result.NextPage;
         foreach (var news in result.Data) NewsItems.Add (news);
      } else {
         MessageBox.Show ("News API Fetch Failed!");
      }
      IsFetching = false; btnLoadMore.IsEnabled = true;
   }
   #endregion
}
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.VisualTree;
using SS14.Launcher.Views;

namespace SS14.Launcher.ViewModels
{
    public class HomePageViewModel : MainWindowTabViewModel
    {
        public override string Name => "Home";

        public Control Control { get; set; }

        public HomePageViewModel()
        {
            Favorites.Add(new ServerEntryViewModel("foo", 50) {IsAltBackground = true});
            Favorites.Add(new ServerEntryViewModel("fooer", 50));
            Favorites.Add(new ServerEntryViewModel("fooest", 50) {IsAltBackground = true});
            Favorites.Add(new ServerEntryViewModel("fooester", 50));
            Favorites.Add(new ServerEntryViewModel("fooestest", 50) {IsAltBackground = true});
            Favorites.Add(new ServerEntryViewModel("fooestestest", 50));
        }

        public ObservableCollection<ServerEntryViewModel> Favorites { get; }
            = new ObservableCollection<ServerEntryViewModel>();

        public async void DirectConnectPressed()
        {
            var window = GetWindow();
            if (window == null)
            {
                return;
            }

            var res = await new DirectConnectDialog().ShowDialog<string>(window);
        }

        public void AddFavoritePressed()
        {
        }

        private Window GetWindow()
        {
            return Control?.GetVisualRoot() as Window;
        }
    }
}
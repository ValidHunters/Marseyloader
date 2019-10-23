namespace SS14.Launcher.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            Updater = new ClientUpdaterViewModel();
        }

        public ClientUpdaterViewModel Updater { get; }

        public void OnWindowInitialized()
        {
            Updater.OnWindowInitialized();
        }
    }


}
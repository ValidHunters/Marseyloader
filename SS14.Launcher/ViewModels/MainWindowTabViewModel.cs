namespace SS14.Launcher.ViewModels
{
    public abstract class MainWindowTabViewModel : ViewModelBase
    {
        public abstract string Name { get; }

        public virtual void Selected()
        {
        }
    }
}
namespace SS14.Launcher.ViewModels.MainWindowTabs;

public abstract class MainWindowTabViewModel : ViewModelBase
{
    public abstract string Name { get; }

    public bool IsSelected { get; set; }

    public virtual void Selected()
    {
    }
}

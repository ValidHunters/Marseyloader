using ReactiveUI.Fody.Helpers;

namespace SS14.Launcher.ViewModels.Login;

public abstract class BaseLoginViewModel : ViewModelBase
{
    [Reactive] public bool Busy { get; protected set; }
    [Reactive] public string? BusyText { get; protected set; }
    [Reactive] public ViewModelBase? OverlayControl { get; set; }

    public virtual void Activated()
    {

    }
}
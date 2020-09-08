using ReactiveUI.Fody.Helpers;

namespace SS14.Launcher.ViewModels.Login
{
    public abstract class BaseLoginViewModel : ViewModelBase
    {
        [Reactive] public bool Busy { get; protected set; }

        public virtual void Activated()
        {

        }
    }
}

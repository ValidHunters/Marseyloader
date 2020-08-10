using ReactiveUI.Fody.Helpers;
using SS14.Launcher.Models;

namespace SS14.Launcher.ViewModels.Login
{
    public sealed class ForgotPasswordViewModel : ViewModelBase
    {
        private readonly ConfigurationManager _cfg;
        private readonly MainWindowLoginViewModel _parentVm;

        [Reactive] public string EditingEmail { get; set; } = "";

        public ForgotPasswordViewModel(ConfigurationManager cfg, MainWindowLoginViewModel parentVm)
        {
            _cfg = cfg;
            _parentVm = parentVm;
        }

        public void BackPressed()
        {
            _parentVm.Screen = LoginScreen.Login;
        }
    }
}

using ReactiveUI.Fody.Helpers;
using SS14.Launcher.Models;

namespace SS14.Launcher.ViewModels.Login
{
    public sealed class ForgotPasswordViewModel : BaseLoginViewModel
    {
        private readonly ConfigurationManager _cfg;
        public MainWindowLoginViewModel ParentVM { get; }

        [Reactive] public string EditingEmail { get; set; } = "";

        public ForgotPasswordViewModel(ConfigurationManager cfg, MainWindowLoginViewModel parentVM)
        {
            _cfg = cfg;
            ParentVM = parentVM;
        }
    }
}

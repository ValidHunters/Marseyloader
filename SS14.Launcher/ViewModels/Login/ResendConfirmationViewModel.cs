using SS14.Launcher.Models;

namespace SS14.Launcher.ViewModels.Login
{
    public class ResendConfirmationViewModel : BaseLoginViewModel
    {
        private readonly ConfigurationManager _cfg;
        public MainWindowLoginViewModel ParentVM { get; }

        public ResendConfirmationViewModel(ConfigurationManager cfg, MainWindowLoginViewModel parentVM)
        {
            _cfg = cfg;
            ParentVM = parentVM;
        }
    }
}

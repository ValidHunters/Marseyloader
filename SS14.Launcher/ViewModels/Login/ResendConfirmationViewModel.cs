using SS14.Launcher.Models;

namespace SS14.Launcher.ViewModels.Login
{
    public class ResendConfirmationViewModel
    {
        private readonly ConfigurationManager _cfg;
        private readonly MainWindowLoginViewModel _parentVm;

        public ResendConfirmationViewModel(ConfigurationManager cfg, MainWindowLoginViewModel parentVm)
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

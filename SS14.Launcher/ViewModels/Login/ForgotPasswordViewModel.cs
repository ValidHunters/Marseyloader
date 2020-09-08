using System.Threading.Tasks;
using ReactiveUI.Fody.Helpers;
using SS14.Launcher.Models;

namespace SS14.Launcher.ViewModels.Login
{
    public sealed class ForgotPasswordViewModel : BaseLoginViewModel, IErrorOverlayOwner
    {
        private readonly ConfigurationManager _cfg;
        private readonly AuthApi _authApi;
        public MainWindowLoginViewModel ParentVM { get; }

        [Reactive] public string EditingEmail { get; set; } = "";

        private bool _errored;

        public ForgotPasswordViewModel(ConfigurationManager cfg, MainWindowLoginViewModel parentVM, AuthApi authApi)
        {
            _cfg = cfg;
            _authApi = authApi;
            ParentVM = parentVM;
        }

        public async void SubmitPressed()
        {
            Busy = true;
            try
            {
                // TODO: Remove Task.Delay here.
                await Task.Delay(1000);
                var errors = await _authApi.ForgotPasswordAsync(EditingEmail);

                _errored = errors != null;

                if (!_errored)
                {
                    // This isn't an error lol but that's what I called the control.
                    OverlayControl = new AuthErrorsOverlayViewModel(this, "Reset email sent", new []
                    {
                        "A reset link has been sent to your email address."
                    });
                }
                else
                {
                    OverlayControl = new AuthErrorsOverlayViewModel(this, "Error", errors!);
                }
            }
            finally
            {
                Busy = false;
            }

        }

        public void OverlayOk()
        {
            if (_errored)
            {
                // Clear overlay and allow re-submit if an error occured.
                OverlayControl = null;
            }
            else
            {
                // If the overlay was a success overlay, switch back to login.
                ParentVM.SwitchToLogin();
            }
        }
    }
}

using System.Threading.Tasks;
using ReactiveUI.Fody.Helpers;

namespace SS14.Launcher.ViewModels.Login
{
    public class ResendConfirmationViewModel : BaseLoginViewModel, IErrorOverlayOwner
    {
        private readonly AuthApi _authApi;
        public MainWindowLoginViewModel ParentVM { get; }

        [Reactive] public string EditingEmail { get; set; } = "";

        private bool _errored;

        public ResendConfirmationViewModel(MainWindowLoginViewModel parentVM, AuthApi authApi)
        {
            _authApi = authApi;
            ParentVM = parentVM;
        }

        public async void SubmitPressed()
        {
            Busy = true;
            try
            {
                var errors = await _authApi.ResendConfirmationAsync(EditingEmail);

                _errored = errors != null;

                if (!_errored)
                {
                    // This isn't an error lol but that's what I called the control.
                    OverlayControl = new AuthErrorsOverlayViewModel(this, "Confirmation email sent", new []
                    {
                        "A confirmation email has been sent to your email address."
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

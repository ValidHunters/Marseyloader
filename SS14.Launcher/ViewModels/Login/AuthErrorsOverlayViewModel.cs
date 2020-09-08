namespace SS14.Launcher.ViewModels.Login
{
    public class AuthErrorsOverlayViewModel : ViewModelBase
    {
        public IErrorOverlayOwner ParentVm { get; }
        public string Title { get; }
        public string[] Errors { get; }

        public AuthErrorsOverlayViewModel(IErrorOverlayOwner parentVm, string title, string[] errors)
        {
            ParentVm = parentVm;
            Title = title;
            Errors = errors;
        }
    }
}

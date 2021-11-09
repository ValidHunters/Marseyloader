namespace SS14.Launcher.ViewModels.Login;

public class AuthErrorsOverlayViewModel : ViewModelBase
{
    public IErrorOverlayOwner ParentVm { get; }
    public string Title { get; }
    public string[] Errors { get; }

    public AuthErrorsOverlayViewModel(IErrorOverlayOwner parentVM, string title, string[] errors)
    {
        ParentVm = parentVM;
        Title = title;
        Errors = errors;
    }
}
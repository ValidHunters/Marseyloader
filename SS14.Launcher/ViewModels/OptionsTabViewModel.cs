namespace SS14.Launcher.ViewModels
{
    public class OptionsTabViewModel : MainWindowTabViewModel
    {
        public override string Name => "Options";

        public string PlayerName { get; set; } = "Joe Genero";
    }
}
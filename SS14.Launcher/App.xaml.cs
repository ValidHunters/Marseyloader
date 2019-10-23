using Avalonia;
using Avalonia.Markup.Xaml;

namespace SS14.Launcher
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
   }
}
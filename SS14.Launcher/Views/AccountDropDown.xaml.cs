using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SS14.Launcher.Views;

public partial class AccountDropDown : UserControl
{
    public static readonly StyledProperty<bool> IsDropDownOpenProperty =
        AvaloniaProperty.Register<AccountDropDown, bool>(nameof(IsDropDownOpen));

    public bool IsDropDownOpen
    {
        get => GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    public AccountDropDown()
    {
        InitializeComponent();
    }
}

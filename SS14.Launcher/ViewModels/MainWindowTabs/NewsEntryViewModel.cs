using System;

namespace SS14.Launcher.ViewModels.MainWindowTabs;

public class NewsEntryViewModel : ViewModelBase
{
    public NewsEntryViewModel(string headline, Uri link)
    {
        Headline = headline;
        Link = link;
    }

    public string Headline { get; }
    public Uri Link { get; }

    public void Open()
    {
        Helpers.OpenUri(Link);
    }
}
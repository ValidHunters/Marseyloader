using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Input;
using Avalonia.Data.Converters;
using Microsoft.Toolkit.Mvvm.Input;
using Serilog;
using Marsey.Config;
using Marsey.Patches;
using Marsey.Subversion;
using Marsey.Misc;

namespace SS14.Launcher.ViewModels.MainWindowTabs
{
    public class PatchesTabViewModel : MainWindowTabViewModel
    {
        public override string Name => "Plugins";
        public ObservableCollection<MarseyPatch> MarseyPatches { get; } = new ObservableCollection<MarseyPatch>();
        public ObservableCollection<SubverterPatch> SubverterPatches { get; } = new ObservableCollection<SubverterPatch>();

        public ICommand OpenPatchDirectoryCommand { get; }

        public PatchesTabViewModel()
        {
            OpenPatchDirectoryCommand = new RelayCommand(() => OpenPatchDirectory(MarseyVars.MarseyPatchFolder));
            LoadPatches();
        }

        private void LoadPatches()
        {
            FileHandler.LoadAssemblies();

            List<MarseyPatch> marseys = Marsyfier.GetMarseyPatches();
            LoadPatchList(marseys, MarseyPatches, "marseypatches");

            List<SubverterPatch> subverters = Subverter.GetSubverterPatches();
            LoadPatchList(subverters, SubverterPatches, "subverterpatches");
        }

        private void OpenPatchDirectory(string directoryName)
        {
            Process.Start(new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = Path.Combine(Directory.GetCurrentDirectory(), directoryName)
            });
        }

        private void LoadPatchList<T>(List<T> patches, ICollection<T> patchList, string patchName) where T : IPatch
        {
            foreach (T patch in patches)
            {
                patchList.Add(patch);
            }
            Log.Debug($"Refreshed {patchName}, got {patchList.Count}.");
        }

    }
}

public class PathToFileNameConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        string? path = value as string;
        return Path.GetFileName(path);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}

public class BooleanToPreloadConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? "(preload)" : "";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}

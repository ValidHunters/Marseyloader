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
using Marsey;
using Marsey.Subversion;

namespace SS14.Launcher.ViewModels.MainWindowTabs
{
    public class PatchesTabViewModel : MainWindowTabViewModel
    {
        public override string Name => "Plugins";
        public bool SubverterPresent { get; set; }
        public ObservableCollection<MarseyPatch> MarseyPatches { get; } = new ObservableCollection<MarseyPatch>();
        public ObservableCollection<SubverterPatch> SubverterPatches { get; } = new ObservableCollection<SubverterPatch>();

        public ICommand OpenMarseyPatchDirectoryCommand { get; }
        public ICommand OpenSubverterPatchDirectoryCommand { get; }

        public PatchesTabViewModel()
        {
            SubverterPresent = Subverse.CheckSubverterPresent();
            OpenMarseyPatchDirectoryCommand = new RelayCommand(() => OpenPatchDirectory(MarseyVars.MarseyPatchFolder));
            OpenSubverterPatchDirectoryCommand = new RelayCommand(() => OpenPatchDirectory(MarseyVars.SubverterPatchFolder));
            LoadPatches();
        }

        private void LoadPatches()
        {
            FileHandler.LoadAssemblies();
            List<MarseyPatch> marseys =
                PatchListManager.GetPatchList<MarseyPatch>() ?? throw new InvalidOperationException();
            LoadPatchList(marseys, MarseyPatches, "marseypatches");
            if (!SubverterPresent) return;
            List<SubverterPatch> subverters = PatchListManager.GetPatchList<SubverterPatch>() ?? throw new InvalidOperationException();
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

        private void LoadPatchList<T>(List<T> patches, ObservableCollection<T> patchList, string patchName) where T : IPatch
        {
            foreach (var patch in patches)
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
        var path = value as string;
        return Path.GetFileName(path);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}

public class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool booleanValue)
        {
            return booleanValue;
        }
        throw new InvalidOperationException("Invalid boolean value");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        if (value is bool visibilityValue)
        {
            return visibilityValue;
        }
        throw new InvalidOperationException("Invalid visibility value");
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

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
        public ObservableCollection<MarseyPatch> SubverterPatches { get; } = new ObservableCollection<MarseyPatch>();

        public ICommand OpenMarseyPatchDirectoryCommand { get; }
        public ICommand OpenSubverterPatchDirectoryCommand { get; }

        public PatchesTabViewModel()
        {
            SubverterPresent = Subverse.InitSubverter();
            OpenMarseyPatchDirectoryCommand = new RelayCommand(() => OpenPatchDirectory("Marsey"));
            OpenSubverterPatchDirectoryCommand = new RelayCommand(() => OpenPatchDirectory("Subversion"));
            LoadPatches();
        }

        private void LoadPatches()
        {
            FileHandler.LoadAssemblies();
            LoadPatchList(PatchAssemblyManager.GetPatchList(), MarseyPatches, "marseypatches");
            if (!SubverterPresent) return;
            //Subverter.LoadSubverts();
            LoadPatchList(Subverter.GetSubverterPatches(), SubverterPatches, "subverterpatches");
        }

        private void OpenPatchDirectory(string directoryName)
        {
            Process.Start(new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = Path.Combine(Directory.GetCurrentDirectory(), directoryName)
            });
        }

        private void LoadPatchList(List<MarseyPatch> patches, ObservableCollection<MarseyPatch> patchList, string patchName)
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
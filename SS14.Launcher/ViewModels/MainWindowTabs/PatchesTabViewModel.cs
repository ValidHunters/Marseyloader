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
using Marsey.Game.ResourcePack;
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
        public ObservableCollection<ResourcePack> ResourcePacks { get; set; } = new ObservableCollection<ResourcePack>();
        public ICommand OpenPatchDirectoryCommand { get; }

        public PatchesTabViewModel()
        {
            OpenPatchDirectoryCommand = new RelayCommand(() => OpenPatchDirectory(MarseyVars.MarseyFolder));
            ReloadMods();
        }

        private void ReloadMods()
        {
            ClearList();
            FileHandler.LoadAssemblies();
            ResMan.LoadDir();

            List<MarseyPatch> marseys = Marsyfier.GetMarseyPatches();
            LoadPatchList(marseys, MarseyPatches, "marseypatches");

            List<SubverterPatch> subverters = Subverter.GetSubverterPatches();
            LoadPatchList(subverters, SubverterPatches, "subverterpatches");

            LoadResPacks(ResMan.GetRPacks());
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

        private void LoadResPacks(List<ResourcePack> ResPacks)
        {
            foreach (ResourcePack resource in ResPacks)
            {
                ResourcePacks.Add(resource);
            }
            
            Log.Debug($"Refreshed resourcepacks, got {ResourcePacks.Count}.");
        }

        private void ClearList()
        {
            ResourcePacks.Clear();
            SubverterPatches.Clear();
            MarseyPatches.Clear();
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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Microsoft.Toolkit.Mvvm.Input;
using Serilog;
using Marsey.Config;
using Marsey.Game.Resources;
using Marsey.Patches;
using Marsey.Subversion;
using Marsey.Misc;
using SS14.Launcher.Marseyverse;

namespace SS14.Launcher.ViewModels.MainWindowTabs
{
    public class PatchesTabViewModel : MainWindowTabViewModel
    {
        public override string Name => "Plugins";
        public ObservableCollection<MarseyPatch> MarseyPatches { get; } = new ObservableCollection<MarseyPatch>();
        public ObservableCollection<SubverterPatch> SubverterPatches { get; } = new ObservableCollection<SubverterPatch>();
        public ObservableCollection<ResourcePack> ResourcePacks { get; } = new ObservableCollection<ResourcePack>();
        public ICommand OpenPatchDirectoryCommand { get; }
        public ICommand ReloadModsCommand { get; }
        public ICommand EnableRefreshCommand { get; }

#if DEBUG
        public bool ShowRPacks => true;
#else
        public bool ShowRPacks => false;
#endif

        public PatchesTabViewModel()
        {
            OpenPatchDirectoryCommand = new RelayCommand(() => OpenPatchDirectory(MarseyVars.MarseyFolder));
            ReloadModsCommand = new RelayCommand(ReloadMods);
            EnableRefreshCommand = new RelayCommand(Refresh);
            ReloadMods();
        }

        private bool first = true;
        private void ReloadMods()
        {
            LoadInitialResources();
            LoadPatches();

            if (!first) return;

            EnableConfiguredPatches();
            first = false;
        }

        private void LoadInitialResources()
        {
            FileHandler.LoadAssemblies();
            ResMan.LoadDir();
        }

        private void LoadPatches()
        {
            LoadPatchList(Marsyfier.GetMarseyPatches(), MarseyPatches, "marseypatches");
            LoadPatchList(Subverter.GetSubverterPatches(), SubverterPatches, "subverterpatches");
            LoadResPacks(ResMan.GetRPacks(), ResourcePacks);
        }

        private void EnableConfiguredPatches()
        {
            List<string> assemblies = Persist.LoadPatchlistConfig();
            LoadEnabledPatches(assemblies, MarseyPatches);
            LoadEnabledPatches(assemblies, SubverterPatches);
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
            foreach (T patch in patches.Where(patch => !patchList.Any(r => r.Equals(patch))))
            {
                patchList.Add(patch);
            }

            Log.Debug($"Refreshed {patchName}, got {patchList.Count}.");
        }

        private void LoadResPacks(List<ResourcePack> ResPacks, ICollection<ResourcePack> RPacks)
        {
            foreach (ResourcePack resource in ResPacks)
            {
                if (RPacks.All(r => r.Dir != resource.Dir)){
                    RPacks.Add(resource);
                }
            }

            Log.Debug($"Refreshed resourcepacks, got {ResourcePacks.Count}.");
        }

        private void Refresh()
        {
            List<string> assemblyFileNames = new();
            SaveEnabledPatches(MarseyPatches, assemblyFileNames);
            SaveEnabledPatches(SubverterPatches, assemblyFileNames);

            Log.Debug($"Saved {assemblyFileNames.Count} patches to config");
            Persist.SavePatchlistConfig(assemblyFileNames);
        }

        private void SaveEnabledPatches(IEnumerable<IPatch> patches, List<string> fileNames)
        {
            foreach (IPatch patch in patches)
            {
                if (patch.Enabled)
                {
                    fileNames.Add(Path.GetFileName(patch.Asmpath));
                }
            }
        }

        private void LoadEnabledPatches(List<string> fileNames, IEnumerable<IPatch> patches)
        {
            foreach (IPatch patch in from filename in fileNames from patch in patches where Path.GetFileName(patch.Asmpath) == filename select patch)
            {
                patch.Enabled = true;
            }
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

using System.Reflection;
using HarmonyLib;

namespace Marsey.Game.Patches.Marseyports;

/// <summary>
/// Base class for backport patches or fixes on certain engine versions or content packs
/// Marsey uses backports in case of breaking changes or some inconvenience caused by the game developers
/// Backports use two identifiers - fork id and engine version, as attributes
/// <seealso cref="Marsey.Game.Patches.Marseyports.Attributes"/>
/// </summary>
public interface IBackport
{
    string TargetType { get; set; }
    string TargetMethod { get; set; }
    /// <summary>
    /// Does this backport target the content pack
    /// </summary>
    bool Content { get; set; }
    MethodInfo? PatchMethodInfo { get; set; }
    HarmonyPatchType? PatchType { get; set; }

    bool Patch();
}
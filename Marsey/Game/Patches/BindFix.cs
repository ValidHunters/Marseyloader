using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Marsey.Handbreak;

namespace Marsey.Game.Patches;

// https://media.discordapp.net/attachments/1132440600478756879/1208211732980301964/MalPR.png?ex=65e275dc&is=65d000dc&hm=d0e99b62cca1151e13a8e4af65cbe7b0e3c4f0d35e74965ab07640f87276df93&=&format=webp&quality=lossless
// Cred: https://github.com/space-wizards/RobustToolbox/pull/4903
// Todo: Remove when 
public static class BindFix
{
    public static void Patch()
    {
        Type Input = AccessTools.TypeByName("Robust.Client.Input.InputManager");
        MethodInfo LKFmi = AccessTools.Method(Input, "LoadKeyFile");
        MethodInfo BindFixTranspiler = AccessTools.Method(typeof(BindFix), "Transpiler");

        Manual.Patch(LKFmi, BindFixTranspiler, HarmonyPatchType.Transpiler);
    }
    
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> codes = [..instructions];

        for (int i = 0; i < codes.Count - 1; i++)
        {
            if (codes[i].opcode == OpCodes.Ldarg_2 && codes[i + 1].opcode == OpCodes.Call && ((MethodInfo)codes[i + 1].operand).Name == "RegisterBinding")
            {
                codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldc_I4_0)); // Erm its akshually false
                codes.Insert(i + 2, new CodeInstruction(OpCodes.Ceq));
                i += 2; // Skip the inserted instructions
            }
        }

        return codes.AsEnumerable();
    }
}
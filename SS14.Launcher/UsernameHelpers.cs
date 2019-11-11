// This file is taken from Robust.Shared.
// It'd probably be best to keep this stuff in sync in a better way but for now I'm copy pasting it.

using System;
using System.Diagnostics.CodeAnalysis;

namespace SS14.Launcher
{
    public static class UsernameHelpers
    {
        private const int NameLengthMax = 32;
        private static readonly string[] InvalidStrings = {
            " ", // That's a space.
            "\"",
            "'",
            "DrCelt",
        };

        /// <summary>
        ///     Checks whether a user name is valid.
        ///     If this is false, feel free to kick the person requesting it. Loudly.
        /// </summary>
        /// <param name="name">The name to check.</param>
        /// <param name="reason">A human-readable reason for why this username is not valid.</param>
        /// <returns>True if the name is acceptable, false otherwise.</returns>
        public static bool IsNameValid(string name, [NotNullWhen(false)] out string? reason)
        {
            // No empty username.
            if (string.IsNullOrWhiteSpace(name))
            {
                reason = "Name can't be empty";
                return false;
            }

            // TODO: This length check is crap and doesn't work correctly.
            if (name.Length > 0 && name.Length >= NameLengthMax)
            {
                reason = "Name too long";
                return false;
            }

            // TODO: Oh yeah, should probably cut out like, control characters or something.
            foreach (var item in InvalidStrings)
            {
                if (name.IndexOf(item, StringComparison.Ordinal) != -1)
                {
                    var invalidChar = item == " " ? "<space>" : item;
                    reason = $"Contains invalid character: \"{invalidChar}\"";
                    return false;
                }
            }

            reason = null;
            return true;
        }
    }
}
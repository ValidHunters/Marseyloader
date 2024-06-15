using System;
using System.Collections.Generic;
using DynamicData;
using Marsey.Patches;
using Marsey.Subversion;
using Splat;
using SS14.Launcher.Models.Data;
using SS14.Launcher.Utility;

namespace SS14.Launcher.Marseyverse;

public class TitleCondition
{
    public required Func<bool> Condition { get; set; }
    public required string Message { get; set; }
}

public class TitleManager
{
    private const string BanMessage = "Banned";
    private static readonly Random Random = new Random();

    public TitleManager()
    {
        DataManager cfg = Locator.Current.GetRequiredService<DataManager>(); // Thanks for inspiration.gif
        if (!cfg.GetCVar(CVars.RandTitle))
        {
            RandomTitle = LauncherVersion.Name;
            return;
        }

        List<TitleCondition> titleConditions = new List<TitleCondition>
        {
            new TitleCondition { Condition = () => OperatingSystem.IsWindows(), Message = "if (OperatingSystem.IsWindows()" },
            new TitleCondition { Condition = () => Subverter.GetSubverterPatches().Count > 5, Message = "Subversion is superior to git you know" },
            new TitleCondition { Condition = () => Marsyfier.GetMarseyPatches().Count > 10, Message = "Marsyfiedisms" },
            new TitleCondition { Condition = () => cfg.GetCVar(CVars.FakeRPC), Message = $"{CVars.RPCUsername} RP" }
        };

        foreach (TitleCondition condition in titleConditions)
        {
            if (condition.Condition())
                TagLines.Add(condition.Message);
        }

        string name = RandTitle();
        string tagline = "";

        if (name == BanMessage)
            tagline = RandBanReason();
        else
            tagline = RandTagLine();

        RandomTitle = name + ": " + tagline;
    }

    private static readonly List<string> Titles =
    [
        "Space Station 14 Launcher", "Dramalauncher",
        "Marsey", "Moonyware", "Marseyloader",
        "Robustcontrol", "Mirailoader", "Almost BepInEx",
        "Video game launcher", "MIT-certified funny",
        "戏剧装载机", "Oldest anarchy launcher in ss14",
        "ILVerifier", "Space Station 13", "BYOND",
        "Goonstation", "BYONDCONTROL", "Unitystation",
        "Stationeers", "RE:SS2D", "Schizostation 14",
        "Thaumatrauma", "Calamari v3", "hamaSS14", BanMessage
    ];

    private static readonly List<string> TagLines =
    [
        "Marsey is the cutest cat", "Not a cheat loader",
        "PR self-merging solutions", "RSHOE please come back",
        "Today I learned RSHOE is datae, it has never been more over",
        "As audited on discord.gg/ss14", "Leading disabler of engine signature checks",
        "Sigmund Goldberg died for this", "Sandbox sidestep simulator", "DIY bomb tutorials",
        "incel matchmaking service", "#1 ransomware provider", "God, King & Bussy",
        "The mayocide is coming", "Leading forum for misandrists", "Trans Rights!",
        "Largest long bacon provider", "天安門大屠殺", "Shitcode tester",
        "The code for the client literally calls itself a cheat client",
        "Tumblr client for fujoshis", "Cheap airfare and hotels", "Aliyah consulting",
        "Friday Night Funkin mod manager", "download .apk MOD (Infinite money, health, free admin)",
        "Disaster generator", "Primary cause of binary blob discussions",
        "George Bush did 9/11 and yet I'm the bad guy", "Hybristophiliac support group",
        "Space game but awesome", "Game SUCKS I go bed", "Go be ironic on wizden",
        "On payroll from the saudi government", "Handling the cheating situation since November",
        "World-class anti-cheat", "Leading cause of secondary psychosis",
        "they even made one for fastnoiselite which tells me some people have brain damage",
        "Station Announcement: Buttfucker69 (Medical Intern) has arrived at the station!",
        "Game's not out but there's 7,000+ ban appeals"
    ];

    // Have I truly lost any imagination? Yeah, kind of.
    private static readonly List<string> BanReasons =
    [
        "Using a modified client", "Self-antag", "Ban evasion", "Datacenter",
        "Mirrored ban from nyano", "Vac banned from secure server", "Unpleasant to deal with on github and ingame",
        "As CMO participated in slave RP", "Left seconds after receiving an ahelp.", "Claiming to be hitler of jamaican jazz",
        "eat shit you sick fuck", "GRUGSTEIN", "alt", "welderbombed a group of sec", "took a welding tank for a walk",
        "raider", "mass murdered people with the gib stick from a present", "overall self-antag and grief of the security department",
        "Gifted a chicken mob to warden, calling it \"BBC\"", "Cult. Appeal in 6 months.", "Literally has 20+ accounts.",
        "liked a transphobic post on twitter", "RP'ing as jocks beating up (killing) nerds (5) and shoving them into lockers",
        "Character named \"ADOLF RIZZLER\"", "wrote on a piece of paper \"I pissed myself\"", "Gross imcompetence as captain",
        "Giving out aa ids", "Being swept", "Being karma", "teaching people how to make schedule 1 narcotics", "Hitler RP in cargo orders",
        "Selling the nuke", "Actively trying to build up a room of miasma", "Set AME to 140 then Disconnected",
        "Exploiting. Teleported into armory using chairs.", "rule 0 permaban speedrun world champion ", "Take a break, bud",
        "No-appeal perma ban based on advice of the player and other hosts", "Metacomms", "\"soft antag rolling,\" powergaming, being rude in OOC",
        "Warned multiple times about powergaming as antag roles", "found to have been soft-antag rolling", "made the Janitor a furry against his will",
        "found to be displaying unhealthy investments in rounds based off of OOC/deadchat commentary", "Mass-RDM. Killed 31 people using electrified grilles",
        "Openly bragging about being a shitter", "Posting fake porn links isn't funny", "3 bans in 6 months threshold",
        "factors leading to a lot of attention seeking behavior", "Did nothing but plasmabomb med for 35 minutes",
        "asking people to do the thug shaker"
    ];

    private static readonly List<string> Actions =
    [
        "Pumping nitrous into distro", "Injecting plasma into batteries", "Slipcuffing hos", "Starting a cult", "Rolling for antag",
        "Plasmaflooding station", "Declaring cargonia", "Evading bans", "Taking fuel tanks for a walk", "Gibbing for no reason",
        "Bullying trialmins", "ANNOUNCING PRESENCE", "Petting Marsey", "Gossiping about spaceman drama", "Porting /vg/ to 14",
        "Granting +HOST", "Playing Balalaika", "Anime past 2018 is slop", "Trading Oil", "Abusing Roskomnadzor", "Making fun of people on mastodon",
        "Hacking the powernet", "Overthrowing the captain", "Building a mech", "Creating AI laws", "Running diagnostics on the engine",
        "Upgrading the medbay", "Securing the armory", "Repairing hull breaches", "Conducting xenobio research", "Exploring lavaland",
        "Mining for plasma", "Crafting improv shells", "Setting up the SM", "Breaching into secure areas", "Fighting syndicate operatives",
        "Escaping on the shuttle", "Reviving dead crewmembers", "Cloning the clown", "Rushing the captain's spare ID", "Sabotaging telecomms",
        "Disposal bypassing into armory", "Exploiting pulling"
    ];

    public string RandomTitle { get; }

    public static string RandTitle() => Titles[Random.Next(Titles.Count)];
    public static string RandTagLine() => TagLines[Random.Next(TagLines.Count)];
    public static string RandBanReason() => BanReasons[Random.Next(BanReasons.Count)];
    public static string RandAction() => Actions[Random.Next(Actions.Count)];
}

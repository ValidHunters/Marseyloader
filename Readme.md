## Rewrite maybe in 10 years

![# Marseyloader](SS14.Launcher/Assets/logo-long.png)

Space Station 14 launcher with client-side modding/patching support.

![# badge](Assets/README/no-stops-no-regrets.svg)
![# badge](Assets/README/ensuring-code-integrity.svg)
![# badge](Assets/README/works-on-selfmerging.svg)

### Changes

* **Integration with the Harmony patching library.**
* * Full functionality regarding methods in client/shared content/engine assemblies.
* * Sideloading custom code as part of the game
* * Win/Mac/Linux support
* * No injectors used, entirely based on reflection
* * Patches are hidden from game
* * "Backport" support
* Enabled multiaccount
* Privacy changes
* * Tokens are updated only on connect or account switch to evade alt detection
* * HWId spoofing
* * Forcibly disable Discord RPC
* * Disable Redialing (Forced reconnects)
* * Wizden hub mirror set as default hub
* * Guest/Authless mode
* * Option to not log into an account by default
* * Locally change username for screenshots (This doesn't change your username in-game)
* Marsey.

### Contributing
If you have any feature you want added to the main repository you are free to submit a pull request.

### Setting up
1. Build solution
2. Run "SS14.Launcher"

### Running
1. Download release
2. Extract launcher
3. Start the loader

### Patching
Marseyloader uses the [Harmony](https://github.com/pardeike/Harmony) patching library. Introduction for the library is provided [here](https://harmony.pardeike.net/) and documentation [here](https://harmony.pardeike.net/articles/intro.html).

Example patches can be found in the [ExampleMarseyPatch](https://github.com/ValidHunters/ExampleMarseyPatch) [SubversionalExamplePatch](https://github.com/ValidHunters/SubversionalExamplePatch) repositories.

### FAQ

#### Where do I ask for help?
Github issues or on the [discord server](https://discord.gg/xHtZXybKeh).

#### How do I make a patch?
[Example Marseypatches](https://github.com/ValidHunters/ExampleMarseyPatch)

#### What is subversion for?
Subversion is used for adding your custom code (and not patching existing code) to the game, like custom commands and what not that can fully interact with the game as if they were part of the original code.

#### Where do I put the patch dll's?
Wherever clicking on the "Open patch directory" in the "Plugins" tab leads you

#### Can you do X?
No.

#### IL logs?
Enable loader debug, will be on your desktop

#### STOP!

Project EOL's immediately when our beloved friends *over there* allow client-side resource packs and UI mods.<br>
This will never happen though.

### TODO
* Resource swapping (resource packs)

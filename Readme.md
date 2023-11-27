![# Marseyloader](SS14.Launcher/Assets/logo-long.png)

Space Station 14 launcher with client-side modding/patching support.

### Changes

* **Integration with the Harmony patching library.**
* * Full functionality regarding methods in client/shared content/engine assemblies.
* * Sideloading custom code using [Subverter](https://github.com/Subversionary/Subverter)
* * Win/Mac/Linux support
* * No injectors used, entirely based on reflection
* Enabled multiaccount
* * Tokens are updated only on connect or account switch to evade alt detection
* Locally change username for screenshots (This doesn't change your username in-game)
* Marsey.

**Project is in maintenance mode - Only bugs and crashes are going to be addressed** 

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

Example patches can be found in the [ExampleMarseyPatch](https://github.com/ValidHunters/ExampleMarseyPatch) repository.

Additionally, custom code can be loaded to the game using the [Subverter](https://github.com/Subversionary/Subverter) helper patch.

### FAQ

#### How do I make a patch?
[Example Marseypatches](https://github.com/ValidHunters/ExampleMarseyPatch)

#### How do I enable subversion?
Compile [Subverter](https://github.com/Subversionary/Subverter), put "Subverter.dll" only in the "Subversion" folder.

#### What is subversion for?
Subversion is used for adding your custom code (and not patching existing code) to the game, like custom commands and what not that can fully interact with the game as if they were part of the original code.

#### Where do I put the patch dll's?
Wherever clicking on the "Open patch directory" in the "Plugins" tab leads you

#### Can you do X?
No.

#### Dotnet 8 when?
now

#### IL logs?
Enable loader debug, will be on your desktop

### TODO
* Log cleanup.

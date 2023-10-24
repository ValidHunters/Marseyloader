# Marseyloader

Space Station 14 launcher with client-side modding/patching support.

### Changes

* **Integration with the Harmony patching library.**
* * Abilitiy to patch everything on the client side including shared.
* Enabled multiaccount
* * Tokens are update only on connect or account switch to evade alt detection
* Locally change username for screenshots (This doesn't change your username in-game)
* Violated MVVM
* Marsey.


### Setting up

1. Build solution
2. Run "SS14.Launcher"

### Patching
Marseyloader uses the [Harmony](https://github.com/pardeike/Harmony) patching library. Introduction for the library is provided [here](https://harmony.pardeike.net/) and documentation [here](https://dotnetcademy.net/Learn/4/Pages/1).

Example patches can be found in the [ExampleMarseyPatch](https://github.com/ValidHunters/ExampleMarseyPatch) repository.

### TODO
* Clean up logs.
* Unfuck MVVM
* Clean up MarseyPatcher

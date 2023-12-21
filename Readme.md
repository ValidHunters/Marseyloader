![# Marseyloader](SS14.Launcher/Assets/logo-long.png)

Space Station 14 launcher with client-side modding/patching support.

[![forthebadge](data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIxODguNjI1MDE1MjU4Nzg5MDYiIGhlaWdodD0iMzUiIHZpZXdCb3g9IjAgMCAxODguNjI1MDE1MjU4Nzg5MDYgMzUiPjxyZWN0IHdpZHRoPSIxMTQuMDAwMDA3NjI5Mzk0NTMiIGhlaWdodD0iMzUiIGZpbGw9IiMzMUM0RjMiLz48cmVjdCB4PSIxMTQuMDAwMDA3NjI5Mzk0NTMiIHdpZHRoPSI3NC42MjUwMDc2MjkzOTQ1MyIgaGVpZ2h0PSIzNSIgZmlsbD0iIzM4OUFENSIvPjx0ZXh0IHg9IjU3LjAwMDAwMzgxNDY5NzI2NiIgeT0iMTcuNSIgZm9udC1zaXplPSIxMiIgZm9udC1mYW1pbHk9IidSb2JvdG8nLCBzYW5zLXNlcmlmIiBmaWxsPSIjRkZGRkZGIiB0ZXh0LWFuY2hvcj0ibWlkZGxlIiBhbGlnbm1lbnQtYmFzZWxpbmU9Im1pZGRsZSIgbGV0dGVyLXNwYWNpbmc9IjIiPkdFTkVSQVRJTkc8L3RleHQ+PHRleHQgeD0iMTUxLjMxMjUxMTQ0NDA5MTgiIHk9IjE3LjUiIGZvbnQtc2l6ZT0iMTIiIGZvbnQtZmFtaWx5PSInTW9udHNlcnJhdCcsIHNhbnMtc2VyaWYiIGZpbGw9IiNGRkZGRkYiIHRleHQtYW5jaG9yPSJtaWRkbGUiIGZvbnQtd2VpZ2h0PSI5MDAiIGFsaWdubWVudC1iYXNlbGluZT0ibWlkZGxlIiBsZXR0ZXItc3BhY2luZz0iMiI+RFJBTUE8L3RleHQ+PC9zdmc+)](https://forthebadge.com)
[![forthebadge](data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIxNzQuNjU2MjUiIGhlaWdodD0iMzUiIHZpZXdCb3g9IjAgMCAxNzQuNjU2MjUgMzUiPjxyZWN0IHdpZHRoPSI2Ny4yMTg3NSIgaGVpZ2h0PSIzNSIgZmlsbD0iI2QwMDIzZiIvPjxyZWN0IHg9IjY3LjIxODc1IiB3aWR0aD0iMTA3LjQzNzUiIGhlaWdodD0iMzUiIGZpbGw9IiNhNjBjM2YiLz48dGV4dCB4PSIzMy42MDkzNzUiIHk9IjE3LjUiIGZvbnQtc2l6ZT0iMTIiIGZvbnQtZmFtaWx5PSInUm9ib3RvJywgc2Fucy1zZXJpZiIgZmlsbD0iI0ZGRkZGRiIgdGV4dC1hbmNob3I9Im1pZGRsZSIgYWxpZ25tZW50LWJhc2VsaW5lPSJtaWRkbGUiIGxldHRlci1zcGFjaW5nPSIyIj5TTE9USDwvdGV4dD48dGV4dCB4PSIxMjAuOTM3NSIgeT0iMTcuNSIgZm9udC1zaXplPSIxMiIgZm9udC1mYW1pbHk9IidNb250c2VycmF0Jywgc2Fucy1zZXJpZiIgZmlsbD0iI0ZGRkZGRiIgdGV4dC1hbmNob3I9Im1pZGRsZSIgZm9udC13ZWlnaHQ9IjkwMCIgYWxpZ25tZW50LWJhc2VsaW5lPSJtaWRkbGUiIGxldHRlci1zcGFjaW5nPSIyIj5IQVRFUyBUSElTPC90ZXh0Pjwvc3ZnPg==)](https://forthebadge.com)
[![forthebadge](data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIyMjUuOTg0Mzc1IiBoZWlnaHQ9IjM1IiB2aWV3Qm94PSIwIDAgMjI1Ljk4NDM3NSAzNSI+PHJlY3Qgd2lkdGg9Ijk5LjUxNTYyNSIgaGVpZ2h0PSIzNSIgZmlsbD0iIzkwMTNmZSIvPjxyZWN0IHg9Ijk5LjUxNTYyNSIgd2lkdGg9IjEyNi40Njg3NSIgaGVpZ2h0PSIzNSIgZmlsbD0iIzZmMTRiZiIvPjx0ZXh0IHg9IjQ5Ljc1NzgxMjUiIHk9IjE3LjUiIGZvbnQtc2l6ZT0iMTIiIGZvbnQtZmFtaWx5PSInUm9ib3RvJywgc2Fucy1zZXJpZiIgZmlsbD0iI0ZGRkZGRiIgdGV4dC1hbmNob3I9Im1pZGRsZSIgYWxpZ25tZW50LWJhc2VsaW5lPSJtaWRkbGUiIGxldHRlci1zcGFjaW5nPSIyIj5XT1JLUyBPTjwvdGV4dD48dGV4dCB4PSIxNjIuNzUiIHk9IjE3LjUiIGZvbnQtc2l6ZT0iMTIiIGZvbnQtZmFtaWx5PSInTW9udHNlcnJhdCcsIHNhbnMtc2VyaWYiIGZpbGw9IiNGRkZGRkYiIHRleHQtYW5jaG9yPSJtaWRkbGUiIGZvbnQtd2VpZ2h0PSI5MDAiIGFsaWdubWVudC1iYXNlbGluZT0ibWlkZGxlIiBsZXR0ZXItc3BhY2luZz0iMiI+U0VMRk1FUkdJTkc8L3RleHQ+PC9zdmc+)](https://forthebadge.com)

### Changes

* **Integration with the Harmony patching library.**
* * Full functionality regarding methods in client/shared content/engine assemblies.
* * Sideloading custom code using [Subverter](https://github.com/Subversionary/Subverter)
* * Win/Mac/Linux support
* * No injectors used, entirely based on reflection
* * Patches are hidden from game
* Enabled multiaccount
* * Tokens are updated only on connect or account switch to evade alt detection
* Locally change username for screenshots (This doesn't change your username in-game)
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

Example patches can be found in the [ExampleMarseyPatch](https://github.com/ValidHunters/ExampleMarseyPatch) repository.

Additionally, custom code can be loaded to the game using the [Subverter](https://github.com/Subversionary/Subverter) helper patch.

### FAQ

#### Where do I ask for help?

Github issues or on the [discord server](https://discord.gg/5RjbK7EzEm).

#### How do I make a patch?
[Example Marseypatches](https://github.com/ValidHunters/ExampleMarseyPatch)

#### How do I enable subversion?
Compile [Subverter](https://github.com/Subversionary/Subverter), put "Subverter.dll" in the directory with the executable.

#### What is subversion for?
Subversion is used for adding your custom code (and not patching existing code) to the game, like custom commands and what not that can fully interact with the game as if they were part of the original code.

#### Where do I put the patch dll's?
Wherever clicking on the "Open patch directory" in the "Plugins" tab leads you

#### Can you do X?
No.

#### IL logs?
Enable loader debug, will be on your desktop

### TODO
* Log cleanup.

# Visual Effect Graph - Extras

Raw bunch of featre prototypes for use with Visual Effect Graph. Some probably broken, totally non-official and unsupported by Unity. Use at your own risk.

## Features
* Initial State (MonoBehaviour) : Enables the effect to start in a Stopped State.
* Blocks
  * GPU Event : Spawn Rate (Over time or distance)
  * Custom Code Block
  * Position on Terrain
* Contexts
  * Distortion Quad (HDRP)
* Operators
  * Get Spawn Count
  * Get Spawn Time (used with SetSpawnTime custom spawner)
* Custom Spawners
  * Set Spawn Time
  * Count over Time
* VFX Volume Mixer
  * Volume Component to set up to 8 float, 8 vector3 and 8 colors
  * Preferences to set the counts and names of every float, vector3 and color by project
  * Parameter Binder for every property
* Parameter Binders
  * Audio Spectrum to Map
  * GameObject 
     * Hierarchy Attribute Map
     * Velocity
     * Previous Position
  * Input
     * Axis
     * Button
     * Mouse
     * Keyboard
     * Touch
  * Terrain
  * UI
    * Toggle
    * Slider

## Requirements

* Unity 2018.3
* Latest 4.x VisualEffectGraph package
* Latest 4.x HD Render Pipeline package

## Install

You can use a manual, local package installation if you need to alter the code locally or automate the fetch of the repository by using a git adress directly. The latter option shall download and manage automatically the repository, with the drawback of being read-only.

### Manual Version

* Clone repository somewhere of your liking
* Make sure your project is correctly configured with both HDRP and VFX Graph at correct revisions 
* In your project, open the `Window/Package Manager` window and use the + button to select the `Add Package from Disk...` option.
* Navigate to your repository folder and select the `package.json` file
* The repository shall be added

### Git reference version

* Make sure your project is correctly configured with both HDRP and VFX Graph at correct revisions 
* With unity closed, edit the `Packages/manifest.json` with a text editor
* append the line `    "net.peeweek.vfxgraph-extras": "https://github.com/IxxyXR/net.peeweek.vfxgraph-extras.git",` under `dependencies`



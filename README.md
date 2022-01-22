# Visual Effect Graph - Extras

Raw bunch of feature prototypes for use with Visual Effect Graph. These features are mostly unpolished, non-official and unsupported by Unity. Use at your own risk.

## Requirements

* Unity 2020.3 / VFX Graph Package

## Installing

You can use a manual, local package installation if you need to alter the code locally or automate the fetch of the repository by using a git address directly. The latter option shall download and manage automatically the repository, with the drawback of being read-only.

### OpenUPM Scoped Registry (2020.1 and newer)

Open Project Preferences and go to Package manager Window.

If not present, add this scoped registry:

* Name: **OpenUPM**
* URL : `https://package.openupm.com`
* Scopes : `net.peeweek`

Once added, you can close the project settings window.

Open Package manager (Window/Package Manager), select the **Visual Effect Graph (Extras)** package, and click the install button.

### Local Package

* Clone repository somewhere of your liking
* Make sure your project is correctly configured with both HDRP and VFX Graph at correct revisions 
* In your project, open the `Window/Package Manager` window and use the + button to select the `Add Package from Disk...` option.
* Navigate to your repository folder and select the `package.json` file
* The repository shall be added

## Features

Currently, these major features are available through the package.

### VFXGraph Extension

The VFXGraph Extension provides a menu with additional UI and functionality features in the VFX Graph Window. 

![](https://raw.githubusercontent.com/peeweek/net.peeweek.vfxgraph-extras/master/Documentation%7E/VFXExtension.png)

### Volume Mixer

Volume Mixer provides a way to define custom properties in the project and blend them through the volume system.

![](https://raw.githubusercontent.com/peeweek/net.peeweek.vfxgraph-extras/master/Documentation%7E/VFXVolumeMixer-Component.png)

These properties can then be retrieved and set to visual effects through a different VFX Volume Mixer PropertyBinders for each property type.

![](https://raw.githubusercontent.com/peeweek/net.peeweek.vfxgraph-extras/master/Documentation%7E/VFXVolumeMixer-Binder.png)

The list of settings available can be edited in the Project Settings, under __Volume Mixer__ category. Up to 8 float, 8 Vectors and 8 colors can be defined in the system.
![](https://raw.githubusercontent.com/peeweek/net.peeweek.vfxgraph-extras/master/Documentation%7E/VFXVolumeMixer-Settings.png)


### VFXGraph Scene Debug

A debug view that displays all the effects visible on screen. Editor Window is Available via the menu `Window/Analysis/VFX Graph Debug`.

![](https://raw.githubusercontent.com/peeweek/net.peeweek.vfxgraph-extras/master/Documentation%7E/VFXGraphDebug-Editor.png)

Alternatively, a Runtime view can be used with similar features. You can drop the `VFXDebug` prefab located in the Resources folder of the package. 

![](https://raw.githubusercontent.com/peeweek/net.peeweek.vfxgraph-extras/master/Documentation%7E/VFXGraphDebug-Runtime.png)

### VFX Template Gallery

VFX Template Gallery enables picking a starting point upon creating a new VFX Graph or adding a new system in the VFX Graph window :
- For Asset creation, use the `Visual Effect Graph (from Template)`
- For adding a new system, use the `T` key shortcut in the Graph Window, or the menu entry in the VFX Extension Menu
- In order to create your new templates, please create a `Visual Effect Graph Template` asset from the same menu as VFX Graph asset creation. In the inspector, you can also also take screenshots from the scene view using the menu in the inspector header (Useful for making your template screenshots)

![](https://raw.githubusercontent.com/peeweek/net.peeweek.vfxgraph-extras/master/Documentation%7E/VFXGraphTemplateGallery.png)

### Custom Block

Custom Block enables writing a block with custom HLSL code, providing input properties, random, and accessing particle attributes.

![](https://raw.githubusercontent.com/peeweek/net.peeweek.vfxgraph-extras/master/Documentation%7E/CustomBlock.png)



# Visual Effect Graph - Extras

Raw bunch of featre prototypes for use with Visual Effect Graph. Some probably broken, totally non-official and unsupported by Unity. Use at your own risk.

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
* append the line `    "net.peeweek.vfxgraph-extras": "https://github.com/peeweek/net.peeweek.vfxgraph-extras.git",` under `dependencies`


# Visual Effect Graph - Extras

Additional Features for use with VFX Graph. These features are mostly unpolished, non-official and unsupported by Unity. Use at your own risk.

## Requirements

* VFX Graph Package
* Unity
  * 2020.3 > Working
  * 2021.3 > Working
  * 2022.X <-- Probably not (due to multi window edition)

## Documentation

You can now access the documentation in the [Wiki](https://github.com/peeweek/net.peeweek.vfxgraph-extras/wiki)

## Installing

You can use a manual, local package installation if you need to alter the code locally or automate the fetch of the repository by using a git address directly. The latter option shall download and manage automatically the repository, with the drawback of being read-only.

### OpenUPM Scoped Registry (2020.1 and newer)

Open Project Preferences and go to Package manager Window.

If not present, add this scoped registry:

* Name: **OpenUPM**
* URL : `https://package.openupm.com`
* Scopes : `net.peeweek`

Once added, you can close the project settings window.

![](https://raw.githubusercontent.com/peeweek/net.peeweek.vfxgraph-extras/master/Documentation%7E/PackageManager.png)

Finally, Open Package manager (Window/Package Manager), select the **Visual Effect Graph (Extras)** package, and click the install button.

### Local Package

* Clone repository somewhere of your liking
* Make sure your project is correctly configured with both HDRP and VFX Graph at correct revisions 
* In your project, open the `Window/Package Manager` window and use the + button to select the `Add Package from Disk...` option.
* Navigate to your repository folder and select the `package.json` file
* The repository shall be added

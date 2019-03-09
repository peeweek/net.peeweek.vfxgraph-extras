# Visual Effect Graph - Extras

I forked https://github.com/peeweek/net.peeweek.vfxgraph-extras/ to allow me to easily experiment with my own custom blocks and nodes.

See https://github.com/peeweek/net.peeweek.vfxgraph-extras/blob/master/README.md for the original notes. The last bit of the installation differs thus (updated urls and package names):

### Git reference version

* Make sure your project is correctly configured with both HDRP and VFX Graph at correct revisions 
* With unity closed, edit the `Packages/manifest.json` with a text editor
* append the line `    "net.ixxy.vfxgraph-extras": "https://github.com/IxxyXR/net.ixxy.vfxgraph-extras.git",` under `dependencies`


### Additions:

Currently just a new block "Color/Color Blend" which supports Photoshop style blend modes.

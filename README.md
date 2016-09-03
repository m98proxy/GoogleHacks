# GoogleHacks

Project currently in development, come back soon.

GoogleHacks throws together a C# API for google maps 
specifically google street view integration with C#/OpenTK projects.

The API within provides sampling input of 6 Skybox images (front, back, left, right, up, down)
mapping them to a skybox using the X3D Background node, outputting a 3D rendering; 360 degree panorama.


The toolkit used is X3D-finely-sharpened built purely in C#.



Development notes:
* 3D rendering API provided by OpenGL via OpenTK.
* Quaternion based camera coordinates as input to street view api
* local caching of textures and preloading 
* preference of local cache to under utilise google api throughput 
* processing of textures into 3D voxels




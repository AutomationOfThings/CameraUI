## Remote Camera Controller UI

The application is to connect, preview and control remote cameras.

## Support
Suppoted by SUNAPI2.4.1 provided by SUMSUNG

## External Extension needed for Visual Studio 2015
The setup project in the solution uses Microsoft Visual Studio 2015 Installer Projects, which can be found here(https://visualstudiogallery.msdn.microsoft.com/f1cc3f3e-c300-40a7-8797-c509fb8933b9)

## Dependency
1. Prism
2. Extended.Wpf.Toolkit.2.8
3. MJPEG-Decoder.1.2
4. The CameraUI uses functionalities from LCM API. Therefore you need to add lcm.dll under directiry packages/LCM. You can update LCM API by simply replacing lcm.dll in packages/LCM.

## Notes:
In order to run the application from Visual Studio without installation. You need to create a RemoteCameraControllerData folder in Documents folder, and copy all files and folder in [CameraUI Project]/Data/ to this new folder.
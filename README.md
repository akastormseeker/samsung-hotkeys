# samsung-hotkeys
Hotkeys app for older "unsupported" laptops for Windows 10

Built using Visual Studio 2015 CE

Nuget dependencies: 
- NAudio (for getting global volume / mute status)

This program requires Samsung's BIOS interface driver and DLL, but getting them can be tricky. They need to be extracted from Samsung's EasySettings package, which doesn't want to install under Windows 10.

The files you need are SABI.sys and Sabi3.dll. Once you have them, place SABI.sys in C:\WINDOWS\system32\Drivers\, and Sabi3.dll with the compiled SamsungHotkeys.exe program. 

I was unable to figure out how to manually extract the InstallShield cab files with the EasySettings installer, but I had kept the original files from my working pre-1511 Windows 10 install.

You can still (I think*) use a program called FileUnsigner to remove the digital signature from the setup program, and then rename it to something else (ie. INSTALL_YOU_PIECE_OF_.exe) to get the installer to run. And maybe enable compatibility mode for good measure. If you can get the installer to run, then it will put SABI.sys in the proper location and register it for you, so you won't have to do that, just get the Sabi3.dll file from C:\Program Files (x86)\Samsung\EasySettings.

* I did a lot of things to try to get this working after the 1511 upgrade, and I think that what I ended up doing was this process.

# SmartPaths

A library for dealing with paths as strongly typed objects instead of strings.\
\
Also included are abstractions for IFile, IFolder, and IFileSystem.\
FileSystem implementations include physical disks as well as in memory RAM drives.

## How are paths handled in SmartPaths?

Paths can be easily constructed via implicit conversion from string:

```C#
AbsoluteFolderPath absoluteFolder = @"C:\Users\Default\";
AbsoluteFilePath absoluteFile = @"C:\Windows\notepad.exe";

RelativeFolderPath relativeFolder = @"..\..\Users\Default\";
RelativeFilePath relativeFile = @"..\..\Windows\notepad.exe";
```

Absolute and relative paths can be combined in various ways:

```C#
AbsoluteFilePath combinedFile = absoluteFolder + relativeFile; 
AbsoluteFilePath combinedFile2 = absoluteFolder / relativeFile; 
// these both result in @"C:\Windows\notepad.exe"

RelativeFilePath relative = absoluteFolder >> absoluteFile;
// this results in @"..\..\Windows\notepad.exe"
```

## How are Paths modeled in SmartPaths

The core path abstractions are illustrated in the following diagram:

![path-interfaces](Docs/path-interfaces.png)

The implementation of the abstractions are illustrated in the following diagram:

![path-implementation](Docs/path-implementation.png)

## How are Files and Folders handled in SmartPaths?

A "Disk" file system and a "RAM" file system have been implemented.\
Both types can be instantiated explicitly, or accessed from the "FileSystem" class.

```C#
FileSystem.Disk
FileSystem.Ram
```

The FileSystem class has a "Current" property that defaults to the DiskFileSystem.\
This property is settable if you wish to use another file system in your application.\

```C#
FileSystem.Current
```

There is a "WorkingDirectory" folder path in every file system.\
With the disk file system, this is backed by Environment.CurrentDirectory.

```C#
FileSystem.Disk.WorkingDirectory = @"c:\my\path\";
```

All file system methods taking a relative path, are resolved against the "WorkingDirectory" before being executed.

```C#
FileSystem.Disk.GetOrCreateFile(@"some\path");
```

## How are Files and Folders modeled in SmartPaths?

The storage abstractions are illustrated in the following diagram:

![storage-abstractions](Docs/storage-abstractions.png)

### Build
You can build SmartPaths using Visual Studio 2022.\
We use [Nuke](https://nuke.build/) as the build engine, and deliver packages on [NuGet.org](https://www.nuget.org/packages?q=SmartPaths)

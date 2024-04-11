## "~/Path/../SmartPaths/"  => ~/SmartPaths/

A library for dealing with paths as strongly typed objects instead of strings.\
\
Also included are abstractions for IFile, IFolder, and IFileSystem.\
FileSystem implementations include physical disks as well as in memory RAM drives.

### How are paths handled in SmartPaths?

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

### Feedback / Contribute

Project development is hosted on [GitHub](https://github.com/EddieGarmon/SmartPaths).\
Issues, bugs, and improvements can be [logged here](https://github.com/EddieGarmon/SmartPaths/issues).
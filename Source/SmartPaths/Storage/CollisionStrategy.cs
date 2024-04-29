namespace SmartPaths.Storage;

/// <summary>Specifies what should happen when trying to create/rename a file or folder to a name that
///     already exists.</summary>
public enum CollisionStrategy
{

    /// <summary>Automatically generate a unique name by appending to the name of the file or folder.</summary>
    GenerateUniqueName = 0,

    /// <summary>Replace the existing file or folder.</summary>
    ReplaceExisting = 1,

    /// <summary>Return an error if another file or folder exists with the same name and abort the
    ///     operation.</summary>
    FailIfExists = 2

}
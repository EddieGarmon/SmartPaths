namespace SmartPaths.Storage;

public abstract class SmartFile<TFolder, TFile> : IFile
    where TFolder : SmartFolder<TFolder, TFile>
    where TFile : SmartFile<TFolder, TFile>
{

    protected SmartFile(AbsoluteFilePath filePath) {
        Path = filePath;
    }

    public abstract TFolder Folder { get; }

    public string Name => Path.FileName;

    public AbsoluteFilePath Path { get; }

    public bool WasDeleted { get; protected set; }

    protected DateTimeOffset LastWrite { get; set; }

    IFolder IFile.Parent => Folder;

    public Task Delete() {
        return Delete(true);
    }

    public abstract Task<bool> Exists();

    public abstract Task<DateTimeOffset> GetLastWriteTime();

    public abstract Task<TFile> Move(AbsoluteFilePath newPath, CollisionStrategy collisionStrategy = CollisionStrategy.FailIfExists);

    public virtual Task<Stream> OpenToAppend() {
        return Task.Run(() => {
                            Stream stream = new SmartStream<TFolder, TFile>(this, true);
                            stream.Seek(0, SeekOrigin.End);
                            return stream;
                        });
    }

    public virtual Task<Stream> OpenToRead() {
        return Task.Run(Stream () => new SmartStream<TFolder, TFile>(this, false));
    }

    public virtual Task<Stream> OpenToWrite() {
        return Task.Run<Stream>(() => new SmartStream<TFolder, TFile>(this, true));
    }

    public abstract Task Touch();

    internal abstract byte[] GetData();

    internal abstract void SetData(byte[] value);

    protected Task Delete(bool notify) {
        WasDeleted = true;
        // NB: do nothing to Data so that we could restore
        Folder.Expunge(this, notify);
        return Task.CompletedTask;
    }

    Task<IFile> IFile.Move(AbsoluteFilePath newPath, CollisionStrategy collisionStrategy) {
        return Move(newPath, collisionStrategy).ContinueWith(IFile (task) => task.Result);
    }

}
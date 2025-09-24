namespace SmartPaths.Storage;

internal class SmartStream<TFolder, TFile> : Stream
    where TFolder : SmartFolder<TFolder, TFile>
    where TFile : SmartFile<TFolder, TFile>
{

    private readonly bool _canWrite;
    private readonly SmartFile<TFolder, TFile> _file;
    private readonly MemoryStream _stream;

    private bool _isDisposed;
    private bool _wasModified;

    public SmartStream(SmartFile<TFolder, TFile> file, SmartStreamMode mode) {
        _file = file;
        _stream = new MemoryStream();
        switch (mode) {
            case SmartStreamMode.Read:
                _canWrite = false;
                LoadStream();
                break;
            case SmartStreamMode.Write:
                _canWrite = true;
                LoadStream();
                break;
            case SmartStreamMode.New:
                _canWrite = true;
                //leave stream empty
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }

    public override bool CanRead => true;

    public override bool CanSeek => true;

    public override bool CanWrite => _canWrite;

    public override long Length => _stream.Length;

    public override long Position {
        get => _stream.Position;
        set => _stream.Position = value;
    }

    public override void Flush() {
        if (!_canWrite) {
            throw new Exception("File not opened for writing." + _file.Path);
        }
        _stream.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count) {
        return _stream.Read(buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin) {
        return _stream.Seek(offset, origin);
    }

    public override void SetLength(long value) {
        if (!_canWrite) {
            throw new Exception("File not opened for writing." + _file.Path);
        }
        _stream.SetLength(value);
        _wasModified = true;
    }

    public override void Write(byte[] buffer, int offset, int count) {
        if (!_canWrite) {
            throw new Exception("File not opened for writing." + _file.Path);
        }
        _stream.Write(buffer, offset, count);
        _wasModified = true;
    }

    protected override void Dispose(bool disposing) {
        if (_isDisposed) {
            return;
        }
        _isDisposed = true;
        if (_canWrite && _wasModified) {
            _file.SetData(_stream.ToArray());
            _file.Touch();
        }
        _stream.Dispose();
        base.Dispose(disposing);
    }

    private void LoadStream() {
        byte[] data = _file.GetData();
        _stream.Write(data, 0, data.Length);
        _stream.Seek(0, SeekOrigin.Begin);
    }

}
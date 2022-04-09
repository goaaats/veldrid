using System.Runtime.InteropServices;

namespace NativeLibraryLoader;

public class NativeLibrary : IDisposable
{
    private readonly IntPtr handle;

    public NativeLibrary(string name) : this(new[] {name}){}

    public NativeLibrary(IEnumerable<string> names)
    {
        foreach (string name in names)
        {
            if (System.Runtime.InteropServices.NativeLibrary.TryLoad(name, out var foundHandle))
            {
                this.handle = foundHandle;
                return;
            }
        }

        throw new Exception("Didn't find library.");
    }

    public T LoadFunction<T>(string name)
    {
        return Marshal.GetDelegateForFunctionPointer<T>(LoadFunction(name));
    }

    public IntPtr LoadFunction(string name)
    {
        if (!System.Runtime.InteropServices.NativeLibrary.TryGetExport(this.handle, name, out var address))
            throw new Exception("Couldn't find function.");

        return address;
    }

    private void ReleaseUnmanagedResources()
    {
        System.Runtime.InteropServices.NativeLibrary.Free(this.handle);
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~NativeLibrary()
    {
        ReleaseUnmanagedResources();
    }
}

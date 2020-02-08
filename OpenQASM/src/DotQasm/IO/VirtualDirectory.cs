using System.IO;
using System.Collections.Generic;
using System;

namespace DotQasm.IO {

public interface IFilesystemObject {
    string Name {get;}
}

public interface IDirectoryHandle: IFilesystemObject {
    IDirectoryHandle ResolveSubdir(string directory);
    IFileHandle ResolveFile(string filename);
    IFilesystemObject ResolvePath(string path);
}

public interface IFileHandle: IFilesystemObject {
    string Contents {get;}
}

public class VirtualFile: IFileHandle {
    public string Contents {get; private set;}
    public string Name {get; private set;}
    public VirtualFile(string name, string contents) {
        this.Contents = contents;
        this.Name = name;
    }
}

public class PhysicalFile: IFileHandle {
    private string path;
    public string Contents => File.ReadAllText(path);
    public string Name {get; private set;}
    public PhysicalFile(string fspath) {
        this.path = fspath;
        this.Name = Path.GetFileName(fspath);
    }
}

public class PhysicalDirectory : IDirectoryHandle {
    private string fspath;
    public string Name {get; private set;}
    public PhysicalDirectory(string fspath) {
        this.fspath = fspath;
        this.Name = Path.GetFileName(fspath);
    }

    public IFileHandle ResolveFile(string filename) {
        if (File.Exists(Path.Combine(fspath, filename))) {
            return new PhysicalFile(Path.Combine(fspath, filename));
        } else {
            return null;
        }
    }

    public IDirectoryHandle ResolveSubdir(string directory) {
        if (Directory.Exists(Path.Combine(fspath, directory))) {
            return new PhysicalDirectory(Path.Combine(fspath, directory));
        } else {
            return null;
        }
    }

    public IFilesystemObject ResolvePath(string path) {
        var fullPath = Path.Combine(fspath, path);
        if (File.Exists(fullPath)) {
            return new PhysicalFile(fullPath);
        } else if (Directory.Exists(fullPath)) {
            return new PhysicalDirectory(fullPath);
        } else {
            return null;
        }
    }
}

public class VirtualDirectory: IDirectoryHandle {

    public string Name {get; private set;}    
    private Dictionary<string, IDirectoryHandle> subdirs = new Dictionary<string, IDirectoryHandle>();
    private Dictionary<string, IFileHandle> files = new Dictionary<string, IFileHandle>();

    public VirtualDirectory(string name) {
        this.Name = name;
    }

    public void AddFile(IFileHandle handle) {
        files[handle.Name] = handle;
    }

    public IDirectoryHandle ResolveSubdir(string directory) {
        if (subdirs.ContainsKey(directory)) {
            return subdirs[directory];
        } else {
            return null;
        }
    }

    public IFileHandle ResolveFile(string filename) {
        if (files.ContainsKey(filename)) {
            return files[filename];
        } else {
            return null;
        }
    }

    public IFilesystemObject ResolvePath(string path) {
        // get first element
        var root = Path.GetPathRoot(path);

        // if only first element
        if (root.Length == 0) {
            if (files.ContainsKey(path)) {
                return ResolveFile(path);
            } else if (subdirs.ContainsKey(path)) {
                return ResolveSubdir(path);
            } else {
                return null;
            }
        } 
        // send remainder off
        else {
            if (subdirs.ContainsKey(root)) {
                return subdirs[root].ResolvePath(path.Substring(root.Length));
            } else {
                return null;
            }
        }
    }
}

}
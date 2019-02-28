using GVFS.Common;
using GVFS.Common.FileSystem;
using GVFS.Common.Tracing;
using Mono.Unix.Native;
using System;
using System.IO;

namespace GVFS.Platform.Posix
{
    public partial class PosixFileSystem : IPlatformFileSystem
    {
        public bool SupportsFileMode { get; } = true;

        public void FlushFileBuffers(string path)
        {
            // TODO(Posix): Use native API to flush file
        }

        public void MoveAndOverwriteFile(string sourceFileName, string destinationFilename)
        {
            if (Stdlib.rename(sourceFileName, destinationFilename) != 0)
            {
                NativeMethods.ThrowLastWin32Exception($"Failed to rename {sourceFileName} to {destinationFilename}");
            }
        }

        public void CreateHardLink(string newFileName, string existingFileName)
        {
            // TODO(Posix): Use native API to create a hardlink
            File.Copy(existingFileName, newFileName);
        }

        public void ChangeMode(string path, ushort mode)
        {
           Syscall.chmod(path, (FilePermissions)mode);
        }

        public bool TryGetNormalizedPath(string path, out string normalizedPath, out string errorMessage)
        {
            return PosixFileSystem.TryGetNormalizedPathImplementation(path, out normalizedPath, out errorMessage);
        }

        public bool HydrateFile(string fileName, byte[] buffer)
        {
            return NativeFileReader.TryReadFirstByteOfFile(fileName, buffer);
        }

        public bool IsExecutable(string fileName)
        {
            Stat statBuffer = this.StatFile(fileName);
            return NativeStat.IsExecutable(statBuffer.st_mode);
        }

        public bool IsSocket(string fileName)
        {
            Stat statBuffer = this.StatFile(fileName);
            return NativeStat.IsSock(statBuffer.st_mode);
        }

        public bool TryCreateDirectoryWithAdminOnlyModify(ITracer tracer, string directoryPath, out string error)
        {
            throw new NotImplementedException();
        }

        public bool TryCreateOrUpdateDirectoryToAdminModifyPermissions(ITracer tracer, string directoryPath, out string error)
        {
            throw new NotImplementedException();
        }

        private Stat StatFile(string fileName)
        {
            if (Syscall.stat(fileName, out Stat statBuffer) == 0)
            {
                return statBuffer;
            }

            throw NativeMethods.LastWin32Exception($"Failed to stat {fileName}");
        }

        private static class NativeStat
        {
            public static bool IsSock(FilePermissions mode)
            {
                // #define  S_ISSOCK(m) (((m) & S_IFMT) == S_IFSOCK)    /* socket */
                return (mode & FilePermissions.S_IFMT) == FilePermissions.S_IFSOCK;
            }

            public static bool IsExecutable(FilePermissions mode)
            {
                return (mode & (FilePermissions.S_IXUSR | FilePermissions.S_IXGRP | FilePermissions.S_IXOTH)) != 0;
            }
        }

        private class NativeFileReader
        {
            public static bool TryReadFirstByteOfFile(string fileName, byte[] buffer)
            {
                int fileDescriptor = -1;
                bool readStatus = false;
                try
                {
                    fileDescriptor = Syscall.open(fileName, OpenFlags.O_RDONLY);
                    if (fileDescriptor != -1)
                    {
                        readStatus = TryReadOneByte(fileDescriptor, buffer);
                    }
                }
                finally
                {
                    Syscall.close(fileDescriptor);
                }

                return readStatus;
            }

            private static bool TryReadOneByte(int fileDescriptor, byte[] buffer)
            {
                unsafe
                {
                    fixed (byte* buf = buffer)
                    {
                        long numBytes = Syscall.read(fileDescriptor, buf, 1);

                        if (numBytes == -1)
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
        }
    }
}

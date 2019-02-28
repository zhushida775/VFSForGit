using GVFS.Common;
using GVFS.Common.FileSystem;
using GVFS.Common.Tracing;
using System;
using System.IO;
using System.Runtime.InteropServices;
using Mono.Unix.Native;

namespace GVFS.Platform.Posix
{
    public class PosixFileBasedLock : FileBasedLock
    {
        private int lockFileDescriptor;

        public PosixFileBasedLock(
            PhysicalFileSystem fileSystem,
            ITracer tracer,
            string lockPath)
            : base(fileSystem, tracer, lockPath)
        {
            this.lockFileDescriptor = -1;
        }

        public override bool TryAcquireLock()
        {
            if (this.lockFileDescriptor == -1)
            {
                this.FileSystem.CreateDirectory(Path.GetDirectoryName(this.LockPath));

                this.lockFileDescriptor = Syscall.open(
                    this.LockPath,
                    OpenFlags.O_CREAT | OpenFlags.O_WRONLY,
                    (FilePermissions)Convert.ToUInt16("644", 8));

                if (this.lockFileDescriptor == -1)
                {
                    Errno errno = Stdlib.GetLastError();
                    EventMetadata metadata = this.CreateEventMetadata((int)errno);
                    this.Tracer.RelatedWarning(
                        metadata,
                        $"{nameof(PosixFileBasedLock)}.{nameof(this.TryAcquireLock)}: Failed to open lock file");

                    return false;
                }
            }

            if (NativeMethods.FLock(this.lockFileDescriptor, NativeMethods.LockEx | NativeMethods.LockNb) != 0)
            {
                int errno = Marshal.GetLastWin32Error();
                if (errno != (int)Errno.EINTR && errno != (int)Errno.EWOULDBLOCK)
                {
                    EventMetadata metadata = this.CreateEventMetadata(errno);
                    this.Tracer.RelatedWarning(
                        metadata,
                        $"{nameof(PosixFileBasedLock)}.{nameof(this.TryAcquireLock)}: Unexpected error when locking file");
                }

                return false;
            }

            return true;
        }

        public override void Dispose()
        {
            if (this.lockFileDescriptor != -1)
            {
                if (Syscall.close(this.lockFileDescriptor) != 0)
                {
                    // Failures of close() are logged for diagnostic purposes only.
                    // It's possible that errors from a previous operation (e.g. write(2))
                    // are only reported in close().  We should *not* retry the close() if
                    // it fails since it may cause a re-used file descriptor from another
                    // thread to be closed.

                    Errno errno = Stdlib.GetLastError();
                    EventMetadata metadata = this.CreateEventMetadata((int)errno);
                    this.Tracer.RelatedWarning(
                        metadata,
                        $"{nameof(PosixFileBasedLock)}.{nameof(this.Dispose)}: Error when closing lock fd");
                }

                this.lockFileDescriptor = -1;
            }
        }

        private EventMetadata CreateEventMetadata(int errno = 0)
        {
            EventMetadata metadata = new EventMetadata();
            metadata.Add("Area", nameof(PosixFileBasedLock));
            metadata.Add(nameof(this.LockPath), this.LockPath);
            if (errno != 0)
            {
                metadata.Add(nameof(errno), errno);
            }

            return metadata;
        }

        private static class NativeMethods
        {
            public const int LockSh = 1; // #define LOCK_SH   1    /* shared lock */
            public const int LockEx = 2; // #define LOCK_EX   2    /* exclusive lock */
            public const int LockNb = 4; // #define LOCK_NB   4    /* don't block when locking */
            public const int LockUn = 8; // #define LOCK_UN   8    /* unlock */

            [DllImport("libc", EntryPoint = "flock", SetLastError = true)]
            public static extern int FLock(int fd, int operation);
        }
    }
}

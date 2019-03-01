using GVFS.Common;
using GVFS.Common.FileSystem;
using GVFS.Common.Git;
using GVFS.Common.Tracing;
using GVFS.Platform.Posix;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;

namespace GVFS.Platform.Linux
{
    public partial class LinuxPlatform : Posix.PosixPlatform
    {
        public LinuxPlatform()
            : base(string.Empty)
        {
        }

        public override IKernelDriver KernelDriver { get; } = null;
        public override IGitInstallation GitInstallation { get; } = new LinuxGitInstallation();
        public override IDiskLayoutUpgradeData DiskLayoutUpgrade { get; } = new LinuxDiskLayoutUpgradeData();
        public override IPlatformFileSystem FileSystem { get; } = new PosixFileSystem();

        public override bool IsProcessActive(int processId)
        {
            return LinuxPlatform.IsProcessActiveImplementation(processId);
        }

        public override string GetOSVersionInformation()
        {
            ProcessResult result = ProcessHelper.Run("uname", args: "-a", redirectOutput: true);
            return string.IsNullOrWhiteSpace(result.Output) ? result.Errors : result.Output;
        }

        public override string GetNamedPipeName(string enlistmentRoot)
        {
            return LinuxPlatform.GetNamedPipeNameImplementation(enlistmentRoot);
        }

        public override bool IsConsoleOutputRedirectedToFile()
        {
            return LinuxPlatform.IsConsoleOutputRedirectedToFileImplementation();
        }

        public override bool IsElevated()
        {
            return LinuxPlatform.IsElevatedImplementation();
        }

        public override bool TryGetGVFSEnlistmentRoot(string directory, out string enlistmentRoot, out string errorMessage)
        {
            return LinuxPlatform.TryGetGVFSEnlistmentRootImplementation(directory, out enlistmentRoot, out errorMessage);
        }
    }
}

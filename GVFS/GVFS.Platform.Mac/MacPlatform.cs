using GVFS.Common;
using GVFS.Common.FileSystem;
using GVFS.Common.Git;
using GVFS.Common.Tracing;
using GVFS.Platform.Posix;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;

namespace GVFS.Platform.Mac
{
    public partial class MacPlatform : Posix.PosixPlatform
    {
        public MacPlatform()
            : base(".dmg")
        {
        }

        public override IKernelDriver KernelDriver { get; } = new ProjFSKext();
        public override IGitInstallation GitInstallation { get; } = new MacGitInstallation();
        public override IDiskLayoutUpgradeData DiskLayoutUpgrade { get; } = new MacDiskLayoutUpgradeData();
        public override IPlatformFileSystem FileSystem { get; } = new PosixFileSystem();

        public override bool IsProcessActive(int processId)
        {
            return MacPlatform.IsProcessActiveImplementation(processId);
        }

        public override string GetOSVersionInformation()
        {
            ProcessResult result = ProcessHelper.Run("sw_vers", args: string.Empty, redirectOutput: true);
            return string.IsNullOrWhiteSpace(result.Output) ? result.Errors : result.Output;
        }

        public override string GetNamedPipeName(string enlistmentRoot)
        {
            return MacPlatform.GetNamedPipeNameImplementation(enlistmentRoot);
        }

        public override bool IsConsoleOutputRedirectedToFile()
        {
            return MacPlatform.IsConsoleOutputRedirectedToFileImplementation();
        }

        public override bool IsElevated()
        {
            return MacPlatform.IsElevatedImplementation();
        }

        public override bool TryGetGVFSEnlistmentRoot(string directory, out string enlistmentRoot, out string errorMessage)
        {
            return MacPlatform.TryGetGVFSEnlistmentRootImplementation(directory, out enlistmentRoot, out errorMessage);
        }
    }
}

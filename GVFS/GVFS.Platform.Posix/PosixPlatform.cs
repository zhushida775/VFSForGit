﻿using GVFS.Common;
using GVFS.Common.FileSystem;
using GVFS.Common.Git;
using GVFS.Common.Tracing;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;

namespace GVFS.Platform.Posix
{
    public abstract partial class PosixPlatform : GVFSPlatform
    {
        protected PosixPlatform(string installerExtension)
            : base(
                executableExtension: string.Empty,
                installerExtension: installerExtension,
                underConstruction: new UnderConstructionFlags(
                    supportsGVFSService: false,
                    supportsGVFSUpgrade: false,
                    supportsGVFSConfig: false))
        {
        }

        public override bool TryVerifyAuthenticodeSignature(string path, out string subject, out string issuer, out string error)
        {
            throw new NotImplementedException();
        }

        public override void ConfigureVisualStudio(string gitBinPath, ITracer tracer)
        {
        }

        public override bool TryGetGVFSHooksPathAndVersion(out string hooksPaths, out string hooksVersion, out string error)
        {
            hooksPaths = string.Empty;

            // TODO(Posix): Get the hooks version rather than the GVFS version (and share that code with the Windows platform)
            hooksVersion = ProcessHelper.GetCurrentProcessVersion();
            error = null;
            return true;
        }

        public override bool TryInstallGitCommandHooks(GVFSContext context, string executingDirectory, string hookName, string commandHookPath, out string errorMessage)
        {
            errorMessage = null;

            string gvfsHooksPath = Path.Combine(executingDirectory, GVFSPlatform.Instance.Constants.GVFSHooksExecutableName);

            File.WriteAllText(
                commandHookPath,
                $"#!/bin/sh\n{gvfsHooksPath} {hookName} \"$@\"");
            GVFSPlatform.Instance.FileSystem.ChangeMode(commandHookPath, Convert.ToUInt16("755", 8));

            return true;
        }

        public override void IsServiceInstalledAndRunning(string name, out bool installed, out bool running)
        {
            throw new NotImplementedException();
        }

        public override void StartBackgroundProcess(ITracer tracer, string programName, string[] args)
        {
            ProcessLauncher.StartBackgroundProcess(tracer, programName, args);
        }

        public override NamedPipeServerStream CreatePipeByName(string pipeName)
        {
            NamedPipeServerStream pipe = new NamedPipeServerStream(
                pipeName,
                PipeDirection.InOut,
                NamedPipeServerStream.MaxAllowedServerInstances,
                PipeTransmissionMode.Byte,
                PipeOptions.WriteThrough | PipeOptions.Asynchronous,
                0,  // default inBufferSize
                0); // default outBufferSize)

            return pipe;
        }

        public override IEnumerable<EventListener> CreateTelemetryListeners(string providerName, string enlistmentId, string mountId)
        {
            // TODO: return TelemetryDaemonEventListener when the telemetry daemon has been implemented for Mac

            // string gitBinRoot = this.GitInstallation.GetInstalledGitBinPath();
            // var daemonListener = TelemetryDaemonEventListener.CreateIfEnabled(gitBinRoot, providerName, enlistmentId, mountId, pipeName: "vfs");
            // if (daemonListener != null)
            // {
            //     yield return daemonListener;
            // }

            yield break;
        }

        public override string GetCurrentUser()
        {
            throw new NotImplementedException();
        }

        public override Dictionary<string, string> GetPhysicalDiskInfo(string path, bool sizeStatsOnly)
        {
            // TODO(Posix): Collect disk information
            Dictionary<string, string> result = new Dictionary<string, string>();
            result.Add("GetPhysicalDiskInfo", "Not yet implemented on Posix");
            return result;
        }

        public override void InitializeEnlistmentACLs(string enlistmentPath)
        {
        }

        public override bool IsGitStatusCacheSupported()
        {
            // TODO(Posix): support git status cache
            return false;
        }

        public override FileBasedLock CreateFileBasedLock(
            PhysicalFileSystem fileSystem,
            ITracer tracer,
            string lockPath)
        {
            return new PosixFileBasedLock(fileSystem, tracer, lockPath);
        }

        public override bool TryKillProcessTree(int processId, out int exitCode, out string error)
        {
            ProcessResult result = ProcessHelper.Run("pkill", $"-P {processId}");
            error = result.Errors;
            exitCode = result.ExitCode;
            return result.ExitCode == 0;
        }
    }
}

﻿namespace GVFS.Platform.Posix
{
    public partial class PosixFileSystem
    {
        public static bool TryGetNormalizedPathImplementation(string path, out string normalizedPath, out string errorMessage)
        {
            // TODO(Mac): Properly determine normalized paths (e.g. across links)
            errorMessage = null;
            normalizedPath = path;
            return true;
        }
    }
}

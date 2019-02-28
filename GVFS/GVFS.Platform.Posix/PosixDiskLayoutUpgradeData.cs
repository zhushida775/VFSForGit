using GVFS.Common;
using GVFS.DiskLayoutUpgrades;

namespace GVFS.Platform.Posix
{
    public abstract class PosixDiskLayoutUpgradeData : IDiskLayoutUpgradeData
    {
        public DiskLayoutUpgrade[] Upgrades
        {
            get
            {
                return new DiskLayoutUpgrade[0];
            }
        }

        public bool TryParseLegacyDiskLayoutVersion(string dotGVFSPath, out int majorVersion)
        {
            majorVersion = 0;
            return false;
        }
    }
}

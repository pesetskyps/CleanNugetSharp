using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using NuGet;

namespace CleanNugetSharp
{
  public static class PlexPackageDeleter
  {
    static ILog logger = log4net.LogManager.GetLogger(typeof(PlexPackageDeleter));
    public static void BackupAndDelete(IPackageRepository repo, string nugetServerUrl, DataServicePackage package, string packageBackupfolder, bool whatIf)
    {
      try
      {
        Backup(nugetServerUrl, package, packageBackupfolder, whatIf);
      }
      catch (Exception ex)
      {
        throw new Exception(string.Format("Backup of package {0} version {1} failed. skipping delete",package.Id,package.Version),ex);
      }

      Delete(repo,package,whatIf);
    }

    private static void Backup(string nugetServerUrl, DataServicePackage package, string packageBackupfolder, bool whatIf)
    {
      var serverUrl = string.Format("{0}/api/v2/package/{1}/{2}", nugetServerUrl, package.Id, package.Version);
      var uri = new Uri(serverUrl);
      logger.Info(string.Format("Downloading package {0} to folder {1} from url {2}",package.Id,packageBackupfolder,uri));
      if (!whatIf)
      {
        var downloader = new PlexPackageDownloader(uri, packageBackupfolder);
        downloader.Download(package);
      }
      else
      {
        logger.Info("skipping as whatif is true");
      }
    }

    private static void Delete(IPackageRepository repo, DataServicePackage package, bool whatIf)
    {
      if (!whatIf)
      {
        repo.RemovePackage(package);
      }
      else
      {
        logger.Info("skipping as whatif is true");
      }
    }
  }
}

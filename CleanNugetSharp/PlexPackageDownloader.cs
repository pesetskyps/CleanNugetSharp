using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet;

namespace CleanNugetSharp
{
  public class PlexPackageDownloader
  {
    private Uri _uri;
    private string _downloadfolder;

    public PlexPackageDownloader(Uri uri, string downloadfolder)
    {
      _uri = uri;
      _downloadfolder = downloadfolder;
    }
    public void Download(IPackage package)
    {
      try
      {
        NuGet.PackageDownloader downloader = new NuGet.PackageDownloader();

        var filename = String.Format("{0}.{1}.nupkg", package.Id, package.Version);
        var path = Path.Combine(_downloadfolder, filename);
        var stream = new System.IO.FileStream(path, FileMode.Create);

        downloader.DownloadPackage(_uri, package, stream);
        downloader.ProgressAvailable += handler;
      }
      catch (Exception ex)
      {
        throw new Exception(string.Format("Download package {0} version {1} failed", package.Id, package.Version),ex);
      }

    }

    private void handler(object obj, ProgressEventArgs args)
    {
      Console.WriteLine(args.PercentComplete);
    }

  }
}

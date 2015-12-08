using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using System.Xml;
using log4net;
using log4net.Util;
using NuGet;

namespace CleanNugetSharp
{
  class Program
  {
    static int Main(string[] args)
    {
      //init logger
      log4net.Config.BasicConfigurator.Configure();
      ILog logger = log4net.LogManager.GetLogger(typeof(Program));
      logger.Info(string.Format("####################"));
      logger.Info(string.Format("Start Nuget clean at {0}", DateTime.Now));
      logger.Info(string.Format("####################"));
      //params
      string csprojNamespaceAlias = "csprojNameSpace";
      CsprojConfig appConfig = (CsprojConfig)System.Configuration.ConfigurationSettings.GetConfig("CsprojConfig");
      var csporjPathes = appConfig.pathes;
      var csporjReferenceExcludes = appConfig.referencesExcludes;
      string datasourcePackagesRegexSearchPattern = appConfig.datasourcePackagesRegexSearchPattern;
      string otherPackagesRegexSearchPattern = appConfig.otherPackagesRegexSearchPattern;
      string[] publicRepositories = appConfig.publicRepositories;

      string csprojNamespace;
      string datasourceMask;
      int countOfPreviousVersionsToStore;
      string nugetServerUrl;
      string packageBackupfolder;
      string dataSourceDefaultVersion;
      string datasourcePackageAddString;
      bool whatif;

      try
      {
        logger.Info(string.Format("Parsing arguments {0}", string.Join(" ", args)));
        var options = new Options();
        if (CommandLine.Parser.Default.ParseArguments(args, options))
        {
          csprojNamespace = options.csprojNamespace;
          datasourceMask = options.datasourceMask;
          countOfPreviousVersionsToStore = options.countOfPreviousVersionsToStore;
          nugetServerUrl = options.nugetServerUrl;
          packageBackupfolder = options.packageBackupfolder;
          dataSourceDefaultVersion = options.dataSourceDefaultVersion;
          if (options.datasourcePackagesRegexSearchPattern != null) datasourcePackagesRegexSearchPattern = options.datasourcePackagesRegexSearchPattern;
          if (options.otherPackagesRegexSearchPattern != null) { otherPackagesRegexSearchPattern = options.otherPackagesRegexSearchPattern; }
          
          datasourcePackageAddString = options.datasourcePackageAddString;
          whatif = options.whatif;
        }
        else
        {
          logger.Info(options.GetUsage());
          throw new Exception("Can't parse arguments");
        }

        //Composite params
        string nugetServerUrlNuget = string.Format("{0}/nuget", nugetServerUrl);
        string dataSourceFilter = string.Format("//{0}:Reference", csprojNamespaceAlias);

        //init csproj collections
        var packagesUsedInCsprojs = new HashSet<IPackageName>(new PackageComparer<IPackageName>());
        var packagesUsedInCsprojFiles = new List<IPackage>();

        //Parse csproj to get package names
        var csprojParser = new CsprojParser(csprojNamespaceAlias, csprojNamespace);
        foreach (var csprojPath in csporjPathes)
        {
          //csprojParser.Parse(csprojPath, dataSourceFilter, datasourcePackagesRegexSearchPattern, packagesUsedInCsprojs,datasourcePackageAddString, dataSourceDefaultVersion);
          csprojParser.Parse(csprojPath, dataSourceFilter, otherPackagesRegexSearchPattern, datasourcePackagesRegexSearchPattern, packagesUsedInCsprojs, csporjReferenceExcludes, datasourcePackageAddString, dataSourceDefaultVersion);

        }
        if (packagesUsedInCsprojs.Count == 0)
        {
          throw new Exception("No packages found in csproj files");
        }

        //init nuget repos
        IPackageRepository repo = PackageRepositoryFactory.Default.CreateRepository(nugetServerUrlNuget);
        List<IPackageRepository> publicRepositoriesList = publicRepositories.Select(packageRepository => PackageRepositoryFactory.Default.CreateRepository(packageRepository)).ToList();

        var packageResolver = new GetPackageDependencies(repo);
        HashSet<DataServicePackage> packagesToSave = new HashSet<DataServicePackage>(new PackageComparer<DataServicePackage>());
        var nugetServerAllPackages = repo.GetPackages();

        //get all package dependencies that will be preserved in the server
        logger.Info(string.Format("Getting all package dependencies that will be preserved in the server"));
        foreach (var package in packagesUsedInCsprojs)
        {
          logger.Info(string.Format("processing {0} with version {1}", package.Id, package.Version));
          var nugetPackage = nugetServerAllPackages.Where(x => x.Id == package.Id).ToList();
          //trying to find in public repos
          if (nugetPackage.Count == 0)
          {
            logger.Info(string.Format("Have not found {0} in private repo . Trying to find in public repository", package.Id));
            IEnumerable<IPackage> foundPublicPackages =new List<IPackage>();
            foreach (var packageRepository in publicRepositoriesList)
            {
              foundPublicPackages = packageRepository.FindPackagesById(package.Id);
              if (foundPublicPackages.ToList().Count != 0)
              {
                break;
              }
            }

            if (foundPublicPackages.ToList().Count == 0)
            {
              throw new Exception(string.Format("Couldn't find the package with id {0} in private and public repos", package.Id));
            }
            else
            {
              logger.Info(string.Format("Found {0} in public repository. skipping it from further processing", package.Id));
            }
          }

          packagesUsedInCsprojFiles.AddRange(nugetPackage);
          nugetPackage.Sort((x, y) => x.Version.CompareTo(y.Version));
          //take N versions to store in nuget
          var nugetPaackagesWithPreviousVersions = nugetPackage.TakeLast(countOfPreviousVersionsToStore);
          //get package dependencies
          foreach (var pack in nugetPaackagesWithPreviousVersions)
          {
            var nugetPackagesWithPreviousVersion = (DataServicePackage)pack;
            //get all dependencies
            packageResolver.GetPackageDependenciesRecursive(nugetPackagesWithPreviousVersion, packagesToSave);
          }
        }

        foreach (DataServicePackage package in packagesUsedInCsprojFiles)
        {
          if (!packagesToSave.Contains(package))
          {
            logger.Info(string.Format("Backing up and Deleting Package {0} version {1}", package.Id, package.Version));
            PlexPackageDeleter.BackupAndDelete(repo, nugetServerUrl, package, packageBackupfolder, whatif);
          }
        }

        foreach (var package in packagesToSave)
        {
          logger.Info(string.Format("Package {0} version {1} will be preserved in the repository", package.Id, package.Version));
        }

      }
      catch (Exception ex)
      {
        logger.Error(ex);
        return 1;
      }

      logger.Info(string.Format("####################"));
      logger.Info(string.Format("Finish Nuget clean at {0}", DateTime.Now));
      logger.Info(string.Format("####################"));
      Console.ReadLine();
      return 0;
    }
  }
}

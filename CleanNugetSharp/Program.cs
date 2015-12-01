using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Xml;
using NuGet;

namespace CleanNugetSharp
{
  class Program
  {
    static void Main(string[] args)
    {

      List<string> DatasourcePackages = new List<string>();
      string fullProjectPath = @"d:\Dev\Git\CA\Cloud.Apps.Accounting.Web\src\Cloud.UI.Accounting\Cloud.UI.Accounting.csproj";
      XmlDocument projDefinition = new XmlDocument();
      projDefinition.Load(fullProjectPath);
      XmlNamespaceManager nsmgr = new XmlNamespaceManager(projDefinition.NameTable);
      nsmgr.AddNamespace("bk", "http://schemas.microsoft.com/developer/msbuild/2003");

      XmlNodeList dataSourcesReferences = projDefinition.SelectNodes("//bk:Reference[contains(@Include, 'Plex.DataSources') and bk:Private]", nsmgr);

      if (dataSourcesReferences != null)
        foreach (XmlElement reference in dataSourcesReferences)
        {
          XmlNode referenceNode = reference.SelectSingleNode("bk:HintPath", nsmgr);
          if (referenceNode != null)
          {
            var packagePath = referenceNode.InnerText;
            const string packagesMask = @"..\lib\";
            var f = packagePath.Substring(packagePath.IndexOf(packagesMask, StringComparison.Ordinal) + packagesMask.Length);
            var ff = f.Replace(".dll", null);
            ff += ".Dev";
            DatasourcePackages.Add(ff);
          }
          foreach (var datasourcePackage in DatasourcePackages)
          {
            IPackageRepository repo = PackageRepositoryFactory.Default.CreateRepository("http://localhost:5555/nuget");
           
            var pack = repo.FindPackagesById(datasourcePackage).Last();
          
            var packageDependencySets = pack.DependencySets;
            foreach (var packageDependencySet in packageDependencySets)
            {
              foreach (var dependency in packageDependencySet.Dependencies)
              {
                IPackage package;
                //SemanticVersion ver = new SemanticVersion(dependency.VersionSpec.MaxVersion);
                var dep = repo.ResolveDependency(dependency,true,true);
                if (repo.TryFindPackage(dependency.Id, dependency.VersionSpec.MaxVersion, out package))
                {
                  
                }
              }
            }
            Console.WriteLine(pack);

          }
          //Console.WriteLine(f);
        }
      Console.ReadLine();
    }
  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet;

namespace CleanNugetSharp
{
  public class GetPackageDependencies
  {
    IPackageRepository repo;

    public GetPackageDependencies(IPackageRepository repository)
    {
      repo = repository;
    }
    public IPackage GetPackageDependenciesRecursive(IPackage package, ref List<PackageDependency> dependencies)
    {

      var packageDependencySets = package.DependencySets;
      if (!packageDependencySets.Any())
      {
        return package;
      }
      foreach (var packageDependencySet in packageDependencySets)
      {
        foreach (var dependency in packageDependencySet.Dependencies)
        {
          //SemanticVersion ver = new SemanticVersion(dependency.VersionSpec.MaxVersion);
          var dependecyPackage = repo.ResolveDependency(dependency, true, true);
          if (dependecyPackage != null)
          {
            return GetPackageDependenciesRecursive(dependecyPackage, ref dependencies);
          }
          else
          {
            throw new Exception("Dependency not found");
          }
        }
      }
      return package;
    }
  }
}

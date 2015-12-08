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
    readonly IPackageRepository _repo;
    public GetPackageDependencies(IPackageRepository repository)
    {
      _repo = repository;
    }
    public void GetPackageDependenciesRecursive(DataServicePackage package, HashSet<DataServicePackage> dependencies)
    {
      dependencies.Add(package);
      var packagesToReslove = new List<DataServicePackage>();

      foreach (var packageDependencySet in package.DependencySets)
      {
        foreach (var dependency in packageDependencySet.Dependencies)
        {
          var dependecyPackage = (DataServicePackage)_repo.ResolveDependency(dependency, true, true);
          if (dependecyPackage != null && !dependencies.Contains(dependecyPackage))
          {
            packagesToReslove.Add(dependecyPackage);
          }
        }
      }

      foreach (var p in packagesToReslove)
      {
        GetPackageDependenciesRecursive(p, dependencies);
      }
    }
  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet;
namespace CleanNugetSharp
{

  public class PackageComparer<T> : IEqualityComparer<T> where T: IPackageName
  {

    public bool Equals(T x, T y)
    {
      var a = (x.Id == y.Id) & (x.Version == y.Version);
      return a;
    }

    public int GetHashCode(T obj)
    {
      unchecked // Overflow is fine, just wrap
      {
        int hash = (int)2166136261;
        // Suitable nullity checks etc, of course :)
        hash = hash * 16777619 ^ obj.Id.GetHashCode();
        hash = hash * 16777619 ^ obj.Version.GetHashCode();
        return hash;
      }
    }
  }

  public class PackageComparerDataServicePackage : PackageComparer<DataServicePackage>
  {
  }

  public class PackageComparerIPackageName : PackageComparer<IPackageName>
  {
  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CleanNugetSharp
{
  public static class Helpers
  {
    // Ex: collection.TakeLast(5);
    public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> source, int N)
    {
      return source.Skip(Math.Max(0, source.Count() - N));
    }

    public static string GetMemberName<T>(Expression<Func<T>> memberExpression)
    {
      MemberExpression expressionBody = (MemberExpression)memberExpression.Body;
      return expressionBody.Member.Name;
    }
  }
}

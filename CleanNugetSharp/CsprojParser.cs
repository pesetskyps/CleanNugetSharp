using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using log4net;
using NuGet;

namespace CleanNugetSharp
{
  public class CsprojParser
  {
    private readonly string _csprojNamespaceAlias;
    private readonly string _csprojNamespace;
    readonly ILog logger = log4net.LogManager.GetLogger(typeof(Program));

    public CsprojParser(string csprojNamespaceAlias, string csprojNamespace)
    {
      _csprojNamespaceAlias = csprojNamespaceAlias;
      _csprojNamespace = csprojNamespace;
    }

    public void Parse(string path, string referencesXPathFilter, string packagesHintPathRegexSearchPattern, string libHintPathRegexSearchPattern, HashSet<IPackageName> packagesUsedInCsproj, string[] csporjReferenceExcludes,
      string addstring = null, string defaultVersion= null)
    {
      string hintPathRegexSearchPattern;

      string libPathPattern = @"^\.\.\\lib";
      string packagesPathPattern = @"^\.\.\\packages";

      XmlDocument projDefinition = new XmlDocument();
      projDefinition.Load(path);
      XmlNamespaceManager nsmgr = new XmlNamespaceManager(projDefinition.NameTable);
      nsmgr.AddNamespace(_csprojNamespaceAlias, _csprojNamespace);

      var otherPackagesReferences = projDefinition.SelectNodes(referencesXPathFilter, nsmgr);

      if (otherPackagesReferences == null)
      {
        throw new Exception(String.Format("Packages not found for csproj {0} by filter {1}", path, referencesXPathFilter));
      };
      foreach (XmlElement reference in otherPackagesReferences)
      {
        XmlNode referenceNode = reference.SelectSingleNode("csprojNameSpace:HintPath", nsmgr);
        
        if (referenceNode != null)
        {
          if (csporjReferenceExcludes.Contains(referenceNode.InnerText))
          {
            logger.Info(string.Format("Skipping reference {0} as it is in exclusion list", referenceNode.InnerText));
            continue;
          }

          string datasourceAddstring = "";
          string datasourceDefaultVersion = null;
          if (Regex.Matches(referenceNode.InnerText, libPathPattern, RegexOptions.IgnoreCase).Count != 0)
          {
            hintPathRegexSearchPattern = libHintPathRegexSearchPattern;
            datasourceAddstring = addstring;
            datasourceDefaultVersion = defaultVersion;
          }
          else if (Regex.Matches(referenceNode.InnerText, packagesPathPattern, RegexOptions.IgnoreCase).Count != 0)
          {
            hintPathRegexSearchPattern = packagesHintPathRegexSearchPattern;
          }
          else
          {
            //logger.Error(new Exception(string.Format("{0} does not fit neither {1} nor {2} pattern to search package names for", referenceNode.InnerText, packagesPathPattern, libPathPattern)));
            //continue;
            throw new Exception(string.Format("{0} does not fit neither {1} nor {2} pattern to search package names for", referenceNode.InnerText, packagesPathPattern, libPathPattern));
          }

          var packagePath = referenceNode.InnerText;
          var matches = Regex.Matches(packagePath, hintPathRegexSearchPattern, RegexOptions.IgnoreCase);
          if (matches.Count == 0)
          {
            //logger.Error(new Exception(String.Format("Packages not found for csproj {0} by regex filter {1}", path, hintPathRegexSearchPattern)));
            //continue;
            throw new Exception(String.Format("Packages not found for csproj {0} by regex filter {1}", path, hintPathRegexSearchPattern));
          }

          foreach (Match match in matches)
          {
            logger.Info(string.Format("parsed package with name {0} from {1}", match.Groups[1].Value, path));
            var packageName = string.Format("{0}{1}", match.Groups[1].Value, datasourceAddstring);
            var version = datasourceDefaultVersion ?? match.Groups[2].Value;
            var pack = new PackageName(packageName, new SemanticVersion(version));
            packagesUsedInCsproj.Add(pack);
          }
        }
        else
        {
          logger.Info(string.Format("skipping '{0}' reference for csproj {1}", reference.Attributes[0].Value, path));
        }
      }
    }
  }
}

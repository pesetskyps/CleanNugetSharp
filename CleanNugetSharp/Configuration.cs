using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace CleanNugetSharp
{
  public class CsprojConfig
  {
    [XmlArrayItem("path")]
    public string[] pathes;

    [XmlArrayItem("referencesExclude")]
    public string[] referencesExcludes;

    [XmlElement("datasourcePackagesRegexSearchPattern")]
    public string datasourcePackagesRegexSearchPattern;

    [XmlElement("otherPackagesRegexSearchPattern")]
    public string otherPackagesRegexSearchPattern;

    [XmlArrayItem("publicRepository")]
    public string[] publicRepositories;


    [XmlElement("csprojNamespace")]
    public string csprojNamespace;

    [XmlElement("countOfPreviousVersionsToStore")]
    public int countOfPreviousVersionsToStore;

    [XmlElement("nugetServerUrl")]
    public string nugetServerUrl;

    [XmlElement("packageBackupfolder")]
    public string packageBackupfolder;

    [XmlElement("dataSourceDefaultVersion")]
    public string dataSourceDefaultVersion;

    [XmlElement("datasourcePackageAddString")]
    public string datasourcePackageAddString;

    [XmlElement("whatif")]
    public bool whatif;
  }

  public class ConfigSectionHandler : IConfigurationSectionHandler
  {

    public const string SectionName = "CsprojConfig";

    public object Create(object parent, object configContext, XmlNode section)
    {
      var selectSingleNode = section.SelectSingleNode("//CsprojConfig");
      CsprojConfig retConf = null;
      if (selectSingleNode != null)
      {
        string szConfig = selectSingleNode.OuterXml;

        if (szConfig != string.Empty || szConfig != null)
        {
          XmlSerializer xsw = new XmlSerializer(typeof(CsprojConfig));
          MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(szConfig));
          ms.Position = 0;
          retConf = (CsprojConfig)xsw.Deserialize(ms);
        }
      }
      return retConf;
    }
  }
}

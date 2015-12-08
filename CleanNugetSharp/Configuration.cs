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

    [XmlArrayItem("datasourcePackagesRegexSearchPattern")]
    public string datasourcePackagesRegexSearchPattern;

    [XmlArrayItem("otherPackagesRegexSearchPattern")]
    public string otherPackagesRegexSearchPattern;
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

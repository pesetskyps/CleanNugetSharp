using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace CleanNugetSharp
{
  public class Options
  {
    [Option("csprojNamespace", Required = false,
      HelpText = "Namespace url of csporj file. Format: http://schemas.microsoft.com/developer/msbuild/2003")]
    public string csprojNamespace { get; set; }

    [Option("countOfPreviousVersionsToStore", Required = false,
      HelpText = "Number of versions of packages to preserve. Format: 3")]
    public int? countOfPreviousVersionsToStore { get; set; }

    [Option("nugetServerUrl", Required = false,
      HelpText = "Nuget Server Url. Format: http://localhost:5555")]
    public string nugetServerUrl { get; set; }

    [Option("packageBackupfolder", Required = false,
      HelpText = "Folder to store deleted packages as a backup. Format: d:\\temp\\test")]
    public string packageBackupfolder { get; set; }

    [Option("dataSourceDefaultVersion", Required = false,
      HelpText = "Currently Datasource don't have version so use default version to create IPackageName object. Format: 1.0.0.0")]
    public string dataSourceDefaultVersion { get; set; }

    [Option("datasourcePackagesRegexSearchPattern", Required = false,
      HelpText = "Regex pattern to extract Datasource name from Reference in Csproj. Format: ..\\lib\\(.*?).dll")]
    public string datasourcePackagesRegexSearchPattern { get; set; }

    [Option("otherPackagesRegexSearchPattern", Required = false,
      HelpText = @"Pattern to extract common packages (not DataSources) from Reference in csproj. Format: ..\\packages\\(.*?).(\\d.*?)\\\\")]
    public string otherPackagesRegexSearchPattern { get; set; }

    [Option("datasourcePackageAddString", Required = false,
      HelpText = "Currently datsource name in csproj doesn't match the name of the package in Nuget. Need to add 'Dev' in the end. Format: .Dev")]
    public string datasourcePackageAddString { get; set; }

    [Option("WhatIf", Required = false,
      HelpText = "Will just list in the log packages that will be deleted. No backup and delete will be performed. Format: true")]
    public bool? whatif { get; set; }

    public string GetUsage()
    {
      return HelpText.AutoBuild(this,
        (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
    }

  }


}

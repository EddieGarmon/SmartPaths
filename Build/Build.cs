// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Fallout.Common;
using Fallout.Common.CI;
using Fallout.Common.IO;
using Fallout.Common.Tooling;
using Fallout.Common.Tools.DotNet;
using Fallout.Common.Tools.ReportGenerator;
using Fallout.Common.Utilities.Collections;
using Fallout.Solutions;
using Serilog;
using static Fallout.Common.Tools.DotNet.DotNetTasks;
using static Fallout.Common.Tools.ReportGenerator.ReportGeneratorTasks;

[ShutdownDotNetAfterServerBuild]
class Build : FalloutBuild
{
    [Parameter] [Secret] readonly string NugetApiKey;

    [Parameter] readonly string NugetApiUrl = "https://api.nuget.org/v3/index.json";

    [Solution("SmartPaths.slnx")] readonly Solution Solution;

    [Parameter("Configuration to build - Default is 'Release' (server)")] Configuration Configuration = Configuration.Release;

    Target Clean =>
        def => def.Before(Restore)
            .Executes(() => {
                PathToSources.GlobDirectories("**/bin", "**/obj").ForEach(path => path.DeleteDirectory());
                PathToTests.GlobDirectories("**/bin", "**/obj").ForEach(path => path.DeleteDirectory());
                PathToArtifacts.CreateOrCleanDirectory();
            });

    Target Compile =>
        def => def.DependsOn(Restore)
            .Executes(() => {
                // If we are building in Debug AND Pack is scheduled, modify the props file BEFORE compilation starts
                if (Configuration == Configuration.Debug && ScheduledTargets.Contains(Pack)) {
                    UpdateVersion();
                }

                DotNetBuild(s => s.SetProjectFile(Solution)
                    .SetConfiguration(Configuration)
                    .EnableNoLogo()
                    .EnableNoRestore()
                    .EnableDeterministic()
                    .EnableContinuousIntegrationBuild());
            });

    Target Debug => def => def.Before(Compile).Triggers(Compile).Executes(() => Configuration = Configuration.Debug);

    Target Pack =>
        def => def.DependsOn(Compile)
            .After(Test)
            .Produces(PathToGeneratedPackages)
            .Executes(() => {
                PathToGeneratedPackages.CreateOrCleanDirectory();

                DotNetPack(s => s.SetProject(Solution)
                    .SetConfiguration(Configuration)
                    .SetOutputDirectory(PathToGeneratedPackages)
                    .EnableNoLogo()
                    .EnableNoRestore()
                    .EnableNoBuild()
                    .EnableContinuousIntegrationBuild());

                CopyPackagesToDevFolder();
            });

    AbsolutePath PathToArtifacts => RootDirectory / "Artifacts";

    AbsolutePath PathToCoverageReport => PathToArtifacts / "04 Coverage Report";

    AbsolutePath PathToCoverageResults => PathToArtifacts / "03 Coverage Results";

    AbsolutePath PathToGeneratedPackages => PathToArtifacts / "05 Packages";

    AbsolutePath PathToSources => RootDirectory / "Source";

    AbsolutePath PathToTestOutput => PathToArtifacts / "01 Test Output";

    AbsolutePath PathToTestResults => PathToArtifacts / "02 Test Results";

    AbsolutePath PathToTests => RootDirectory / "Tests";

    Target Publish =>
        def => def.Requires(() => NugetApiKey)
            .DependsOn(Clean)
            .DependsOn(Test)
            .DependsOn(Pack)
            .WhenSkipped(DependencyBehavior.Execute)
            .Executes(() => { PublishTo(NugetApiUrl, NugetApiKey); });

    Target Restore =>
        def => def.Executes(() => {
            DotNetRestore(s => s.SetProjectFile(Solution));
        });

    Target Test =>
        def => def.DependsOn(Compile)
            .Produces(PathToTestOutput, PathToTestResults, PathToCoverageResults, PathToCoverageReport)
            .Executes(() => {
                PathToTestOutput.CreateOrCleanDirectory();
                PathToTestResults.CreateOrCleanDirectory();
                PathToCoverageResults.CreateOrCleanDirectory();
                PathToCoverageReport.CreateOrCleanDirectory();

                IEnumerable<Project> testProjects = Solution.GetAllProjects("*.Tests");

                foreach (Project project in testProjects) {
                    // Execute the test executable directly (bypasses VSTest overhead)
                    DotNetRun(s => s.SetProjectFile(project)
                        .SetFramework("net10.0")
                        .SetConfiguration(Configuration)
                        .EnableNoRestore()
                        .EnableNoBuild()
                        // Native switches feed directly into the built-in coverage engine
                        .SetApplicationArguments("--coverage",
                            "--coverage-output-format",
                            "cobertura",
                            "--results-directory",
                            $"{PathToCoverageResults}"));
                }

                ReportGenerator(s => s.SetTargetDirectory(PathToCoverageReport)
                    .SetFramework("net10.0")
                    .SetReports(PathToCoverageResults / "*.xml")
                    .SetReportTypes(ReportTypes.HtmlInline, ReportTypes.Badges)
                    .SetClassFilters("-*CodeGeneration*", "-*.g.*", "-*.Abbreviations*"));

                Log.Information("Coverage report generated at {Path}", PathToCoverageReport / "index.html");
            });

    void CopyPackagesToDevFolder() {
        string? devNugetFolder = Environment.GetEnvironmentVariable("NuGetDevPackages");
        if (string.IsNullOrWhiteSpace(devNugetFolder)) {
            Log.Information("[Fallout Pack] NuGetDevPackages environment variable not set. Skipping copy of generated packages.");
        } else {
            AbsolutePath nugetFolder = devNugetFolder;

            if (nugetFolder.DirectoryExists()) {
                Log.Information("[Fallout Pack] Copying packages to: {Path}", nugetFolder);

                PathToGeneratedPackages.GlobFiles("*.nupkg")
                    .ForEach(packagePath => {
                        AbsolutePath targetPath = nugetFolder / packagePath.Name;
                        packagePath.Copy(targetPath, ExistsPolicy.FileOverwrite);
                    });
            } else {
                Log.Warning("[Fallout Pack] NuGetDevPackages variable is defined but target path does not exist: {Path}", nugetFolder);
            }
        }
    }

    void PublishTo(string nugetSource, string nugetApiKey) =>
        DotNetNuGetPush(nuget => nuget.SetSource(nugetSource)
            .SetApiKey(nugetApiKey)
            .EnableNoServiceEndpoint()
            .EnableSkipDuplicate()
            .CombineWith(PathToGeneratedPackages.GlobFiles("*.nupkg"), (s, package) => s.SetTargetPath(package)));

    void UpdateVersion() {
        AbsolutePath propsFile = PathToSources / "Directory.Build.props";
        if (!propsFile.FileExists()) {
            throw new FileNotFoundException($"Could not find expected file layout at: {propsFile}");
        }

        XDocument doc = XDocument.Load(propsFile, LoadOptions.PreserveWhitespace);
        XElement? versionElement = doc.Descendants("Version").FirstOrDefault();
        if (versionElement is null) {
            XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";
            versionElement = doc.Descendants(ns + "Version").FirstOrDefault();
        }

        if (versionElement is null) {
            throw new InvalidOperationException("Could not find <Version> tag in Directory.Build.props");
        }

        string rawVersion = versionElement.Value.Trim();
        if (Version.TryParse(rawVersion, out Version? currentVersion)) {
            // System.Version structure: major.minor[.build[.revision]]
            // but we use the SymVer model
            int newRevision = (currentVersion.Build >= 0 ? currentVersion.Build : 0) + 1;
            string updatedVersion = $"{currentVersion.Major}.{currentVersion.Minor}.{newRevision}";
            versionElement.Value = updatedVersion;
            doc.Save(propsFile);

            Log.Information("[Fallout Build] Incremented Directory.Build.props version to: {Version}", updatedVersion);
        } else {
            throw new FormatException($"The value '{rawVersion}' inside your Directory.Build.props is not a valid System.Version string.");
        }
    }

    public static int Main() => Execute<Build>(run => run.Test, run => run.Pack);
}

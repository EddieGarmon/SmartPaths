using System.Collections.Generic;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.ReportGenerator;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.ReportGenerator.ReportGeneratorTasks;

[ShutdownDotNetAfterServerBuild]
class Build : NukeBuild
{

    [Solution("SmartPaths.sln")] readonly Solution Solution;

    [Parameter("Configuration to build - Default is 'Release' (server)")] Configuration Configuration = Configuration.Release;

    [Parameter] string NugetApiKey;

    Target Clean =>
        def => def.Before(Restore)
                  .Executes(() =>
                            {
                                PathToSources.GlobDirectories("**/bin", "**/obj").ForEach(path => path.DeleteDirectory());
                                PathToTests.GlobDirectories("**/bin", "**/obj").ForEach(path => path.DeleteDirectory());
                                PathToArtifacts.CreateOrCleanDirectory();
                            });

    Target Compile =>
        def => def.DependsOn(Restore)
                  .Executes(() =>
                            {
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
                  .Executes(() =>
                            {
                                DotNetPack(s => s.SetProject(Solution)
                                                 .SetConfiguration(Configuration)
                                                 .SetOutputDirectory(PathToGeneratedPackages)
                                                 .EnableNoLogo()
                                                 .EnableNoRestore()
                                                 .EnableNoBuild()
                                                 .EnableContinuousIntegrationBuild());
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
                  .Executes(() =>
                            {
                                DotNetNuGetPush(nuget => nuget.SetSource("nuget.org")
                                                              .SetApiKey(NugetApiKey)
                                                              .EnableNoServiceEndpoint()
                                                              .EnableSkipDuplicate()
                                                              .CombineWith(PathToGeneratedPackages.GlobFiles("*.nupkg"),
                                                                           (s, package) => s.SetTargetPath(package)));
                            });

    Target Restore =>
        def => def.Executes(() =>
                            {
                                DotNetRestore(s => s.SetProjectFile(Solution));
                            });

    Target Test =>
        def => def.DependsOn(TestExecution)
                  .DependsOn(TestReporting);

    Target TestExecution =>
        def => def.Unlisted()
                  .DependsOn(Compile)
                  .Produces(PathToTestOutput, PathToTestResults, PathToCoverageResults, PathToCoverageReport)
                  .Executes(() =>
                            {
                                PathToTestOutput.CreateOrCleanDirectory();
                                PathToTestResults.CreateOrCleanDirectory();
                                PathToCoverageResults.CreateOrCleanDirectory();
                                PathToCoverageReport.CreateOrCleanDirectory();

                                List<Project> testProjects = Solution.GetAllProjects("*.Tests").ToList();

                                DotNetTest(cfg => cfg.EnableNoLogo()
                                                     .EnableNoBuild()
                                                     .SetConfiguration(Configuration)
                                                     .SetResultsDirectory(PathToTestOutput)
                                                     .SetDataCollector("XPlat Code Coverage")
                                                     .CombineWith(testProjects,
                                                                  (s, p) => s.SetProjectFile(p)
                                                                             .SetLoggers($"trx;LogFileName={p.Name}.trx")));
                            });

    Target TestReporting =>
        def => def.Unlisted()
                  .DependsOn(TestExecution)
                  .AssuredAfterFailure()
                  .Executes(() =>
                            {
                                foreach (AbsolutePath path in PathToTestOutput.GlobFiles("*.trx")) {
                                    TrxHelper helper = new(path);
                                    //move results.trx file to test results
                                    helper.TrxFilepath.MoveToDirectory(PathToTestResults);
                                    //move coverage.xml file to coverage results folder
                                    helper.CoverageSource.Move(PathToCoverageResults / helper.CoverageDestination);
                                }

                                ReportGenerator(s => s.SetTargetDirectory(PathToCoverageReport)
                                                      .SetFramework("net8.0")
                                                      .SetReportTypes(ReportTypes.Html)
                                                      .SetReports(PathToCoverageResults / "*.xml"));
                            });

    public static int Main() => Execute<Build>(run => run.Test, run => run.Pack);

}
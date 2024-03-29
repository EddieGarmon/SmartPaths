using System.Xml.Linq;
using Nuke.Common.IO;

class TrxHelper
{

    public TrxHelper(AbsolutePath trxFilepath) {
        TrxFilepath = trxFilepath;
        XElement root = XElement.Load(trxFilepath);
        //Log.Information($"root= {root.Name}");

        XElement testSettings = root.Element("{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}TestSettings");
        //Log.Information($"testSettings= {testSettings}");
        XElement deployment = testSettings.Element("{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}Deployment");
        //Log.Information($"deployment= {deployment}");
        string deploymentRoot = deployment.FirstAttribute.Value;
        //Log.Information($"deploymentRoot= {deploymentRoot}");

        XElement resultSummary = root.Element("{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}ResultSummary");
        //Log.Information($"resultSummary= {resultSummary}");
        XElement cde = resultSummary.Element("{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}CollectorDataEntries");
        //Log.Information($"cde= {cde}");
        XElement collector = cde.Element("{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}Collector");
        //Log.Information($"collector= {collector}");
        string computerName = collector.Attribute("agentName").Value;
        //Log.Information($"computer= {computerName}");

        CoverageSource = trxFilepath.Parent / deploymentRoot / "In" / computerName / "coverage.cobertura.xml";
        CoverageDestination = (RelativePath)(trxFilepath.NameWithoutExtension + ".cobertura.xml");
    }

    public RelativePath CoverageDestination { get; }

    public AbsolutePath CoverageSource { get; }

    public AbsolutePath TrxFilepath { get; }

}
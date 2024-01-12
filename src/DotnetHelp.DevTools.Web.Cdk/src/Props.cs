using Amazon.CDK;

namespace DotnetHelp.DevTools.Web.Cdk;

public class Props : StackProps
{
    public string CertificateArn { get; set; }
    public string CustomDomain { get; set; }
}
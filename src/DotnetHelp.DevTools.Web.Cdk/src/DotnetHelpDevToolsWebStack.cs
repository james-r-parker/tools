using Amazon.CDK;
using Amazon.CDK.AWS.CloudFront;
using Amazon.CDK.AWS.CloudFront.Origins;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.S3.Deployment;
using Constructs;

namespace DotnetHelp.DevTools.Web.Cdk;

public class DotnetHelpDevToolsWebStack : Stack
{
    internal DotnetHelpDevToolsWebStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
    {
        var s3Bucket = new Bucket(this, "WebBucket", new BucketProps());

        var cloudfront = new Distribution(this, "WebDistribution", new DistributionProps
        {
            DefaultBehavior = new BehaviorOptions
            {
                Origin = new S3Origin(s3Bucket),
                ViewerProtocolPolicy = ViewerProtocolPolicy.HTTPS_ONLY,
                Compress = true,
                AllowedMethods = AllowedMethods.ALLOW_GET_HEAD,
            },
            DefaultRootObject = "index.html",
            HttpVersion = HttpVersion.HTTP2_AND_3,
            EnableIpv6 = true,
            MinimumProtocolVersion = SecurityPolicyProtocol.TLS_V1_2_2021,
            PriceClass = PriceClass.PRICE_CLASS_100,
        });

        cloudfront.AddBehavior(
            "/api/*",
            new HttpOrigin("prm6cs2gduogih4yecimgqqeae0ilqkw.lambda-url.eu-west-2.on.aws", new HttpOriginProps
            {
                ProtocolPolicy = OriginProtocolPolicy.HTTPS_ONLY
            }),
            new BehaviorOptions
            {
                ViewerProtocolPolicy = ViewerProtocolPolicy.HTTPS_ONLY,
                Compress = true,
                AllowedMethods = AllowedMethods.ALLOW_ALL,
            });

        new BucketDeployment(this, "DeployWebsite", new BucketDeploymentProps
        {
            DestinationBucket = s3Bucket,
            Distribution = cloudfront,
            Sources = new ISource[]
            {
                Source.Asset("../DotnetHelp.DevTools.Web/out/wwwroot")
            }
        });

        new CfnOutput(this, "WWW", new CfnOutputProps
        {
            Value = cloudfront.DomainName
        });
    }
}
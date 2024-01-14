using System;
using Amazon.CDK;
using Amazon.CDK.AWS.CertificateManager;
using Amazon.CDK.AWS.CloudFront;
using Amazon.CDK.AWS.CloudFront.Origins;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.S3.Deployment;
using Constructs;
using StorageClass = Amazon.CDK.AWS.S3.StorageClass;

namespace DotnetHelp.DevTools.Web.Cdk;

public class DotnetHelpDevToolsWebStack : Stack
{
    internal DotnetHelpDevToolsWebStack(Construct scope, string id, Props props) : base(scope, id, props)
    {
        var s3Bucket = new Bucket(this, "WebBucket", new BucketProps());

        var logBucket = new Bucket(this, "WebLogsBucket", new BucketProps
        {
            RemovalPolicy = RemovalPolicy.DESTROY,
            AutoDeleteObjects = true,
            LifecycleRules = new[]
            {
                new LifecycleRule
                {
                    Enabled = true,
                    NoncurrentVersionExpiration = Duration.Days(30),
                    Expiration = Duration.Days(30),
                    AbortIncompleteMultipartUploadAfter = Duration.Days(1),
                }
            },
            Encryption = BucketEncryption.S3_MANAGED,
            AccessControl = BucketAccessControl.LOG_DELIVERY_WRITE,
            ObjectOwnership = ObjectOwnership.OBJECT_WRITER
        });

        var cloudfront = new Distribution(this, "WebDistribution", new DistributionProps
        {
            DefaultBehavior = new BehaviorOptions
            {
                Origin = new S3Origin(s3Bucket),
                ViewerProtocolPolicy = ViewerProtocolPolicy.HTTPS_ONLY,
                Compress = true,
                AllowedMethods = AllowedMethods.ALLOW_GET_HEAD,
                FunctionAssociations = new IFunctionAssociation[]
                {
                    new FunctionAssociation()
                    {
                        EventType = FunctionEventType.VIEWER_REQUEST,
                        Function = new Function(this, "RewriteFunction", new FunctionProps
                        {
                            Runtime =FunctionRuntime.JS_2_0,
                            Comment = "DotnetHelp Rewrite Function",
                            Code = FunctionCode.FromInline(@"
                                function handler(event) {
                                    var request = event.request;
                                    var uri = request.uri;
                                      
                                    if (uri.endsWith('/') || !uri.includes('.')) {
                                      request.uri = '/index.html';
                                    }

                                    return request;
                                }
                            "),
                        })
                    }
                },
            },
            DefaultRootObject = "index.html",
            HttpVersion = HttpVersion.HTTP2_AND_3,
            EnableIpv6 = true,
            MinimumProtocolVersion = SecurityPolicyProtocol.TLS_V1_2_2021,
            PriceClass = PriceClass.PRICE_CLASS_100,
            Certificate = Certificate.FromCertificateArn(this, "Certificate", props.CertificateArn),
            DomainNames = new[] { props.CustomDomain },
            EnableLogging = true,
            LogBucket = logBucket,
            LogFilePrefix = "web/",
        });

        cloudfront.AddBehavior(
            "/api/*",
            new HttpOrigin(Fn.Select(2, Fn.Split("/", Fn.ImportValue("DOTNETHELP:DEVTOOLS:API:URL"))),
                new HttpOriginProps
                {
                    ProtocolPolicy = OriginProtocolPolicy.HTTPS_ONLY
                }),
            new BehaviorOptions
            {
                ViewerProtocolPolicy = ViewerProtocolPolicy.HTTPS_ONLY,
                Compress = true,
                AllowedMethods = AllowedMethods.ALLOW_ALL,
                OriginRequestPolicy = new OriginRequestPolicy(this, "ApiOriginRequestPolicy",
                    new OriginRequestPolicyProps
                    {
                        CookieBehavior = OriginRequestCookieBehavior.None(),
                        HeaderBehavior = OriginRequestHeaderBehavior.All(),
                        QueryStringBehavior = OriginRequestQueryStringBehavior.All(),
                        Comment = "DotnetHelp Api Origin Request Policy"
                    }),
                CachePolicy = new CachePolicy(this, "ApiCachePolicy", new CachePolicyProps
                {
                    Comment = "DotnetHelp Api Cache Policy",
                    CookieBehavior = CacheCookieBehavior.None(),
                    HeaderBehavior = CacheHeaderBehavior.AllowList("Origin", "Accept"),
                    QueryStringBehavior = CacheQueryStringBehavior.All(),
                    EnableAcceptEncodingGzip = true,
                    EnableAcceptEncodingBrotli = true,
                    MinTtl = Duration.Seconds(0),
                    MaxTtl = Duration.Seconds(30),
                    DefaultTtl = Duration.Seconds(0),
                })
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
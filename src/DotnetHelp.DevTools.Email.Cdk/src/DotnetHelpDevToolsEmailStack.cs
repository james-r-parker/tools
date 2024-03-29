using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.S3.Notifications;
using Amazon.CDK.AWS.SES;
using Amazon.CDK.AWS.SES.Actions;
using Constructs;

namespace DotnetHelp.DevTools.Email.Cdk;

public class DotnetHelpDevToolsEmailStack : Stack
{
    internal DotnetHelpDevToolsEmailStack(Construct scope, string id, Props props) : base(scope, id,
        props)
    {
        var emailBucket = new Bucket(this, "EmailBucket", new BucketProps
        {
            RemovalPolicy = RemovalPolicy.DESTROY,
            BlockPublicAccess = BlockPublicAccess.BLOCK_ALL,
            Encryption = BucketEncryption.S3_MANAGED,
            LifecycleRules = new ILifecycleRule[]
            {
                new LifecycleRule
                {
                    Enabled = true,
                    Expiration = Duration.Days(30),
                    Id = "DeleteOldVersions",
                }
            }
        });

        emailBucket.GrantWrite(new ServicePrincipal("ses.amazonaws.com"));

        var functionRole = new Role(this, "EmailRole", new RoleProps
        {
            AssumedBy = new ServicePrincipal("lambda.amazonaws.com"),
            ManagedPolicies = new IManagedPolicy[]
            {
                ManagedPolicy.FromAwsManagedPolicyName("service-role/AWSLambdaBasicExecutionRole"),
                ManagedPolicy.FromManagedPolicyName(this, "DB_POLICY",
                    Fn.ImportValue("DOTNETHELP:DEVTOOLS:INFRASTRUCTURE:DB:POLICY")),
                ManagedPolicy.FromManagedPolicyName(this, "WSS_POLICY",
                    Fn.ImportValue("DOTNETHELP:DEVTOOLS:WSS:POLICY"))
            },
        });
        
        emailBucket.GrantRead(functionRole);

        var emailFunction = new Function(this, "EmailFunction", new FunctionProps
        {
            Architecture = Architecture.X86_64,
            Runtime = Runtime.PROVIDED_AL2023,
            MemorySize = 256,
            Role = functionRole,
            Description = "DotnetHelp.DevTools.Email",
            Handler = "bootstrap",
            Code = Code.FromAsset("../email/"),
            Timeout = Duration.Seconds(20),
            ReservedConcurrentExecutions = 10,
            Environment = new Dictionary<string, string>
            {
                { "WEBSOCKET_URL", Fn.ImportValue("DOTNETHELP:DEVTOOLS:WSS:URL") },
                { "CONNECTION_TABLE_NAME", Fn.ImportValue("DOTNETHELP:DEVTOOLS:WSS:TABLE") },
                { "DB_TABLE_NAME", Fn.ImportValue("DOTNETHELP:DEVTOOLS:INFRASTRUCTURE:DB:TABLE") },
                { "EMAIL_BUCKET", emailBucket.BucketName },
                { "AWS_STS_REGIONAL_ENDPOINTS", "regional" },
                { "ASPNETCORE_ENVIRONMENT", "Release" },
            }
        });
        
        
        //Trigger the lambda function when a new object is created in the bucket
        emailBucket.AddEventNotification(EventType.OBJECT_CREATED, new LambdaDestination(emailFunction));

        _ = new ReceiptRuleSet(this, "EmailRuleSet", new ReceiptRuleSetProps
        {
            DropSpam = true,
            ReceiptRuleSetName = "DotnetHelp.DevTools.Email",
            Rules = new IReceiptRuleOptions[]
            {
                new ReceiptRuleOptions
                {
                    Recipients = new[] { "tools.dotnethelp.co.uk" },
                    Enabled = true,
                    ScanEnabled = false,
                    Actions = new IReceiptRuleAction[]
                    {
                        new S3(new S3Props()
                        {
                            Bucket = emailBucket,
                            ObjectKeyPrefix = "emails/"
                        })
                    },
                    TlsPolicy = TlsPolicy.REQUIRE,
                },
            },
        });
    }
}
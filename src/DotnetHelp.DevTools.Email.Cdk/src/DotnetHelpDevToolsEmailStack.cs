using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.DynamoDB;
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
        var emailTable = new Table(this, "EmailTable", new TableProps()
        {
            PartitionKey = new Attribute() { Name = "messageId", Type = AttributeType.STRING },
            BillingMode = BillingMode.PAY_PER_REQUEST,
            RemovalPolicy = RemovalPolicy.DESTROY,
            Encryption = TableEncryption.AWS_MANAGED,
            PointInTimeRecovery = false,
            TimeToLiveAttribute = "ttl",
        });

        var emailBucket = new Bucket(this, "EmailBucket", new BucketProps
        {
            RemovalPolicy = RemovalPolicy.DESTROY,
            BlockPublicAccess = BlockPublicAccess.BLOCK_ALL,
            Encryption = BucketEncryption.S3_MANAGED,
            LifecycleRules = new[]
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
                ManagedPolicy.FromManagedPolicyName(this, "WSS_API_POLICY",
                    Fn.ImportValue("DOTNETHELP:DEVTOOLS:WSS:API:POLICY"))
            },
        });

        var emailPolicy = new ManagedPolicy(this, "EmailPolicy", new ManagedPolicyProps());
        emailTable.GrantReadData(emailPolicy);
        emailBucket.GrantRead(emailPolicy);

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
                { "EMAIL_TABLE_NAME", emailTable.TableName },
                { "EMAIL_BUCKET", emailBucket.BucketName },
                { "AWS_STS_REGIONAL_ENDPOINTS", "regional" }
            }
        });

        emailTable.GrantReadWriteData(functionRole);
        emailBucket.GrantReadWrite(functionRole);

        //Trigger the lambda function when a new object is created in the bucket
        emailBucket.AddEventNotification(EventType.OBJECT_CREATED, new LambdaDestination(emailFunction));

        new ReceiptRuleSet(this, "EmailRuleSet", new ReceiptRuleSetProps
        {
            DropSpam = true,
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

        new CfnOutput(this, "EMAIL_TABLE", new CfnOutputProps
        {
            Value = emailTable.TableName,
            ExportName = "DOTNETHELP:DEVTOOLS:EMAIL:TABLE",
        });

        new CfnOutput(this, "EMAIL_BUCKET", new CfnOutputProps
        {
            Value = emailBucket.BucketName,
            ExportName = "DOTNETHELP:DEVTOOLS:EMAIL:BUCKET",
        });

        new CfnOutput(this, "EMAIL_DB_POLICY", new CfnOutputProps
        {
            Value = emailPolicy.ManagedPolicyName,
            ExportName = "DOTNETHELP:DEVTOOLS:EMAIL:DB:POLICY",
        });
    }
}
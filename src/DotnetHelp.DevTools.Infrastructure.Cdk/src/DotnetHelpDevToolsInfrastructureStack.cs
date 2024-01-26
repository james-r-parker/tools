using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.CodeDeploy;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Constructs;

namespace DotnetHelp.DevTools.Infrastructure.Cdk;

public class DotnetHelpDevToolsInfrastructureStack : Stack
{
    internal DotnetHelpDevToolsInfrastructureStack(Construct scope, string id, Props props) : base(scope, id, props)
    {
        var binTable = new Table(this, "BinTable", new TableProps()
        {
            PartitionKey = new Attribute() { Name = "bucket", Type = AttributeType.STRING },
            SortKey = new Attribute() { Name = "created", Type = AttributeType.NUMBER },
            BillingMode = BillingMode.PAY_PER_REQUEST,
            RemovalPolicy = RemovalPolicy.DESTROY,
            Encryption = TableEncryption.AWS_MANAGED,
            PointInTimeRecovery = false,
            TimeToLiveAttribute = "ttl",
        });

        binTable.AddLocalSecondaryIndex(new LocalSecondaryIndexProps()
        {
            SortKey = new Attribute() { Name = "slug", Type = AttributeType.STRING },
            IndexName = "ix_bucket_slug",
        });
        
        var cacheTable = new Table(this, "APICacheTable", new TableProps()
        {
            PartitionKey = new Attribute() { Name = "id", Type = AttributeType.STRING },
            BillingMode = BillingMode.PAY_PER_REQUEST,
            RemovalPolicy = RemovalPolicy.DESTROY,
            Encryption = TableEncryption.AWS_MANAGED,
            PointInTimeRecovery = false,
            TimeToLiveAttribute = "ttl",
        });
        
        var dataPolicy = new ManagedPolicy(this, "DatabasePolicy", new ManagedPolicyProps());
        binTable.GrantReadWriteData(dataPolicy);
        cacheTable.GrantReadWriteData(dataPolicy);
        
        new CfnOutput(this, "BIN_TABLE", new CfnOutputProps
        {
            Value = binTable.TableName,
            ExportName = "DOTNETHELP:DEVTOOLS:INFRASTRUCTURE:BIN:TABLE",
        });
        
        new CfnOutput(this, "CACHE_TABLE", new CfnOutputProps
        {
            Value = cacheTable.TableName,
            ExportName = "DOTNETHELP:DEVTOOLS:INFRASTRUCTURE:CACHE:TABLE",
        });

        new CfnOutput(this, "BIN_DB_POLICY", new CfnOutputProps
        {
            Value = dataPolicy.ManagedPolicyName,
            ExportName = "DOTNETHELP:DEVTOOLS:INFRASTRUCTURE:DB:POLICY",
        });
    }
}
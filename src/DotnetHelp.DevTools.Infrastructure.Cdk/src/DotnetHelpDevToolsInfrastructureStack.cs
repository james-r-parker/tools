using Amazon.CDK;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.IAM;
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
        
        var db = new Table(this, "DnhDb", new TableProps()
        {
            PartitionKey = new Attribute() { Name = "bucket", Type = AttributeType.STRING },
            SortKey = new Attribute() { Name = "created", Type = AttributeType.NUMBER },
            BillingMode = BillingMode.PAY_PER_REQUEST,
            RemovalPolicy = RemovalPolicy.DESTROY,
            Encryption = TableEncryption.AWS_MANAGED,
            PointInTimeRecovery = false,
            TimeToLiveAttribute = "ttl",
        });

        db.AddLocalSecondaryIndex(new LocalSecondaryIndexProps()
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
        db.GrantReadWriteData(dataPolicy);
        
        _ = new CfnOutput(this, "DB_TABLE", new CfnOutputProps
        {
            Value = db.TableName,
            ExportName = "DOTNETHELP:DEVTOOLS:INFRASTRUCTURE:DB:TABLE",
        });
        
        _ = new CfnOutput(this, "BIN_TABLE", new CfnOutputProps
        {
            Value = binTable.TableName,
            ExportName = "DOTNETHELP:DEVTOOLS:INFRASTRUCTURE:BIN:TABLE",
        });
        
        _ = new CfnOutput(this, "CACHE_TABLE", new CfnOutputProps
        {
            Value = cacheTable.TableName,
            ExportName = "DOTNETHELP:DEVTOOLS:INFRASTRUCTURE:CACHE:TABLE",
        });

        _ = new CfnOutput(this, "BIN_DB_POLICY", new CfnOutputProps
        {
            Value = dataPolicy.ManagedPolicyName,
            ExportName = "DOTNETHELP:DEVTOOLS:INFRASTRUCTURE:DB:POLICY",
        });
    }
}
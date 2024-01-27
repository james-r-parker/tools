using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.S3;
using DotnetHelp.DevTools.Email.Handler;
using DotnetHelp.DevTools.Shared;
using DotnetHelp.DevTools.WebsocketClient;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace DotnetHelp.DevTools.Email.Tests;

public class EmailStorageTests
{
    private const string S3Bucket = "test-bucket";
    private const string S3Key = "incoming/test-object.eml";
    private const string TableName = "test-table";

    private readonly Mock<IAmazonDynamoDB> _db;
    private readonly Mock<IAmazonS3> _s3;
    private readonly Mock<IWebsocketClient> _wss;
    private readonly EmailStorage _sut;

    public EmailStorageTests()
    {
        Environment.SetEnvironmentVariable(
            "DB_TABLE_NAME",
            TableName,
            EnvironmentVariableTarget.Process);

        _db = new Mock<IAmazonDynamoDB>();
        _s3 = new Mock<IAmazonS3>();
        _wss = new Mock<IWebsocketClient>();
        _sut = new EmailStorage(_db.Object, _s3.Object, _wss.Object, new NullLogger<EmailStorage>());
    }

    [Fact]
    public async Task Store()
    {
        _s3.Setup(x => x.GetObjectAsync(S3Bucket, S3Key, default))
            .ReturnsAsync(new Amazon.S3.Model.GetObjectResponse
            {
                HttpStatusCode = System.Net.HttpStatusCode.OK,
                ResponseStream = File.OpenRead("test-data/test-object.eml")
            });

        await _sut.ProcessEmailAsync(S3Bucket, S3Key, default);
        
        _db.Verify(x =>
            x.PutItemAsync(It.Is<PutItemRequest>(y => y.TableName == TableName && y.Item["bucket"].S == "incoming_email-3adeb671803e48159a58af7dfbc2b461"),
                default), Times.Once);
        
        _wss.Verify(x => x.SendMessage(It.Is<WebSocketMessage>(y => y.Action == "INCOMING_EMAIL" && y.Bucket == "3adeb671803e48159a58af7dfbc2b461"), default), Times.Once);
    }
}
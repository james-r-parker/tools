# Developer Tools - Leveraging dotnet C# and AWS Skills

This is a sample application crafted in C# using dotnet core, designed to showcase cutting-edge technologies.

## Demo
Explore the application at [www.dotnethelp.co.uk](https://www.dotnethelp.co.uk).

## API
The heart of this project is its REST API, written in C# using dotnet 8 and incorporating [AOT compilation](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/). AOT compilation significantly enhances the cold boot speeds of the application. The API is hosted as an [AWS Lambda](https://aws.amazon.com/lambda/) function, leveraging the capabilities of [AL2023](https://docs.aws.amazon.com/linux/al2023/ug/what-is-amazon-linux.html).

## Websockets
To facilitate real-time updates stemming from asynchronous operations, the frontend relies on websockets. The management of these websockets is entrusted to an AWS Lambda function, written in C#. Connection details are securely stored in an [AWS DynamoDB Table](https://docs.aws.amazon.com/dynamodb/). This Lambda function operates behind an [AWS API Gateway Websocket](https://docs.aws.amazon.com/apigateway/latest/developerguide/apigateway-websocket-api.html) Endpoint.

## Front End
The frontend of this application, a standalone [Blazor WebAssembly](https://dotnet.microsoft.com/en-us/apps/aspnet/web-apps/blazor) application, is again coded in C#. Tailwind provides the stylish touch, and communication with the API is achieved through shared models and HttpClients. The frontend resides in an [AWS S3 Bucket](https://aws.amazon.com/s3/) with the added efficiency of a [Cloudfront Distribution](https://docs.aws.amazon.com/AmazonCloudFront/latest/DeveloperGuide/distribution-overview.html) for optimal caching benefits.

## Searching
Empowering users to swiftly find the desired tools, the frontend seamlessly integrates with [Algolia](https://www.algolia.com/) search. The frontend application directly calls Algolia from the frontend, delivering a lightning-fast search experience.

## Building
[GitHub actions](https://docs.github.com/en/actions) take the lead in building and deploying this project. The entire infrastructure ([IAC](https://aws.amazon.com/what-is/iac/)) is managed as code through the [AWS CDK](https://aws.amazon.com/cdk/). AWS Code Deploy, triggered by the CDK, handles API deployments, offering the flexibility of Canary deployments with automatic rollback capabilities in case of Pre Traffic Hook failures.

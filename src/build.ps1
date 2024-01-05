Remove-Item -Path ".\app" -Recurse
Remove-Item -Path ".\Build.zip"
docker build . -f .\DotnetHelp.DevTools.Api\Dockerfile -t api_aot_build --output=. --target=final
Rename-Item -Path ".\app\DotnetHelp.DevTools.Api" -NewName "bootstrap"

$compress = @{
  Path = "./app/**"
  CompressionLevel = "Fastest"
  DestinationPath = "./Build.zip"
}
Compress-Archive @compress

aws lambda update-function-code --function-name test --zip-file fileb://Build.zip
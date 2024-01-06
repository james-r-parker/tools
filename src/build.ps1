Remove-Item -Path ".\app" -Recurse -ErrorAction SilentlyContinue
Remove-Item -Path ".\Build.zip" -ErrorAction SilentlyContinue
docker build . -f .\DotnetHelp.DevTools.Api\Dockerfile -t api_aot_build --output=. --target=final
Rename-Item -Path ".\app\DotnetHelp.DevTools.Api" -NewName "bootstrap"
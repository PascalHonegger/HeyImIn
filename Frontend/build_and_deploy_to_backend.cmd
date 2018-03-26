del /S /F /Q ..\Backend\WebApplication\wwwroot\assets
del /F /Q ..\Backend\WebApplication\wwwroot\*.css
del /F /Q ..\Backend\WebApplication\wwwroot\*.eot
del /F /Q ..\Backend\WebApplication\wwwroot\*.html
del /F /Q ..\Backend\WebApplication\wwwroot\*.jpg
del /F /Q ..\Backend\WebApplication\wwwroot\*.js
del /F /Q ..\Backend\WebApplication\wwwroot\*.map
del /F /Q ..\Backend\WebApplication\wwwroot\*.svg
del /F /Q ..\Backend\WebApplication\wwwroot\*.ttf
del /F /Q ..\Backend\WebApplication\wwwroot\*.txt
del /F /Q ..\Backend\WebApplication\wwwroot\*.woff
del /F /Q ..\Backend\WebApplication\wwwroot\*.woff2

npm run build:aot:prod

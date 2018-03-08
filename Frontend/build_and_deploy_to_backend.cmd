del /S /F /Q ..\Backend\WebApplication\client\assets
del /F /Q ..\Backend\WebApplication\client\*.css
del /F /Q ..\Backend\WebApplication\client\*.eot
del /F /Q ..\Backend\WebApplication\client\*.html
del /F /Q ..\Backend\WebApplication\client\*.jpg
del /F /Q ..\Backend\WebApplication\client\*.js
del /F /Q ..\Backend\WebApplication\client\*.map
del /F /Q ..\Backend\WebApplication\client\*.svg
del /F /Q ..\Backend\WebApplication\client\*.ttf
del /F /Q ..\Backend\WebApplication\client\*.txt
del /F /Q ..\Backend\WebApplication\client\*.woff
del /F /Q ..\Backend\WebApplication\client\*.woff2

npm run build:aot:prod

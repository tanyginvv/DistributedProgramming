cd ..\Valuator\
start dotnet run --urls "http://localhost:5001"
start dotnet run --urls "http://localhost:5002"

cd ..\..\..\..\..\..\nginx-1.25.4
start nginx -c ..\..\..\..\..\..\nginx-1.25.4\conf\nginx.conf
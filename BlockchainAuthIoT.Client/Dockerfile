FROM mcr.microsoft.com/dotnet/aspnet:5.0
RUN mkdir /app
COPY ./bin/Release/net5.0/publish/ /app/
WORKDIR /app
CMD ["dotnet", "./BlockchainAuthIoT.Client.dll", "urls=http://*:4000"]
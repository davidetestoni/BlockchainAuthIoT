FROM mcr.microsoft.com/dotnet/runtime:5.0
RUN mkdir /app
COPY ./bin/Release/net5.0/publish/ /app/
WORKDIR /app
CMD ["dotnet", "./BlockchainAuthIoT.Device.dll"]
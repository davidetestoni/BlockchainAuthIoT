FROM mcr.microsoft.com/dotnet/runtime:5.0
RUN mkdir /app
WORKDIR /app
COPY ./bin/Release/net5.0/publish/* .
CMD ["dotnet", "./BlockchainAuthIoT.Device.dll"]
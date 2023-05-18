FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS publish
WORKDIR /src
COPY ./src ./

RUN dotnet restore "./SimpleMockServer/SimpleMockServer.csproj"
COPY . .
RUN dotnet publish "./SimpleMockServer/SimpleMockServer.csproj" -c Release -o /app/publish \
  --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine AS final

WORKDIR /app
EXPOSE 80
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "SimpleMockServer.dll"]
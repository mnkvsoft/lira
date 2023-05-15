FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS publish
WORKDIR /src
COPY ./src ./

RUN dotnet restore "./SimpleMockServer/SimpleMockServer.csproj" --runtime alpine-x64
COPY . .
RUN dotnet publish "./SimpleMockServer/SimpleMockServer.csproj" -c Release -o /app/publish \
  --no-restore \
  --runtime alpine-x64 \
  --self-contained true \
  /p:PublishTrimmed=true \
  /p:PublishSingleFile=true

FROM mcr.microsoft.com/dotnet/runtime-deps:7.0-alpine AS final

WORKDIR /app
EXPOSE 80
COPY --from=publish /app/publish .

ENTRYPOINT ["./SimpleMockServer"]
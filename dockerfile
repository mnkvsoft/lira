FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS publish
WORKDIR /src
COPY ./src ./

RUN dotnet restore "./Lira/Lira.csproj"
COPY . .
RUN dotnet publish "./Lira/Lira.csproj" -c Release -o /app/publish \
  --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine AS final
RUN apk add --no-cache tzdata

WORKDIR /app
EXPOSE 80
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Lira.dll"]
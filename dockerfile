FROM mcr.microsoft.com/dotnet/sdk:8.0 AS publish
WORKDIR /src
COPY ./src ./

RUN dotnet restore "./Lira/Lira.csproj"
COPY . .
RUN dotnet publish "./Lira/Lira.csproj" -c Release -o /app/publish \
  --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Lira.dll"]
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV CODE_DATA_PATH=/data/codes.json

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY backend/CodeShowcase.Api/ ./CodeShowcase.Api/
RUN dotnet restore CodeShowcase.Api/CodeShowcase.Api.csproj
RUN dotnet publish CodeShowcase.Api/CodeShowcase.Api.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
COPY data /data
ENTRYPOINT ["dotnet", "CodeShowcase.Api.dll"]


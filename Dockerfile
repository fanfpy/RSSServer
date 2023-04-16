#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 3243

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["RSS.Web/RSS.Web.csproj", "RSS.Web/"]
COPY ["RSS.Repository/RSS.Repository.csproj", "RSS.Repository/"]
COPY ["RSS.Util/RSS.Util.csproj", "RSS.Util/"]
COPY ["RSS.Model/RSS.Model.csproj", "RSS.Model/"]
COPY ["UpdateFeed/UpdateFeed.csproj", "UpdateFeed/"]
RUN dotnet restore "RSS.Web/RSS.Web.csproj"
COPY . .
WORKDIR "/src/RSS.Web"
RUN dotnet build "RSS.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RSS.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RSS.Web.dll"]
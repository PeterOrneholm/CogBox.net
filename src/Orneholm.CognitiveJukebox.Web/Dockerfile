#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["src/Orneholm.AIJukebox.Web/Orneholm.AIJukebox.Web.csproj", "Orneholm.AIJukebox.Web/"]
COPY ["src/Orneholm.AIJukebox.Core/Orneholm.AIJukebox.Core.csproj", "Orneholm.AIJukebox.Core/"]
RUN dotnet restore "Orneholm.AIJukebox.Web/Orneholm.AIJukebox.Web.csproj"
COPY /src .
WORKDIR "/src/Orneholm.AIJukebox.Web"
RUN dotnet build "Orneholm.AIJukebox.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Orneholm.AIJukebox.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Orneholm.AIJukebox.Web.dll"]

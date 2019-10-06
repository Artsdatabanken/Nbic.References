FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
ENV AuthAuthority=https://demo.identityserver.io
ENV AuthAuthorityEndPoint=https://demo.identityserver.io/connect/authorize
ENV AuthApiSecret="test-secret"
ENV ApiName=api
ENV DbProvider=Sqlite
ENV DbConnectionString="Data Source=references.db"

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["Nbic.References/Nbic.References.csproj", "Nbic.References/"]
COPY ["Nbic.References.Model/Nbic.References.Public.csproj", "Nbic.References.Model/"]
COPY ["Nbic.References.EFCore/Nbic.References.EFCore.csproj", "Nbic.References.EFCore/"]
RUN dotnet restore "Nbic.References/Nbic.References.csproj"
COPY . .
WORKDIR "/src/Nbic.References"
RUN dotnet build "Nbic.References.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Nbic.References.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Nbic.References.dll"]
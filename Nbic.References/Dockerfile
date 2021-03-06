FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
RUN sed -i 's/DEFAULT@SECLEVEL=2/DEFAULT@SECLEVEL=1/g' /etc/ssl/openssl.cnf
RUN sed -i 's/DEFAULT@SECLEVEL=2/DEFAULT@SECLEVEL=1/g' /usr/lib/ssl/openssl.cnf
EXPOSE 8000
ENV AuthAuthority=https://demo.identityserver.io
ENV AuthAuthorityEndPoint=https://demo.identityserver.io/connect/authorize
ENV ApiName=api
ENV WriteAccessRole="my_write_access_role"
ENV SwaggerClientId="implicit"
ENV DbProvider=Sqlite
ENV DbConnectionString="Data Source=references.db"
ENV ASPNETCORE_URLS="http://+:8000"

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Nbic.References/Nbic.References.csproj", "Nbic.References/"]
COPY ["Nbic.References.Model/Nbic.References.Public.csproj", "Nbic.References.Model/"]
COPY ["Nbic.References.EFCore/Nbic.References.EFCore.csproj", "Nbic.References.EFCore/"]
COPY ["Nbic.Indexer/Nbic.Indexer.csproj", "Nbic.Indexer/"]
RUN dotnet restore "Nbic.References/Nbic.References.csproj"
COPY . .
WORKDIR "/src/Nbic.References"
RUN dotnet build "Nbic.References.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Nbic.References.csproj" -c Release -o /app/publish

FROM base AS final
RUN groupadd -r --gid 1007 dockerrunner && useradd -r -g dockerrunner dockerrunner
WORKDIR /app
COPY --from=publish /app/publish .
run mkdir -m 777 /app/Data
USER dockerrunner
ENTRYPOINT ["dotnet", "Nbic.References.dll"]
#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
# temp fix ols ssl on sql server
RUN sed -i 's/DEFAULT@SECLEVEL=2/DEFAULT@SECLEVEL=1/g' /etc/ssl/openssl.cnf
RUN sed -i 's/DEFAULT@SECLEVEL=2/DEFAULT@SECLEVEL=1/g' /usr/lib/ssl/openssl.cnf
EXPOSE 80
EXPOSE 443

# environment variables
ENV AuthAuthority=https://demo.identityserver.io
ENV AuthAuthorityEndPoint=https://demo.identityserver.io/connect/authorize
ENV ApiName=api
ENV WriteAccessRole="my_write_access_role"
ENV SwaggerClientId="implicit"
ENV DbProvider=Sqlite
ENV DbConnectionString="Data Source=references.db"

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Nbic.References/Nbic.References.csproj", "Nbic.References/"]
COPY ["Nbic.References.Model/Nbic.References.Public.csproj", "Nbic.References.Model/"]
COPY ["Nbic.Indexer/Nbic.Indexer.csproj", "Nbic.Indexer/"]
COPY ["Nbic.References.EFCore/Nbic.References.EFCore.csproj", "Nbic.References.EFCore/"]
RUN dotnet restore "Nbic.References/Nbic.References.csproj"
COPY . .
WORKDIR "/src/Nbic.References"
RUN dotnet build "Nbic.References.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Nbic.References.csproj" -c Release -o /app/publish

FROM base AS final
# use port 8000 internally in final build
EXPOSE 8000
ENV ASPNETCORE_URLS="http://+:8000"
# use spesific user dockerunner with this spesific gid - not root 
RUN groupadd -r --gid 1007 dockerrunner && useradd -r -g dockerrunner dockerrunner
WORKDIR /app
COPY --from=publish /app/publish .
RUN mkdir -m 777 /app/Data
USER dockerrunner
ENTRYPOINT ["dotnet", "Nbic.References.dll"]
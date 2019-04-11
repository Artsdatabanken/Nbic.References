FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

ENV AuthAuthority=https://demo.identityserver.io
ENV AuthAuthorityEndPoint=https://demo.identityserver.io/connect/authorize
ENV AuthApiSecret="test-secret"
ENV ApiName=api
ENV DbProvider=Sqlite
ENV DbConnectionString="Data Source=referencesdb.db"

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["Nbic.References/Nbic.References.csproj", "Nbic.References/"]
RUN dotnet restore "Nbic.References/Nbic.References.csproj"
COPY . .
WORKDIR "/src/Nbic.References"
RUN dotnet build "Nbic.References.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Nbic.References.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Nbic.References.dll"]
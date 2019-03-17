FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["Nbic.Reference/Nbic.Reference.csproj", "Nbic.Reference/"]
RUN dotnet restore "Nbic.Reference/Nbic.Reference.csproj"
COPY . .
WORKDIR "/src/Nbic.Reference"
RUN dotnet build "Nbic.Reference.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Nbic.Reference.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Nbic.Reference.dll"]
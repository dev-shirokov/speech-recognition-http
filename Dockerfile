#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["speech-recognitive-http/speech-recognitive-http.csproj", "speech-recognitive-http/"]
RUN dotnet restore "speech-recognitive-http/speech-recognitive-http.csproj"
COPY . .
WORKDIR "/src/speech-recognitive-http"
RUN dotnet build "speech-recognitive-http.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "speech-recognitive-http.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "speech-recognitive-http.dll"]
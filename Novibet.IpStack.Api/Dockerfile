FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Novibet.IpStack.Api/Novibet.IpStack.Api.csproj", "Novibet.IpStack.Api/"]
RUN dotnet restore "Novibet.IpStack.Api/Novibet.IpStack.Api.csproj"
COPY . .
WORKDIR "/src/Novibet.IpStack.Api"
RUN dotnet build "Novibet.IpStack.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Novibet.IpStack.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Novibet.IpStack.Api.dll"]
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY ReverseProxy/*.csproj ./ReverseProxy/
RUN dotnet restore ./ReverseProxy/

# copy other source files and build app
COPY ReverseProxy/. ./ReverseProxy/
WORKDIR /source/ReverseProxy
RUN dotnet publish -c release -o /app --no-restore

# Build image
FROM mcr.microsoft.com/dotnet/aspnet:3.1
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "ReverseProxy.dll"]

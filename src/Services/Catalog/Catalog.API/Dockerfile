##See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
#
##FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
#
## This stage is used when running from VS in fast mode (Default for Debug configuration)
#
#FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
#WORKDIR /app
#EXPOSE 80
#
##FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
#
#FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
#WORKDIR /src
#COPY ["Services/Catalog/Catalog.API/Catalog.API.csproj", "Services/Catalog/Catalog.API/"]
#RUN dotnet restore "Services/Catalog/Catalog.API/Catalog.API.csproj"
#COPY . .
#WORKDIR "/src/Services/Catalog/Catalog.API"
#RUN dotnet build "Catalog.API.csproj" -c Release -o /app/build
#
## This stage is used to publish the service project to be copied to the final stage
#FROM build AS publish
#RUN dotnet publish "Catalog.API.csproj" -c Release -o /app/publish
#
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "Catalog.API.dll"]
#

# See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

# See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

# --- Base Stage ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# --- Build Stage ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies.
# IMPORTANT: Use relative path to the project file within the build context.
COPY ["Catalog.API.csproj", "./"]
#COPY ["src/Services/Catalog/Catalog.API/Catalog.API.csproj", "src/Services/Catalog/Catalog.API/"]
RUN dotnet restore "Catalog.API.csproj"

# Copy the rest of the application source code.
# IMPORTANT: Use relative paths to the source code within the build context.
COPY . .

# Set the working directory to the project directory.
WORKDIR "/src"

# Build the application in Release configuration.
RUN dotnet build "Catalog.API.csproj" -c Release -o /app/build

# --- Publish Stage ---
FROM build AS publish
RUN dotnet publish "Catalog.API.csproj" -c Release -o /app/publish

# --- Final Stage ---
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Catalog.API.dll"]
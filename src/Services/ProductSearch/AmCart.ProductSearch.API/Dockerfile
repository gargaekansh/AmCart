## See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.
#
## This stage is used when running from VS in fast mode (Default for Debug configuration)
#FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
#USER $APP_UID
#WORKDIR /app
#EXPOSE 8080
#EXPOSE 8081
##EXPOSE 80 
#
#
## This stage is used to build the service project
#FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
#ARG BUILD_CONFIGURATION=Release
#WORKDIR /src
#
#COPY ["Services/ProductSearch/AmCart.ProductSearch.API/AmCart.ProductSearch.API.csproj", "Services/ProductSearch/AmCart.ProductSearch.API/"]
#RUN dotnet restore "./Services/ProductSearch/AmCart.ProductSearch.API/AmCart.ProductSearch.API.csproj"
#COPY . .
#WORKDIR "/src/Services/ProductSearch/AmCart.ProductSearch.API"
#RUN dotnet build "./AmCart.ProductSearch.API.csproj" -c $BUILD_CONFIGURATION -o /app/build
#
## This stage is used to publish the service project to be copied to the final stage
#FROM build AS publish
#ARG BUILD_CONFIGURATION=Release
#RUN dotnet publish "./AmCart.ProductSearch.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
#
## This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "AmCart.ProductSearch.API.dll"]

#//////////////////////////////////////////////////////////

# --- Base Stage ---
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# --- Build Stage ---
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy the project file first (to optimize layer caching)
COPY ["AmCart.ProductSearch.API.csproj", "./"]
RUN dotnet restore "AmCart.ProductSearch.API.csproj"

# Copy the rest of the source code
COPY . .

# Set the working directory to the project directory
WORKDIR "/src"

# Build the application in Release configuration
RUN dotnet build "AmCart.ProductSearch.API.csproj" -c Release -o /app/build

# --- Publish Stage ---
FROM build AS publish
RUN dotnet publish "AmCart.ProductSearch.API.csproj" -c Release -o /app/publish

# --- Final Stage ---
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AmCart.ProductSearch.API.dll"]
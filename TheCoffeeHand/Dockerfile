# Base image for running the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Set environment variable for SQLite DB location inside the container
ENV ConnectionStrings__DefaultConnection="Data Source=/app/TheCoffeeHandDb.db"

# Build stage for compiling the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["TheCoffeeHand/TheCoffeeHand.csproj", "TheCoffeeHand/"]
COPY ["Services/Services.csproj", "Services/"]
COPY ["Services.Interfaces/Services.Interfaces.csproj", "Services.Interfaces/"]
COPY ["Repositories/Infrastructure.csproj", "Repositories/"]
COPY ["Interfracture/Domain.csproj", "Interfracture/"]
COPY ["Core/Core.csproj", "Core/"]
RUN dotnet restore "./TheCoffeeHand/TheCoffeeHand.csproj"

COPY . .
WORKDIR "/src/TheCoffeeHand"
RUN dotnet build "./TheCoffeeHand.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TheCoffeeHand.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime stage
FROM runtime AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Copy the database file from the main project folder into the container
COPY TheCoffeeHand/TheCoffeeHandDb.db /app/TheCoffeeHandDb.db

# Ensure SQLite database persists outside the container
VOLUME ["/app"]

ENTRYPOINT ["dotnet", "TheCoffeeHand.dll"]

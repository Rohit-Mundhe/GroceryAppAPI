FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

# Copy project file
COPY GroceryOrderingApp.Backend.csproj ./

# Restore dependencies with explicit package source
RUN dotnet restore --no-cache

# Copy entire project
COPY . .

# Restore dependencies with explicit package source
RUN dotnet restore --no-cache

# Build
RUN dotnet build -c Release -o /app/build

# Publish
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "GroceryOrderingApp.Backend.dll"]

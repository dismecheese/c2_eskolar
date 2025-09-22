# Use official .NET 9 image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file first for better caching
COPY ["c2_eskolar.csproj", "./"]
RUN dotnet restore "./c2_eskolar.csproj"

# Copy everything else
COPY . .
RUN dotnet publish "./c2_eskolar.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Set environment variables for production
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "c2_eskolar.dll"]
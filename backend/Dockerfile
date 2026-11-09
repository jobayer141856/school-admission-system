# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first for optimal caching
COPY ["src/SchoolAdmission.Domain/SchoolAdmission.Domain.csproj", "src/SchoolAdmission.Domain/"]
COPY ["src/SchoolAdmission.Application/SchoolAdmission.Application.csproj", "src/SchoolAdmission.Application/"]
COPY ["src/SchoolAdmission.Infrastructure/SchoolAdmission.Infrastructure.csproj", "src/SchoolAdmission.Infrastructure/"]
COPY ["src/SchoolAdmission.API/SchoolAdmission.API.csproj", "src/SchoolAdmission.API/"]
COPY ["tests/SchoolAdmission.UnitTests/SchoolAdmission.UnitTests.csproj", "tests/SchoolAdmission.UnitTests/"]
COPY ["SchoolAdmission.sln", "./"]

RUN dotnet restore "SchoolAdmission.sln"

# Copy remaining source code
COPY . .

# Run build
RUN dotnet build "SchoolAdmission.sln" -c Release --no-restore

# Publish API
RUN dotnet publish "src/SchoolAdmission.API/SchoolAdmission.API.csproj" -c Release -o /app/publish --no-build

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "SchoolAdmission.API.dll"]


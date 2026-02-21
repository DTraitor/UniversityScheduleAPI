# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY *.sln .
COPY API/*.csproj ./API/
COPY BusinessLogic/*.csproj ./BusinessLogic/
COPY DataAccess/*.csproj ./DataAccess/
COPY Common/*.csproj ./Common/
COPY APITests/*.csproj ./APITests/
COPY BusinessLogicTests/*.csproj ./BusinessLogicTests/
COPY DataAccessTests/*.csproj ./DataAccessTests/
COPY ScheduledJobs/*.csproj ./ScheduledJobs/

# Restore all dependencies
RUN dotnet restore

# Copy all source code
COPY . .

# Publish the API project
RUN dotnet publish API/API.csproj -c Release -o /app

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "API.dll"]

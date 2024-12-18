# Use the official .NET runtime image for running the appa
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

# Use the official .NET SDK image for building the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the .csproj file and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the application source code
COPY . ./

# Build the application
RUN dotnet publish -c Release -o /app/publish

# Copy JSON files
COPY json/config.json json/guildEvents.json json/i18n.json /app/publish/

# Final stage: Use the runtime image to run the app
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Set the entry point for the application
ENTRYPOINT ["dotnet", "tsom_bot.dll"]
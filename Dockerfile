# Use .NET runtime image for production
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use .NET SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AI_Chatbot.csproj", "."]
RUN dotnet restore "./AI_Chatbot.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./AI_Chatbot.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Install dotnet-ef CLI
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"

# Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AI_Chatbot.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Use final runtime image for deployment
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AI_Chatbot.dll"]

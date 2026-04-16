# Use official .NET 8 SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy all source code and publish release build
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# Use smaller runtime image for final deployment
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

# Expose port that Render expects
EXPOSE 10000
ENV ASPNETCORE_URLS=http://+:10000

# Entry point
ENTRYPOINT ["dotnet", "PcComponentsApi.dll"]
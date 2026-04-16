 официальный образ .NET 8 SDK для сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем проект и восстанавливаем зависимости
COPY *.csproj ./
RUN dotnet restore

#копирование всего кода и публикация релиза
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# Финальный образ для запуска (легче и безопаснее)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Открытие порта 80 (или 10000, как требует Render)
EXPOSE 10000
ENTRYPOINT ["dotnet", "PcComponentsApi.dll"]
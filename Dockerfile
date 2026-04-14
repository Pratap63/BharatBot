FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ChatBot/ChatBot.csproj", "ChatBot/"]
RUN dotnet restore "ChatBot/ChatBot.csproj"
COPY . .
WORKDIR "/src/ChatBot"
RUN dotnet build "ChatBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ChatBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChatBot.dll"]
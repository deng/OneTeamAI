FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY AI-Chatbot.sln ./
COPY Chatbot.Api/Chatbot.Api.csproj Chatbot.Api/
COPY Chatbot.Api.Tests/Chatbot.Api.Tests.csproj Chatbot.Api.Tests/
RUN dotnet restore Chatbot.Api/Chatbot.Api.csproj

COPY Chatbot.Api/. Chatbot.Api/
COPY Chatbot.Api.Tests/. Chatbot.Api.Tests/
RUN dotnet publish Chatbot.Api/Chatbot.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

RUN mkdir -p /app/data

COPY --from=build /app/publish ./

EXPOSE 5078

ENTRYPOINT ["dotnet", "Chatbot.Api.dll"]

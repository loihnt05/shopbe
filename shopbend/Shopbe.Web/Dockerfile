# syntax=docker/dockerfile:1

# ---- build ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first for better layer caching
COPY shopbend/Directory.Build.props ./shopbend/
COPY shopbend/Shopbe.Web/Shopbe.Web.csproj ./shopbend/Shopbe.Web/
COPY shopbend/Shopbe.Application/Shopbe.Application.csproj ./shopbend/Shopbe.Application/
COPY shopbend/Shopbe.Infrastructure/Shopbe.Infrastructure.csproj ./shopbend/Shopbe.Infrastructure/
COPY shopbend/Shopbe.Domain/Shopbe.Domain.csproj ./shopbend/Shopbe.Domain/

RUN dotnet restore ./shopbend/Shopbe.Web/Shopbe.Web.csproj

# Copy the rest of the source
COPY shopbend ./shopbend

RUN dotnet publish ./shopbend/Shopbe.Web/Shopbe.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---- runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish ./

ENTRYPOINT ["dotnet", "Shopbe.Web.dll"]


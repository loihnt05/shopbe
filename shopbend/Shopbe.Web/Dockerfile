# syntax=docker/dockerfile:1
# NOTE:
# This Dockerfile runs a pre-built publish output (see shopbend/Shopbe.Web/publish).
# It avoids restoring/building inside Docker, which can fail when the pinned SDK
# (global.json) does not exist in the container or when SDK resolution is restricted.
#
# To produce the publish output locally:
#   dotnet publish shopbend/Shopbe.Web/Shopbe.Web.csproj -c Release -o shopbend/Shopbe.Web/publish

# ---- runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
# Ensure Kestrel listens on the container port
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Copy pre-built publish output from the build context
COPY shopbend/Shopbe.Web/publish ./

ENTRYPOINT ["dotnet", "Shopbe.Web.dll"]

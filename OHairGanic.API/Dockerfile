# ===== Build =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj của từng project để cache restore
COPY OHairGanic.API/OHairGanic.API.csproj OHairGanic.API/
COPY OHairGanic.BLL/OHairGanic.BLL.csproj OHairGanic.BLL/
COPY OHairGanic.DAL/OHairGanic.DAL.csproj OHairGanic.DAL/
COPY OHairGanic.DTO/OHairGanic.DTO.csproj OHairGanic.DTO/

# Add packages to BLL project before restore to cache them
WORKDIR /src/OHairGanic.BLL
RUN dotnet add package Microsoft.ML.OnnxRuntime --version 1.19.2 && \
    dotnet add package SixLabors.ImageSharp --version 3.1.4

# Restore all dependencies (cached layer)
WORKDIR /src
RUN dotnet restore OHairGanic.API/OHairGanic.API.csproj

# copy toàn bộ source
COPY . .
WORKDIR /src/OHairGanic.API

# Publish without restore (dependencies already cached)
RUN dotnet publish -c Release -o /out --no-restore /p:UseAppHost=false

# ===== Runtime =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
COPY --from=build /out .
ENTRYPOINT ["dotnet","OHairGanic.API.dll"]

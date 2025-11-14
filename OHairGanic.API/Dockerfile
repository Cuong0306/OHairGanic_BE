# ===== Build =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj của từng project để cache restore
COPY OHairGanic.API/OHairGanic.API.csproj OHairGanic.API/
COPY OHairGanic.BLL/OHairGanic.BLL.csproj OHairGanic.BLL/
COPY OHairGanic.DAL/OHairGanic.DAL.csproj OHairGanic.DAL/
COPY OHairGanic.DTO/OHairGanic.DTO.csproj OHairGanic.DTO/

RUN dotnet restore OHairGanic.API/OHairGanic.API.csproj

# copy toàn bộ source
COPY . .
WORKDIR /src/OHairGanic.API

# ✅ THÊM 4 DÒNG NÀY — để cài thư viện AI trước khi publish
RUN dotnet add ../OHairGanic.BLL package Microsoft.ML.OnnxRuntime --version 1.19.2 && \
    dotnet add ../OHairGanic.BLL package SixLabors.ImageSharp --version 3.1.4 && \
    dotnet publish -c Release -o /out --no-restore /p:UseAppHost=false

# ===== Runtime =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
COPY --from=build /out .
ENTRYPOINT ["dotnet","OHairGanic.API.dll"]

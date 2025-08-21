
# Imagem base do SDK para build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copia os arquivos do projeto
COPY . ./

# Restaura dependências e compila
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Imagem base para runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copia os arquivos compilados
COPY --from=build /app/out ./

# Expõe a porta usada pela API
EXPOSE 5000

# Executa a API
ENTRYPOINT ["dotnet", "SimulacaoCreditoAPI.dll"]

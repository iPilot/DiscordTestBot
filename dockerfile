FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build
WORKDIR /app
RUN dotnet publish -c Release -o out ./app

FROM mcr.microsoft.com/dotnet/core/runtime:3.0 AS runtime
WORKDIR /app
COPY --from=build ./out ./

ENTRYPOINT ["dotnet", "PochinkiBot.dll"]
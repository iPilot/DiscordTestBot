FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build
WORKDIR .
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/runtime:3.0 AS runtime
WORKDIR /app
COPY --from=build ./out ./

ENTRYPOINT ["dotnet", "PochinkiBot.dll"]
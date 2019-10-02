FROM microsoft/dotnet:3.0.0-core-runtime-alpine

RUN apk update
RUN apk add curl
ENV LANG C.UTF-8

WORKDIR app
COPY build .

ENTRYPOINT ["dotnet", "DexBee.Sport.Hub.Listener.dll"]
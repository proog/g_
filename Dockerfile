FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS dotnetbuild
WORKDIR /src

COPY src/*.csproj ./
RUN dotnet restore

COPY src/. ./
RUN dotnet publish -c Release -o out


FROM node:lts-buster-slim AS vuebuild
WORKDIR /src/VueApp

COPY src/VueApp/package.json src/VueApp/package-lock.json ./
RUN npm install

COPY src/VueApp/. ./
RUN npm run build


FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS run
WORKDIR /app

COPY --from=dotnetbuild /src/out ./
COPY --from=vuebuild /src/wwwroot ./wwwroot

EXPOSE 80
ENTRYPOINT ["./g_"]

FROM microsoft/dotnet:2.2-sdk AS dotnetbuild
WORKDIR /src

COPY src/*.csproj ./
RUN dotnet restore

COPY src/. ./
RUN dotnet publish -c Release -o out


FROM node:10-jessie-slim AS vuebuild
WORKDIR /src/VueApp

COPY src/VueApp/package.json src/VueApp/package-lock.json ./
RUN npm install

COPY src/VueApp/. ./
RUN npm run build


FROM microsoft/dotnet:2.2-aspnetcore-runtime AS run
WORKDIR /app

COPY --from=dotnetbuild /src/out ./
COPY --from=vuebuild /src/wwwroot ./wwwroot

EXPOSE 80
ENTRYPOINT ["dotnet", "g_.dll"]

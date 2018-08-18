FROM microsoft/dotnet:2.1-sdk-alpine AS dotnetbuild
WORKDIR /src

COPY src/*.csproj ./
RUN dotnet restore

COPY src/. ./
RUN dotnet publish -c Release -o out


FROM node:8-alpine AS vuebuild
WORKDIR /src/VueApp

COPY src/VueApp/package.json src/VueApp/yarn.lock ./
RUN yarn

COPY src/VueApp/. ./
RUN yarn build


FROM microsoft/dotnet:2.1-aspnetcore-runtime-alpine AS run
WORKDIR /app

COPY --from=dotnetbuild /src/out ./
COPY --from=vuebuild /src/wwwroot ./wwwroot

EXPOSE 5000
ENTRYPOINT ["dotnet", "g_.dll"]

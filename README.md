# g_

g_ is a web app for creating a publicly available game collection.

Unlike many other collection apps, this is designed to be self-hosted and **does not rely on any external game databases**. All games, genres, platforms and tags are managed by the user. Optionally, you may register for an API key for the Giant Bomb game database and use that to make adding games a lot easier. This will allow automatic retrieval of cover art and game information.

g_ requires .NET Core 1.1 and a MySQL database. Configure `appsettings.json` and the app will create the necessary tables on first run as well as a default user called *Default* with password *default*.

[Here's a live demo.](http://permortensen.com/games/)

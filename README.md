# g_

g_ is a web app for creating a publicly available game collection.

Unlike many other collection apps, this is designed to be self-hosted and **does not rely on any external game databases**. All games, genres, platforms and tags are managed by the user. Optionally, you may register for an API key for the Giant Bomb game database and use that to make adding games a lot easier. This will allow automatic retrieval of cover art and game information.

g_ requires Apache 2 with mod_rewrite and PHP 5, as well as an SQL server supported by Illuminate Database. Put the g_ root somewhere in your DocumentRoot and go to &lt;g_root&gt;/api/setup to get started. This will guide you through the necessary configuration.

[Here's a live demo.](http://permortensen.com/games/)

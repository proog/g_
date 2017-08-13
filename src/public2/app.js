let app = new Vue({
  el: '#app',
  data: {
    users: [],
    selectedUser: null,
    currentUser: null,
    games: [],
    genres: [],
    platforms: [],
    tags: [],
    newGame: null,
    search: '',
    accessToken: '',
    isSettingsOpen: false
  },
  computed: {
    filteredGames: function () {
      return filterGames(
        this.games,
        this.genres,
        this.platforms,
        this.tags,
        this.search
      )
    },
    canEdit: function () {
      return this.currentUser
        && this.selectedUser
        && this.currentUser.id === this.selectedUser.id
    }
  },
  methods: {
    addGame: function () {
      this.newGame = {
        finished: 0,
        platform_ids: [],
        genre_ids: [],
        tag_ids: []
      }
    },
    newGameCancelled: function (game) {
      this.newGame = null
    },
    gameSaved: function (original, edited) {
      let updated = _.assign({}, original, edited)

      if (original === this.newGame) {
        this.newGame = null
        this.games.push(updated)
      }
      else {
        let index = _.indexOf(this.games, original)
        Vue.set(this.games, index, updated)
      }
    },
    gameRemoved: function (game) {
      this.games = _.without(this.games, game)
    },
    imageSaved: function (game, url) {
      game.image = url
    },
    imageRemoved: function (game) {
      game.image = null
    },
    genreRemoved: function (genre) {
      this.genres = _.without(this.genres, genre)

      for (let game of this.games) {
        game.genre_ids = _.without(game.genre_ids, genre.id)
      }
    },
    platformRemoved: function (platform) {
      this.platforms = _.without(this.platforms, platform)

      for (let game of this.games) {
        game.platform_ids = _.without(game.platform_ids, platform.id)
      }
    },
    tagRemoved: function (tag) {
      this.tags = _.without(this.tags, tag)

      for (let game of this.games) {
        game.tag_ids = _.without(game.tag_ids, tag.id)
      }
    },
    openSettings: function () {
      this.isSettingsOpen = true
    },
    closeSettings: function () {
      this.isSettingsOpen = false
    }
  },
  mounted: function () {
    this.currentUser = { id: 1 }

    getUsers()
      .then(users => {
        this.users = users
        this.selectedUser = _.head(users)
        return this.selectedUser
      })
      .then(user => {
        return Promise.all([
          getGames(user.id).then(games => this.games = games),
          getGenres(user.id).then(genres => this.genres = genres),
          getPlatforms(user.id).then(platforms => this.platforms = platforms),
          getTags(user.id).then(tags => this.tags = tags)
        ])
      })
  }
})

function filterGames(games, genres, platforms, tags, searchQuery) {
  let search = _.toLower(searchQuery)
    , filtered = search !== '' ? _.filter(games, filterPredicate) : games

  return _.orderBy(filtered, [
    x => x.currently_playing,
    x => _.toLower(x.sort_as) || _.toLower(x.title)
  ], ['desc', 'asc'])

  function filterPredicate(game) {
    let fields = [
      game.title,
      game.sort_as,
      game.developer,
      game.publisher,
      game.comment,
      _.toString(game.year)
    ]

    if (_.some(fields, field => _.includes(_.toLower(field), search)))
      return true

    let descriptors = _.concat(
      _.filter(genres, x => _.includes(game.genre_ids, x.id)),
      _.filter(platforms, x => _.includes(game.platform_ids, x.id)),
      _.filter(tags, x => _.includes(game.tag_ids, x.id))
    )
    let descriptorFields = _.flatMap(descriptors, x => [x.name, x.short_name])

    if (_.some(descriptorFields, field => _.includes(_.toLower(field), search)))
      return true

    return false
  }
}

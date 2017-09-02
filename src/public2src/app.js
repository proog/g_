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
    api: new Api(),
    isSettingsOpen: false,
    isLoginOpen: false
  },
  computed: {
    sortedGenres() {
      return _.orderBy(this.genres, x => _.toLower(x.name))
    },
    sortedPlatforms() {
      return _.orderBy(this.platforms, x => _.toLower(x.name))
    },
    sortedTags() {
      return _.orderBy(this.tags, x => _.toLower(x.name))
    },
    filteredGames() {
      return filterGames(
        this.games,
        this.genres,
        this.platforms,
        this.tags,
        this.search
      )
    },
    isLoggedIn() {
      return !!this.currentUser
    },
    canEdit() {
      return this.currentUser
        && this.selectedUser
        && this.currentUser.id === this.selectedUser.id
    }
  },
  methods: {
    addGame() {
      this.newGame = {
        currently_playing: false,
        finished: 0,
        platform_ids: [],
        genre_ids: [],
        tag_ids: []
      }
    },
    newGameCancelled(game) {
      this.newGame = null
    },
    gameSaved(original, edited) {
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
    gameRemoved(game) {
      this.games = _.without(this.games, game)
    },
    imageSaved(game, url) {
      game.image = url
    },
    imageRemoved(game) {
      game.image = null
    },
    openSettings() {
      this.isSettingsOpen = true
    },
    settingsSaved(genres, platforms, tags) {
      this.isSettingsOpen = false
      this.genres = genres
      this.platforms = platforms
      this.tags = tags

      for (let game of this.games) {
        game.genre_ids = pruneGameDescriptors(game.genre_ids, genres)
        game.platform_ids = pruneGameDescriptors(game.platform_ids, platforms)
        game.tag_ids = pruneGameDescriptors(game.tag_ids, tags)
      }
    },
    closeSettings() {
      this.isSettingsOpen = false
    },
    openLogin() {
      this.isLoginOpen = true
    },
    closeLogin() {
      this.isLoginOpen = false
    },
    loggedIn(username, accessToken) {
      this.api.accessToken = accessToken
      this.currentUser = _.find(this.users, x => x.username === username)
      this.closeLogin()
    },
    logOut() {
      this.api.accessToken = null
      this.currentUser = null
      this.newGame = null
      this.isSettingsOpen = false
    },
    debouncedSearch: _.debounce(function (event) {
      this.search = event.target.value
    }, 500)
  },
  mounted() {
    this.currentUser = { id: 1 }

    this.api.getUsers()
      .then(users => {
        this.users = users
        this.selectedUser = _.head(users)
        this.api.userId = this.selectedUser.id
        return this.selectedUser
      })
      .then(user => {
        return Promise.all([
          this.api.getGames().then(games => this.games = games),
          this.api.getGenres().then(genres => this.genres = genres),
          this.api.getPlatforms().then(platforms => this.platforms = platforms),
          this.api.getTags().then(tags => this.tags = tags)
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

function pruneGameDescriptors(ids, descriptors) {
  let invalidIds = _.difference(ids, _.map(descriptors, x => x.id))
  return _.without(ids, invalidIds)
}

<template>
  <div id="app">
    <nav class="navbar fixed-top navbar-light bg-light">
      <span class="navbar-text" v-if="selectedUser">
        {{ selectedUser.username }}'s games
      </span>
      <form class="form-inline">
        <button type="button"
                class="btn btn-success mr-2"
                :disabled="!!newGame"
                @click="addGame"
                v-if="canEdit">
          New
        </button>
        <button type="button"
                class="btn btn-outline-primary mr-2"
                :disabled="!!isSettingsOpen"
                @click="openSettings"
                v-if="canEdit">
          Settings
        </button>
        <button type="button"
                class="btn btn-outline-primary mr-2"
                @click="logOut"
                v-if="isLoggedIn">
          Log out
        </button>
        <button type="button"
                class="btn btn-outline-primary mr-2"
                @click="openLogin"
                v-else>
          Log in
        </button>
        <input type="search"
                class="form-control"
                placeholder="Search"
                :value="search"
                @input="debouncedSearch">
      </form>
    </nav>
    <div class="container-fluid">
      <div class="row" v-if="isLoginOpen">
        <div class="col pb-3">
          <login-form :api="api"
                      @login="loggedIn"
                      @cancel="closeLogin">
          </login-form>
        </div>
      </div>
      <div class="row" v-if="isSettingsOpen">
        <div class="col pb-3">
          <system-settings :genres="sortedGenres"
                           :platforms="sortedPlatforms"
                           :tags="sortedTags"
                           :api="api"
                           @save="settingsSaved"
                           @cancel="closeSettings">
          </system-settings>
        </div>
      </div>
      <div class="row" v-if="!!newGame">
        <div class="col col-lg-10 col-xl-8 mx-lg-auto pb-3">
          <game-item :game="newGame"
                     :all-genres="sortedGenres"
                     :all-platforms="sortedPlatforms"
                     :all-tags="sortedTags"
                     :api="api"
                     :is-editable="canEdit"
                     :is-new="true"
                     :is-assisted="isAssistedCreationEnabled"
                     @save="gameSaved"
                     @cancel="newGameCancelled">
          </game-item>
        </div>
      </div>
      <div class="row">
        <div class="col-12 col-lg-10 col-xl-6 mx-lg-auto pb-3"
             v-for="game in filteredGames"
             :key="game.id">
          <game-item :game="game"
                     :all-genres="sortedGenres"
                     :all-platforms="sortedPlatforms"
                     :all-tags="sortedTags"
                     :api="api"
                     :is-editable="canEdit"
                     :is-new="false"
                     :is-assisted="false"
                     @save="gameSaved"
                     @remove="gameRemoved">
          </game-item>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import _ from 'lodash'
import Vue from 'vue'
import Api from './api'
import GameItem from './GameItem.vue'
import LoginForm from './LoginForm.vue'
import SystemSettings from './SystemSettings.vue'

export default {
  data() {
    return {
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
      isLoginOpen: false,
      isAssistedCreationEnabled: false
    }
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
        title: '',
        sort_as: '',
        year: '',
        developer: '',
        publisher: '',
        currently_playing: false,
        finished: 0,
        playtime: '',
        comment: '',
        image: null,
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

      // remove any relations to deleted descriptors
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
      sessionStorage.setItem('token', accessToken)
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
    const accessToken = sessionStorage.getItem('token')

    this.api.getUsers()
      .then(users => {
        this.users = users

        if (accessToken && isJwtValid(accessToken)) {
          let payload = getJwtPayload(accessToken)
          this.api.accessToken = accessToken
          this.currentUser = _.find(users, x => x.id === payload['id'])
        }
      })
      .then(() => this.api.getConfig())
      .then(config => {
        this.selectedUser = _.find(this.users, x => x.id === config.default_user_id)
        this.api.userId = this.selectedUser.id
        this.isAssistedCreationEnabled = config.is_assisted_creation_enabled

        return Promise.all([
          this.api.getGames().then(games => this.games = games),
          this.api.getGenres().then(genres => this.genres = genres),
          this.api.getPlatforms().then(platforms => this.platforms = platforms),
          this.api.getTags().then(tags => this.tags = tags)
        ])
      })
  },
  components: {
    'game-item': GameItem,
    'login-form': LoginForm,
    'system-settings': SystemSettings
  }
}

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

function isJwtValid(token) {
  try {
    let payload = getJwtPayload(token)
      , now = new Date().getTime() / 1000
    return now >= payload['nbf'] && now < payload['exp']
  } catch (e) {
    return false
  }
}

function getJwtPayload(token) {
  try {
    let split = token.split('.')
    return JSON.parse(atob(split[1]))
  } catch (e) {
    return undefined
  }
}
</script>

<style>
.container-fluid {
  margin-top: 70px;
}
</style>

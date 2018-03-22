<template>
<div>
  <nav class="navbar fixed-top navbar-expand navbar-light bg-light">
    <form class="form-inline">
      <input type="search"
              class="form-control mr-2"
              placeholder="Search"
              v-focus
              :value="search"
              @input="debouncedSearch">
    </form>
    <form class="form-inline ml-auto">
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
    </form>
    <ul class="navbar-nav" v-if="selectedUser">
      <li class="nav-item dropdown">
        <a class="nav-link dropdown-toggle pointer" data-toggle="dropdown">
          {{ selectedUser.username }}'s games
        </a>
        <div class="dropdown-menu">
          <router-link v-for="user in users" :key="user.id"
                        :to="{ name: 'user', params: { userId: user.id }}"
                        class="dropdown-item">
            {{ user.username }}
          </router-link>
          <div class="dropdown-divider"></div>
          <button type="button" class="dropdown-item" @click="logOut" v-if="isLoggedIn">
            Log out
          </button>
          <button type="button" class="dropdown-item" @click="openLogin" v-else>
            Log in
          </button>
        </div>
      </li>
    </ul>
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
            v-for="game in limitedGames"
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
    <div class="row">
      <div class="col-12 pb-3 text-center">
        <p>
          Showing {{ limitedGames.length }}
          <span v-if="canShowMore">of {{ filteredGames.length }}</span>
          games
        </p>
        <button type="button" class="btn btn-success btn-lg px-5" v-if="canShowMore" @click="showMore">
          Show more
        </button>
      </div>
    </div>
  </div>
</div>
</template>

<script>
import _ from 'lodash'
import Vue from 'vue'
import Api from './api'
import { filterGames, getJwtPayload, isJwtValid } from './util'
import Focus from './focus'
import GameItem from './GameItem.vue'
import LoginForm from './LoginForm.vue'
import SystemSettings from './SystemSettings.vue'

export default {
  props: {
    userId: Number
  },
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
      isAssistedCreationEnabled: false,
      limit: 20
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
      return filterGames(this.games, this.genres, this.platforms, this.tags, this.search)
    },
    limitedGames() {
      return _.take(this.filteredGames, this.limit)
    },
    canShowMore() {
      return this.filteredGames.length > this.limitedGames.length
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
  watch: {
    userId() {
      // called when the route user id changes
      this.loadData()
    }
  },
  created() {
    // initial render
    this.loadData()
  },
  methods: {
    loadData() {
      this.api.getUsers()
        .then(users => {
          this.users = users

          const accessToken = sessionStorage.getItem('token')

          if (accessToken && isJwtValid(accessToken)) {
            const payload = getJwtPayload(accessToken)
            this.api.accessToken = accessToken
            this.currentUser = _.find(users, x => x.id === payload['id'])
          }
        })
        .then(() => this.api.getConfig())
        .then(config => {
          // find user from route
          const user = _.find(this.users, x => x.id === this.userId)

          // redirect to default user if none specified or not found
          if (!user) {
            this.$router.replace({
              name: 'user',
              params: { userId: config.default_user_id }
            })

            return this.userId ? Promise.reject('User not found') : Promise.resolve()
          }

          this.selectedUser = user
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
      sessionStorage.removeItem('token')
      this.api.accessToken = null
      this.currentUser = null
      this.newGame = null
      this.isSettingsOpen = false
    },
    debouncedSearch: _.debounce(function (event) {
      this.search = event.target.value
      this.limit = 20
    }, 200),
    showMore() {
      this.limit *= 2
    }
  },
  components: {
    'game-item': GameItem,
    'login-form': LoginForm,
    'system-settings': SystemSettings
  },
  directives: {
    'focus': Focus
  }
}

function pruneGameDescriptors(ids, descriptors) {
  let invalidIds = _.difference(ids, _.map(descriptors, x => x.id))
  return _.without(ids, invalidIds)
}
</script>

<style>
.container-fluid {
  margin-top: 70px;
}

.pointer {
  cursor: pointer;
}
</style>

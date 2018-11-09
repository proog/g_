<template>
<div class="container">
  <h1>g_ installer</h1>
  <div v-if="saved">
    <p class="lead">
      Cool beans! g_ has been successfully installed.
    </p>
    <router-link :to="{ name: 'root' }" class="btn btn-primary btn-lg">
      Open g_
    </router-link>
  </div>
  <div v-else>
    <p class="lead">
      This page will help you set up your own installation of g_.
    </p>
    <form class="card-body" @submit.prevent="save">
      <h2>Create default user</h2>
      <p>
        g_ needs at least one user to function. Create your login details here.
      </p>
      <div class="form-group">
        <input type="text" class="form-control" placeholder="Username" required v-focus v-model="username">
        <small class="form-text text-muted">
          This username will be visible in the application to identify your collection.
        </small>
      </div>
      <div class="form-group">
        <input type="password" class="form-control" placeholder="Password" required v-model="password">
      </div>

      <h2>Other settings</h2>
      <div class="form-group">
        <input type="text" class="form-control" placeholder="Giant Bomb API key (optional)" v-model="apiKey">
        <small class="form-text text-muted">
          g_ relies on the Giant Bomb game wiki for assisted game creation, making it possible to automatically retrieve most information when adding games to your collection.<br>
          If you want to use this feature, you need to create a free Giant Bomb account and <a href="http://www.giantbomb.com/api/" target="_blank" rel="noopener noreferrer">request an API key</a>.
          Without an API key, assisted game creation will be unavailable, but you can still add games manually.
        </small>
      </div>

      <h2>Install g_</h2>
      <p>
        That's all folks! Time to do this thing!
      </p>
      <div class="alert alert-danger" v-if="error">Error: {{ error }}</div>
      <button type="submit" class="btn btn-primary btn-lg" :disabled="saving">
        Install
      </button>
    </form>
  </div>
</div>
</template>

<script>
import Api from './api'
import Focus from './focus'
import { getLink } from './util'

export default {
  data() {
    return {
      username: '',
      password: '',
      apiKey: '',
      saving: false,
      saved: false,
      error: '',
      api: new Api()
    }
  },
  created() {
    this.api.get('/api').then(root => {
      this.api.root = root

      if (!getLink(root, 'setup')) {
        this.$router.replace({ name: 'root' })
        return Promise.reject('No setup link')
      }
    })
  },
  methods: {
    save() {
      const link = getLink(this.api.root, 'setup')
      const data = {
        username: this.username.trim(),
        password: this.password.trim(),
        api_key: this.apiKey.trim()
      }

      this.error = ''
      this.saving = true
      this.api.post(link.href, data)
        .then(() => {
          this.saved = true
          this.saving = false
        })
        .catch(error => {
          this.error = error
          this.saving = false
        })
    }
  },
  directives: {
    'focus': Focus
  }
}
</script>

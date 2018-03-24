<template>
<form class="card" @submit.prevent="login" @keyup.esc="cancel">
  <div class="card-body">
    <div class="form-group">
      <input type="text"
              class="form-control"
              required
              placeholder="Username"
              v-focus
              v-model="username"
              :class="{ 'is-invalid': !!error }"
              @input="clearError">
    </div>
    <div class="form-group">
      <input type="password"
              class="form-control"
              required
              placeholder="Password"
              v-model="password"
              :class="{ 'is-invalid': !!error }"
              @input="clearError">
    </div>
    <button type="submit" class="btn btn-success">
      Log in
    </button>
  </div>
</form>
</template>

<script>
import Api from './api'
import Focus from './focus'
import { getLink } from './util'

export default {
  props: {
    api: Api
  },
  data() {
    return {
      username: '',
      password: '',
      error: false
    }
  },
  methods: {
    login() {
      const link = getLink(this.api.root, 'oauth')
      const data = {
        'grant_type': 'password',
        'username': this.username,
        'password': this.password
      }

      this.clearError()
      this.api.postForm(link.href, data)
        .then(oauth => {
          const username = this.username
          this.clear()
          this.$emit('login', username, oauth.access_token)
        })
        .catch(() => this.error = true)
    },
    cancel() {
      this.clear()
      this.$emit('cancel')
    },
    clear() {
      this.username = ''
      this.password = ''
      this.error = false
    },
    clearError() {
      this.error = false
    }
  },
  directives: {
    'focus': Focus
  }
}
</script>

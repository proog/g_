<template>
<form class="card" @submit.prevent="login" @keyup.esc="cancel">
  <div class="card-body">
    <div class="form-group">
      <input type="text"
              class="form-control"
              required
              placeholder="Username"
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
      this.clearError()
      this.api.getAccessToken(this.username, this.password)
        .then(oauth => {
          let username = this.username
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
  }
}
</script>

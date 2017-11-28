Vue.component('login-form', {
  template: '#login-form-template',
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
})

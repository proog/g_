class Api {
  constructor() {
    this.userId = null
    this.accessToken = null
  }

  getUsers() {
    return this.get('/api/users')
  }

  getGames() {
    return this.get(`/api/users/${this.userId}/games`)
  }

  getGenres() {
    return this.get(`/api/users/${this.userId}/genres`)
  }
  getPlatforms() {
    return this.get(`/api/users/${this.userId}/platforms`)
  }
  getTags() {
    return this.get(`/api/users/${this.userId}/tags`)
  }

  postGame(game) {
    return this.post(`/api/users/${this.userId}/games`, game)
  }
  putGame(game) {
    return this.put(`/api/users/${this.userId}/games/${game.id}`, game)
  }
  deleteGame(game) {
    return this.del(`/api/users/${this.userId}/games/${game.id}`)
  }

  postImage(game, imageFile) {
    let form = new FormData();
    form.append('image', imageFile)

    return this.send(`/api/users/${this.userId}/games/${game.id}/image`, 'POST', form)
  }
  deleteImage(game) {
    return this.del(`/api/users/${this.userId}/games/${game.id}/image`)
  }

  postGenre(genre) {
    return this.post(`/api/users/${this.userId}/genres`, genre)
  }
  postPlatform(platform) {
    return this.post(`/api/users/${this.userId}/platforms`, platform)
  }
  postTag(tag) {
    return this.post(`/api/users/${this.userId}/tags`, tag)
  }

  putGenre(genre) {
    return this.put(`/api/users/${this.userId}/genres/${genre.id}`, genre)
  }
  putPlatform(platform) {
    return this.put(`/api/users/${this.userId}/platforms/${platform.id}`, platform)
  }
  putTag(tag) {
    return this.put(`/api/users/${this.userId}/tags/${tag.id}`, tag)
  }

  deleteGenre(genre) {
    return this.del(`/api/users/${this.userId}/genres/${genre.id}`)
  }
  deletePlatform(platform) {
    return this.del(`/api/users/${this.userId}/platforms/${platform.id}`)
  }
  deleteTag(tag) {
    return this.del(`/api/users/${this.userId}/tags/${tag.id}`)
  }

  getAccessToken(username, password) {
    let form = new FormData()
    form.append('grant_type', 'password')
    form.append('username', username)
    form.append('password', password)

    return this.send('/api/token', 'POST', form)
  }

  get(url) {
    return this.send(url, 'GET', null)
  }
  post(url, data) {
    return this.send(url, 'POST', JSON.stringify(data))
  }
  put(url, data) {
    return this.send(url, 'PUT', JSON.stringify(data))
  }
  del(url) {
    return this.send(url, 'DELETE', null)
  }

  send(url, method, body) {
    let options = {
      method: method,
      body: body,
      headers: new Headers()
    }

    if (_.isString(body))
      options.headers.append('Content-Type', 'application/json')

    if (this.accessToken)
      options.headers.append('Authorization', `Bearer ${this.accessToken}`)

    return fetch(url, options).then(response => {
      return response.text().then(text => {
        let parsed = text.length > 0
          ? JSON.parse(text)
          : undefined

        return response.ok
          ? parsed
          : Promise.reject(parsed && parsed.message)
      })
    })
  }
}

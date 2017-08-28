function getUsers() {
  return get('/api/users')
}

function getGames(userId) {
  return get(`/api/users/${userId}/games`)
}

function getGenres(userId) {
  return get(`/api/users/${userId}/genres`)
}
function getPlatforms(userId) {
  return get(`/api/users/${userId}/platforms`)
}
function getTags(userId) {
  return get(`/api/users/${userId}/tags`)
}

function postGame(userId, game) {
  return post(`/api/users/${userId}/games`, game)
}
function putGame(userId, game) {
  return put(`/api/users/${userId}/games/${game.id}`, game)
}
function deleteGame(userId, game) {
  return del(`/api/users/${userId}/games/${game.id}`)
}

function postGenre(userId, genre) {
  return post(`/api/users/${userId}/genres`, genre)
}
function postPlatform(userId, platform) {
  return post(`/api/users/${userId}/platforms`, platform)
}
function postTag(userId, tag) {
  return post(`/api/users/${userId}/tags`, tag)
}

function putGenre(userId, genre) {
  return put(`/api/users/${userId}/genres/${genre.id}`, genre)
}
function putPlatform(userId, platform) {
  return put(`/api/users/${userId}/platforms/${platform.id}`, platform)
}
function putTag(userId, tag) {
  return put(`/api/users/${userId}/tags/${tag.id}`, tag)
}

function deleteGenre(userId, genre) {
  return del(`/api/users/${userId}/genres/${genre.id}`)
}
function deletePlatform(userId, platform) {
  return del(`/api/users/${userId}/platforms/${platform.id}`)
}
function deleteTag(userId, tag) {
  return del(`/api/users/${userId}/tags/${tag.id}`)
}

function getAccessToken(username, password) {
  let form = new FormData()
  form.append('grant_type', 'password')
  form.append('username', username)
  form.append('password', password)

  return send('/api/token', 'POST', form)
}

function get(url, accessToken) {
  return send(url, 'GET', null, accessToken)
}
function post(url, data, accessToken) {
  return send(url, 'POST', JSON.stringify(data), accessToken)
}
function put(url, data, accessToken) {
  return send(url, 'PUT', JSON.stringify(data), accessToken)
}
function del(url, accessToken) {
  return send(url, 'DELETE', null, accessToken)
}

function send(url, method, body, accessToken) {
  let options = {
    method: method,
    body: body,
    headers: new Headers()
  }

  if (accessToken)
    options.headers.append('Authorization', `Bearer ${accessToken}`)

  return fetch(url, options)
    .then(response => {
      let json = response.json()

      return response.ok
        ? json
        : Promise.reject(json.message)
    })
}

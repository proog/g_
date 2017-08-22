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

function get(url, accessToken) {
  return send(url, {
    method: 'GET',
    headers: accessToken
      ? { 'Authorization': `Bearer ${accessToken}` }
      : {}
  })
}

function post(url, data, accessToken) {
  return send(url, {
    method: 'POST',
    data: JSON.stringify(data),
    headers: accessToken
      ? { 'Authorization': `Bearer ${accessToken}` }
      : {}
  })
}

function put(url, data, accessToken) {
  return send(url, {
    method: 'PUT',
    data: JSON.stringify(data),
    headers: accessToken
      ? { 'Authorization': `Bearer ${accessToken}` }
      : {}
  })
}

function send(url, options) {
  return fetch(url, options)
    .then(response => response.json())
}

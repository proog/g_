function getUsers() {
  return getJson('/api/users')
}

function getGames(userId) {
  return getJson(`/api/users/${userId}/games`)
}

function getGenres(userId) {
  return getJson(`/api/users/${userId}/genres`)
    .then(genres => _.orderBy(genres, x => x.name))
}

function getPlatforms(userId) {
  return getJson(`/api/users/${userId}/platforms`)
    .then(platforms => _.orderBy(platforms, x => x.name))
}

function getTags(userId) {
  return getJson(`/api/users/${userId}/tags`)
    .then(tags => _.orderBy(tags, x => x.name))
}

function getJson(url, accessToken) {
  let options = {
    method: 'GET',
    headers: accessToken
      ? { 'Authorization': `Bearer ${accessToken}` }
      : {}
  }

  return fetch(url, options)
    .then(response => response.json())
}

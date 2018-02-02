export function filterGames(games, genres, platforms, tags, searchQuery) {
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

export function isJwtValid(token) {
  try {
    let payload = getJwtPayload(token)
      , now = new Date().getTime() / 1000
    return now >= payload['nbf'] && now < payload['exp']
  } catch (e) {
    return false
  }
}

export function getJwtPayload(token) {
  try {
    let split = token.split('.')
    return JSON.parse(atob(split[1]))
  } catch (e) {
    return undefined
  }
}

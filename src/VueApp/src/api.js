import _ from 'lodash'

export default class Api {
  constructor() {
    this.accessToken = null
    this.root = null
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

  postForm(url, values) {
    let form = new FormData()

    for (let key in values)
      form.append(key, values[key])

    return this.send(url, 'POST', form)
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

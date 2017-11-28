Vue.component('system-settings', {
  template: '#system-settings-template',
  props: {
    genres: Array,
    platforms: Array,
    tags: Array,
    api: Api
  },
  data() {
    return {
      editedGenres: _.cloneDeep(this.genres),
      editedPlatforms: _.cloneDeep(this.platforms),
      editedTags: _.cloneDeep(this.tags)
    }
  },
  methods: {
    addGenre() {
      this.editedGenres.push({ name: '', short_name: '' })
    },
    addPlatform() {
      this.editedPlatforms.push({ name: '', short_name: '' })
    },
    addTag() {
      this.editedTags.push({ name: '', short_name: '' })
    },
    removeGenre(genre) {
      if (confirm(`Are you sure you want to delete ${genre.name}?`))
        this.editedGenres = _.without(this.editedGenres, genre)
    },
    removePlatform(platform) {
      if (confirm(`Are you sure you want to delete ${platform.name}?`))
        this.editedPlatforms = _.without(this.editedPlatforms, platform)
    },
    removeTag(tag) {
      if (confirm(`Are you sure you want to delete ${tag.name}?`))
        this.editedTags = _.without(this.editedTags, tag)
    },
    save() {
      let addedGenres = getAdded(this.genres, this.editedGenres)
        , addedPlatforms = getAdded(this.platforms, this.editedPlatforms)
        , addedTags = getAdded(this.tags, this.editedTags)

      let updatedGenres = getUpdated(this.genres, this.editedGenres)
        , updatedPlatforms = getUpdated(this.platforms, this.editedPlatforms)
        , updatedTags = getUpdated(this.tags, this.editedTags)

      let removedGenres = getRemoved(this.genres, this.editedGenres)
        , removedPlatforms = getRemoved(this.platforms, this.editedPlatforms)
        , removedTags = getRemoved(this.tags, this.editedTags)

      // map to delete, post and put
      let promises = _.flatten([
        _.map(addedGenres, x => this.api.postGenre(x)),
        _.map(addedPlatforms, x => this.api.postPlatform(x)),
        _.map(addedTags, x => this.api.postTag(x)),
        _.map(updatedGenres, x => this.api.putGenre(x)),
        _.map(updatedPlatforms, x => this.api.putPlatform(x)),
        _.map(updatedTags, x => this.api.putTag(x)),
        _.map(removedGenres, x => this.api.deleteGenre(x)),
        _.map(removedPlatforms, x => this.api.deletePlatform(x)),
        _.map(removedTags, x => this.api.deleteTag(x))
      ])

      Promise.all(promises)
        .then(() => Promise.all([
          this.api.getGenres(),
          this.api.getPlatforms(),
          this.api.getTags()
        ]))
        .then(refreshed => {
          this.$emit('save', refreshed[0], refreshed[1], refreshed[2])
        })
    },
    cancel() {
      this.$emit('cancel')
    }
  }
})

function getAdded(originalDescriptors, editedDescriptors) {
  // it's added if it doesn't have an id
  return _.filter(editedDescriptors, edited => !edited.id)
}

function getUpdated(originalDescriptors, editedDescriptors) {
  // it's updated if its id is found in the original array and its properties have changed
  return _(editedDescriptors)
    .filter(edited => !!edited.id)
    .filter(edited => {
      let original = _.find(originalDescriptors, x => x.id === edited.id)

      if (original === undefined)
        return false

      return edited.name !== original.name || edited.short_name !== original.short_name
    })
    .value()
}

function getRemoved(originalDescriptors, editedDescriptors) {
  // it's removed if its id is found in the original array, but not in the edited one
  return _.filter(
    originalDescriptors,
    original => _.every(editedDescriptors, edited => edited.id !== original.id)
  )
}

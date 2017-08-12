Vue.component('game-item', {
  template: '#game-item-template',
  props: {
    game: Object,
    allGenres: Array,
    allPlatforms: Array,
    allTags: Array,
    isEditable: Boolean,
    isNew: Boolean
  },
  data: function () {
    return {
      isEditing: false,
      edited: null,
      imageFile: null,
      imageRemoved: false
    }
  },
  computed: {
    genres: function () {
      return summary(this.allGenres, this.game.genre_ids)
    },
    platforms: function () {
      return summary(this.allPlatforms, this.game.platform_ids)
    },
    tags: function () {
      return summary(this.allTags, this.game.tag_ids)
    }
  },
  methods: {
    edit: function () {
      this.isEditing = true
      this.edited = _.cloneDeep(this.game)
      this.imageFile = null
      this.imageRemoved = false
    },
    save: function () {
      if (this.isNew) {
        // POST
        // _.assign(this.edited, created)
      }
      else {
        // PUT
        // _.assign(this.edited, updated)
      }

      if (this.game.image && this.imageRemoved) {
        // DELETE image, then
        this.edited.image = null
      }
      else if (this.imageFile) {
        // POST image, then
        let url = 'http://lorempizza.com/380/240'
        this.edited.image = url
      }

      this.isEditing = false
      this.$emit('save', this.game, this.edited)
    },
    remove: function () {
      if (!confirm(`Are you sure you want to delete ${this.game.title}?`))
        return

      // DELETE game.id, then
      this.isEditing = false
      this.$emit('remove', this.game)
    },
    cancel: function () {
      this.isEditing = false
      this.$emit('cancel', this.game)
    },
    updateImage: function (fileEvent) {
      this.imageRemoved = false
      this.imageFile = _.head(fileEvent.target.files)
    },
    removeImage: function () {
      // ask for confirmation for existing games (because we can't easily get the image back)
      if (!this.isNew && !confirm('Are you sure you want to delete this image?'))
        return

      this.imageRemoved = true
      this.imageFile = null

      // clear the file input by resetting the form
      this.$refs.imageForm.reset()
    }
  },
  mounted: function () {
    // if adding a new game, go directly to edit mode
    if (this.isNew)
      this.edit()
  }
})

function summary(descriptors, ids) {
  return descriptors
    .filter(x => _.includes(ids, x.id))
    .map(x => x.name)
    .join(', ')
}

Vue.component('game-item', {
  template: '#game-item-template',
  props: {
    game: Object,
    allGenres: Array,
    allPlatforms: Array,
    allTags: Array,
    api: Api,
    isEditable: Boolean,
    isNew: Boolean,
    isAssisted: Boolean
  },
  data() {
    return {
      isEditing: false,
      isSaving: false,
      edited: null,
      imageFile: null,
      imageRemoved: false
    }
  },
  computed: {
    byLine() {
      let developer = this.game.developer
        , publisher = this.game.publisher

      if (developer && publisher && developer !== publisher)
        return `${developer} / ${publisher}`

      return developer ? developer : publisher
    },
    playtime() {
      return this.game.playtime || '--:--:--'
    },
    comment() {
      return this.game.comment || '-'
    },
    genres() {
      return summary(this.allGenres, this.game.genre_ids)
    },
    platforms() {
      return summary(this.allPlatforms, this.game.platform_ids)
    },
    tags() {
      return summary(this.allTags, this.game.tag_ids)
    }
  },
  watch: {
    'edited.title': function (newTitle, oldTitle) {
      // for new games, update sort_as if it's following the title
      if (this.isNew && this.edited.sort_as === oldTitle)
        this.edited.sort_as = newTitle

      // when editing the title, provide autocompletion via giant bomb
      if (this.isAssisted && newTitle.length > 2)
        this.autocomplete(newTitle)
    }
  },
  methods: {
    edit() {
      this.isEditing = true
      this.edited = _.cloneDeep(this.game)
      this.imageFile = null
      this.imageRemoved = false
    },
    save() {
      let createOrUpdate = () => {
        return this.isNew
          ? this.api.postGame(this.edited)
          : this.api.putGame(this.edited)
      }
      let postOrDeleteImage = () => {
        if (this.imageFile)
          return this.api.postImage(this.game, this.imageFile)
            .then(gameWithImage => this.edited.image = gameWithImage.image)

        if (this.game.image && this.imageRemoved)
          return this.api.deleteImage(this.game)
            .then(() => this.edited.image = null)
      }

      if (this.isSaving)
        return

      this.isSaving = true
      createOrUpdate()
        .then(updated => _.assign(this.edited, updated))
        .then(() => postOrDeleteImage())
        .then(() => {
          this.isSaving = false
          this.isEditing = false
          this.$emit('save', this.game, this.edited)
        })
        .catch(error => {
          this.isSaving = false
          alert(error)
        })
    },
    remove() {
      if (this.isSaving || !confirm(`Are you sure you want to delete ${this.game.title}?`))
        return

      this.api.deleteGame(this.game).then(() => {
        this.isEditing = false
        this.$emit('remove', this.game)
      })
    },
    cancel() {
      if (this.isSaving)
        return

      this.isEditing = false
      this.$emit('cancel', this.game)
    },
    updateImage(fileEvent) {
      this.imageRemoved = false
      this.imageFile = _.head(fileEvent.target.files)
    },
    removeImage() {
      // ask for confirmation for existing games (because we can't easily get the image back)
      if (!this.isNew && !confirm('Are you sure you want to delete this image?'))
        return

      this.imageRemoved = true
      this.imageFile = null

      // clear the file input by resetting the form
      this.$refs.imageForm.reset()
    },
    autocomplete: _.debounce(function (title) {
      this.api.getAssistedSearch(title).then(gbGames => {
        console.log(gbGames)
      })
    }, 1000)
  },
  mounted() {
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

Vue.component('system-settings', {
  template: '#system-settings-template',
  props: {
    genres: Array,
    platforms: Array,
    tags: Array
  },
  methods: {
    removeGenre: function (genre) {
      if (!confirm(`Are you sure you want to delete ${genre.name}?`))
        return

      // DELETE genre
      this.$emit('remove-genre', genre)
    },
    removePlatform: function (platform) {
      if (!confirm(`Are you sure you want to delete ${platform.name}?`))
        return

      // DELETE platform
      this.$emit('remove-platform', platform)
    },
    removeTag: function (tag) {
      if (!confirm(`Are you sure you want to delete ${tag.name}?`))
        return

      // DELETE tag
      this.$emit('remove-tag', tag)
    },
    close: function () {
      this.$emit('close')
    }
  }
})

Vue.component('descriptors-editor', {
  template: '#descriptors-editor-template',
  props: {
    items: Array
  },
  methods: {
    remove: function (item) {
      this.$emit('remove', item)
    }
  }
})

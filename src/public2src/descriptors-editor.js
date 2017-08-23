Vue.component('descriptors-editor', {
  template: '#descriptors-editor-template',
  props: {
    items: Array
  },
  methods: {
    add() {
      this.$emit('add')
    },
    remove(item) {
      this.$emit('remove', item)
    }
  }
})

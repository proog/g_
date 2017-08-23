Vue.component('descriptor-list', {
  template: '#descriptor-list-template',
  props: {
    value: Array,
    items: Array
  },
  data() {
    return { cloned: _.clone(this.value) }
  },
  methods: {
    update() {
      this.$emit('input', this.cloned)
    }
  }
})

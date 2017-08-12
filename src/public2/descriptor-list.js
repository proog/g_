Vue.component('descriptor-list', {
  template: '#descriptor-list-template',
  props: {
    value: Array,
    items: Array
  },
  data: function () {
    return { cloned: _.clone(this.value) }
  },
  methods: {
    update: function () {
      this.$emit('input', this.cloned)
    }
  }
})

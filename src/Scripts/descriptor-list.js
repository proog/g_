Vue.component('descriptor-list', {
  template: '#descriptor-list-template',
  props: {
    value: Array,
    items: Array
  },
  data() {
    return { cloned: _.clone(this.value) }
  },
  watch: {
    value: function (newValue) {
      // update cloned list when parent component has updated the original
      this.cloned = _.clone(newValue)
    }
  },
  methods: {
    update() {
      this.$emit('input', this.cloned)
    }
  }
})

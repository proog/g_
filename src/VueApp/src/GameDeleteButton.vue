<template>
<button type="button" class="btn btn-outline-danger" :disabled="disabled" @click="click">
  Delete
</button>
</template>

<script>
export default {
  props: {
    game: Object,
    disabled: Boolean
  },
  methods: {
    click() {
      if (this.disabled || !confirm(`Are you sure you want to delete ${this.game.title}?`))
        return

      this.$emit('click')
    },
  }
}
</script>

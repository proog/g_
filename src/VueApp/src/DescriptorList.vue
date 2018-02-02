<template>
<div>
  <div class="form-check" v-for="item in items" :key="item.id">
    <label class="form-check-label">
      <input type="checkbox"
              class="form-check-input"
              v-model="cloned"
              :value="item.id"
              @change="update">
      {{ item.name }}
    </label>
  </div>
</div>
</template>

<script>
import _ from 'lodash'

export default {
  props: {
    value: Array,
    items: Array
  },
  data() {
    return { cloned: _.clone(this.value) }
  },
  watch: {
    value (newValue) {
      // update cloned list when parent component has updated the original
      this.cloned = _.clone(newValue)
    }
  },
  methods: {
    update() {
      this.$emit('input', this.cloned)
    }
  }
}
</script>

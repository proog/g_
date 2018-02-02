<template>
<div>
  <div class="form-check" v-for="(item, index) in items" :key="item.id">
    <input type="checkbox"
            class="form-check-input"
            v-model="cloned"
            :id="ids[index]"
            :value="item.id"
            @change="update">
    <label class="form-check-label" :for="ids[index]">
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
  computed: {
    ids() {
      // create an array of ids to tie inputs and labels together
      return this.items.map(x => _.uniqueId('_id'))
    }
  },
  methods: {
    update() {
      this.$emit('input', this.cloned)
    }
  }
}
</script>

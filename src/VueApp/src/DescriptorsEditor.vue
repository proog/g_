<template>
<div>
  <div class="form-row" v-for="item in items" :key="item.id">
    <div class="col-6 form-group">
      <input type="text"
              class="form-control form-control-sm"
              required
              placeholder="Name"
              v-model="item.name">
    </div>
    <div class="col-4 form-group">
      <input type="text"
              class="form-control form-control-sm"
              required
              placeholder="Short name"
              v-model="item.short_name">
    </div>
    <div class="col-2 form-group">
      <button type="button" class="btn btn-sm btn-outline-danger" @click="remove(item)">
        <i class="fa fa-times"></i>
      </button>
    </div>
  </div>
  <div class="form-row">
    <div class="col form-group">
      <button type="button" class="btn btn-sm btn-outline-success" @click="add">
        <i class="fa fa-plus"></i>
      </button>
    </div>
  </div>
</div>
</template>

<script>
export default {
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
}
</script>

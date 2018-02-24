<template>
<div class="input-group">
  <input type="text"
          class="form-control"
          placeholder="Title"
          required
          v-focus
          ref="input"
          :value="value"
          @input="titleChanged($event.target.value)">
  <div class="input-group-append" v-if="completions.length">
    <button type="button"
            class="btn dropdown-toggle"
            data-toggle="dropdown"
            :class="completions.length ? 'btn-outline-success' : 'btn-outline-secondary'"
            :disabled="!completions.length">
      Suggest
    </button>
    <div class="dropdown-menu">
      <button type="button"
              class="dropdown-item"
              v-for="completion in completions"
              :key="completion.id"
              @click="selectCompletion(completion)">
        {{ completion.title }}
      </button>
    </div>
  </div>
</div>
</template>

<script>
import _ from 'lodash'
import Api from './api'
import Focus from './focus'

export default {
  props: {
    api: Api,
    isAssisted: Boolean,
    value: String
  },
  data() {
    return { completions: [] }
  },
  methods: {
    titleChanged(title) {
      this.$emit('input', title)

      // when editing the title, provide autocompletion via giant bomb
      if (this.isAssisted && title.length > 2)
        this.autocomplete(title)
      else
        this.completions = []
    },
    autocomplete: _.debounce(function (title) {
      this.api.getAssistedSearch(title).then(gbGames => {
        this.completions = gbGames
      })
    }, 1000),
    selectCompletion(completion) {
      this.api.getAssistedGame(completion.id).then(gbGame => {
        this.$refs.input.focus()
        this.$emit('select', gbGame)
      })
    }
  },
  directives: {
    'focus': Focus
  }
}
</script>

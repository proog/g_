<template>
<div class="card h-100">
  <game-editor v-if="isEditing"
                :game="game"
                :all-genres="allGenres"
                :all-platforms="allPlatforms"
                :all-tags="allTags"
                :api="api"
                :is-new="isNew"
                :is-assisted="isAssisted"
                @save="saved"
                @cancel="cancelled"
                @remove="remove">
  </game-editor>
  <game-display v-else
                :game="game"
                :all-genres="allGenres"
                :all-platforms="allPlatforms"
                :all-tags="allTags"
                :is-editable="isEditable"
                @edit="edit"
                @remove="remove">
  </game-display>
</div>
</template>

<script>
import _ from 'lodash'
import Api from './api'
import GameDisplay from './GameDisplay.vue'
import GameEditor from './GameEditor.vue'

export default {
  props: {
    game: Object,
    allGenres: Array,
    allPlatforms: Array,
    allTags: Array,
    api: Api,
    isEditable: Boolean,
    isNew: Boolean,
    isAssisted: Boolean
  },
  data() {
    return { isEditing: false }
  },
  methods: {
    edit() {
      this.isEditing = true
    },
    remove() {
      this.api.deleteGame(this.game).then(() => {
        this.isEditing = false
        this.$emit('remove', this.game)
      })
    },
    saved(updatedGame) {
      this.isEditing = false
      this.$emit('save', this.game, updatedGame)
    },
    cancelled() {
      this.isEditing = false
      this.$emit('cancel', this.game)
    }
  },
  mounted() {
    // if adding a new game, go directly to edit mode
    if (this.isNew)
      this.edit()
  },
  components: {
    'game-editor': GameEditor,
    'game-display': GameDisplay
  }
}
</script>

<style>
.cover {
  height: 250px;
  text-align: center;
}

.cover img {
  max-height: 100%;
}
</style>

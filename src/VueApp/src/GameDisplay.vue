<template>
<div class="row card-body">
  <div class="col-4">
    <div class="cover">
      <img :src="game.image" class="rounded img-fluid" v-if="game.image">
      <h4 v-else>No image</h4>
    </div>
  </div>
  <div class="col-8">
    <div class="h-100 d-flex flex-column justify-content-between">
      <div>
        <h4>
          <i class="fa fa-play-circle" v-if="game.currently_playing"></i>
          {{ game.title }}
          <small v-if="game.year">({{ game.year }})</small>
        </h4>
        <p>{{ byLine }}</p>
        <p>
          <i class="fa" :class="game.finished === 1 ? 'fa-check-square-o' : 'fa-square-o'"></i>
          {{ playtime }}
          <br>
          <i class="fa fa-comment"></i> {{ comment }}
        </p>
        <p>
          <i class="fa fa-gamepad"></i> {{ platforms }}<br>
          <i class="fa fa-book"></i> {{ genres }}<br>
          <i class="fa fa-tag"></i> {{ tags }}
        </p>
      </div>
      <div class="text-right" v-if="isEditable">
        <button type="button" class="btn btn-outline-primary" @click="edit">
          Edit
        </button>
        <delete-button :game="game" @click="remove"></delete-button>
      </div>
    </div>
  </div>
</div>
</template>

<script>
import _ from 'lodash'
import DeleteButton from './GameDeleteButton.vue'

export default {
  props: {
    game: Object,
    allGenres: Array,
    allPlatforms: Array,
    allTags: Array,
    isEditable: Boolean
  },
  computed: {
    byLine() {
      let developer = this.game.developer
        , publisher = this.game.publisher

      if (developer && publisher && developer !== publisher)
        return `${developer} / ${publisher}`

      return developer ? developer : publisher
    },
    playtime() {
      return this.game.playtime || '--:--:--'
    },
    comment() {
      return this.game.comment || '-'
    },
    genres() {
      return summary(this.allGenres, this.game.genre_ids)
    },
    platforms() {
      return summary(this.allPlatforms, this.game.platform_ids)
    },
    tags() {
      return summary(this.allTags, this.game.tag_ids)
    }
  },
  methods: {
    edit() {
      this.$emit('edit')
    },
    remove() {
      this.$emit('remove')
    }
  },
  components: {
    'delete-button': DeleteButton
  }
}

function summary(descriptors, ids) {
  return descriptors
    .filter(x => _.includes(ids, x.id))
    .map(x => x.name)
    .join(', ')
}
</script>

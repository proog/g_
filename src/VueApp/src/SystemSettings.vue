<template>
<form class="card" @submit.prevent="save" @keyup.esc="cancel">
  <div class="card-body">
    <div class="row">
      <div class="col-12 col-lg-4">
        <h4>Genres</h4>
        <descriptors-editor :items="editedGenres"
                            @add="addGenre"
                            @remove="removeGenre">
        </descriptors-editor>
      </div>
      <div class="col-12 col-lg-4">
        <h4>Platforms</h4>
        <descriptors-editor :items="editedPlatforms"
                            @add="addPlatform"
                            @remove="removePlatform">
        </descriptors-editor>
      </div>
      <div class="col-12 col-lg-4">
        <h4>Tags</h4>
        <descriptors-editor :items="editedTags"
                            @add="addTag"
                            @remove="removeTag">
        </descriptors-editor>
      </div>
    </div>
    <div>
      <button type="submit" class="btn btn-success">
        Save
      </button>
      <button type="button" class="btn btn-outline-secondary" @click="cancel">
        Cancel
      </button>
    </div>
  </div>
</form>
</template>

<script>
import _ from 'lodash'
import Api from './api'
import { getLink } from './util'
import DescriptorsEditor from './DescriptorsEditor.vue'

export default {
  props: {
    genres: Array,
    platforms: Array,
    tags: Array,
    user: Object,
    api: Api
  },
  data() {
    return {
      editedGenres: _.cloneDeep(this.genres),
      editedPlatforms: _.cloneDeep(this.platforms),
      editedTags: _.cloneDeep(this.tags)
    }
  },
  methods: {
    addGenre() {
      this.editedGenres.push({ name: '', short_name: '' })
    },
    addPlatform() {
      this.editedPlatforms.push({ name: '', short_name: '' })
    },
    addTag() {
      this.editedTags.push({ name: '', short_name: '' })
    },
    removeGenre(genre) {
      if (confirm(`Are you sure you want to delete ${genre.name}?`))
        this.editedGenres = _.without(this.editedGenres, genre)
    },
    removePlatform(platform) {
      if (confirm(`Are you sure you want to delete ${platform.name}?`))
        this.editedPlatforms = _.without(this.editedPlatforms, platform)
    },
    removeTag(tag) {
      if (confirm(`Are you sure you want to delete ${tag.name}?`))
        this.editedTags = _.without(this.editedTags, tag)
    },
    save() {
      let addedGenres = getAdded(this.genres, this.editedGenres)
        , addedPlatforms = getAdded(this.platforms, this.editedPlatforms)
        , addedTags = getAdded(this.tags, this.editedTags)

      let updatedGenres = getUpdated(this.genres, this.editedGenres)
        , updatedPlatforms = getUpdated(this.platforms, this.editedPlatforms)
        , updatedTags = getUpdated(this.tags, this.editedTags)

      let removedGenres = getRemoved(this.genres, this.editedGenres)
        , removedPlatforms = getRemoved(this.platforms, this.editedPlatforms)
        , removedTags = getRemoved(this.tags, this.editedTags)

      const genresLink = getLink(this.user, 'genres')
      const platformsLink = getLink(this.user, 'platforms')
      const tagsLink = getLink(this.user, 'tags')

      const update = descriptor => {
        const link = getLink(descriptor, 'self')
        return this.api.put(link.href, descriptor)
      }

      const remove = descriptor => {
        const link = getLink(descriptor, 'self')
        return this.api.del(link.href, descriptor)
      }

      // map to delete, post and update
      const promises = _.flatten([
        _.map(addedGenres, x => this.api.post(genresLink.href, x)),
        _.map(addedPlatforms, x => this.api.post(platformsLink.href, x)),
        _.map(addedTags, x => this.api.post(tagsLink.href, x)),
        _.map(updatedGenres, update),
        _.map(updatedPlatforms, update),
        _.map(updatedTags, update),
        _.map(removedGenres, remove),
        _.map(removedPlatforms, remove),
        _.map(removedTags, remove)
      ])

      Promise.all(promises)
        .then(() => Promise.all([
          this.api.get(genresLink.href),
          this.api.get(platformsLink.href),
          this.api.get(tagsLink.href)
        ]))
        .then(refreshed => {
          this.$emit('save', refreshed[0], refreshed[1], refreshed[2])
        })
    },
    cancel() {
      this.$emit('cancel')
    }
  },
  components: {
    'descriptors-editor': DescriptorsEditor
  }
}

function getAdded(originalDescriptors, editedDescriptors) {
  // it's added if it doesn't have an id
  return _.filter(editedDescriptors, edited => !edited.id)
}

function getUpdated(originalDescriptors, editedDescriptors) {
  // it's updated if its id is found in the original array and its properties have changed
  return _(editedDescriptors)
    .filter(edited => !!edited.id)
    .filter(edited => {
      let original = _.find(originalDescriptors, x => x.id === edited.id)

      if (original === undefined)
        return false

      return edited.name !== original.name || edited.short_name !== original.short_name
    })
    .value()
}

function getRemoved(originalDescriptors, editedDescriptors) {
  // it's removed if its id is found in the original array, but not in the edited one
  return _.filter(
    originalDescriptors,
    original => _.every(editedDescriptors, edited => edited.id !== original.id)
  )
}
</script>

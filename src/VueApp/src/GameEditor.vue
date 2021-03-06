<template>
<form class="card-body" @submit.prevent="save" @keyup.esc="cancel">
  <div class="row">
    <div class="col-4">
      <div class="cover">
        <img :src="edited.image" class="rounded img-fluid" v-if="edited.image && !imageRemoved">
        <h4 v-else>No image</h4>
      </div>
      <div class="form-group mt-3">
        <div class="custom-file">
          <input type="file"
                  class="custom-file-input"
                  :disabled="isSaving"
                  @change="updateImage"
                  ref="imageInput">
          <label class="custom-file-label">
            <span v-if="imageFile">{{ imageFile.name }}</span>
            <span v-else>Choose file</span>
          </label>
        </div>
      </div>
      <div class="form-group" v-if="imageUrl">
        <input type="text" class="form-control" readonly v-model="imageUrl">
      </div>
      <div class="form-group">
        <button type="button"
                class="btn btn-outline-danger"
                :disabled="isSaving"
                @click="removeImage"
                v-if="(edited.image || imageFile || imageUrl) && !imageRemoved">
          Delete image
        </button>
      </div>
    </div>
    <div class="col-8">
      <div class="form-row">
        <div class="col-8 form-group">
          <title-editor :api="api"
                        :is-assisted="isAssisted"
                        v-model="edited.title"
                        @select="applyAssistance">
          </title-editor>
        </div>
        <div class="col-4 form-group">
          <input type="number"
                  class="form-control"
                  placeholder="Release year"
                  required
                  v-model.number="edited.year">
        </div>
      </div>
      <div class="form-row">
        <div class="col-8 form-group">
          <input type="text"
                  class="form-control"
                  placeholder="Sort as"
                  v-model="edited.sort_as">
        </div>
        <div class="col-4 form-group">
          <div class="form-check">
            <input type="checkbox"
                    class="form-check-input"
                    v-model="edited.currently_playing"
                    :id="idPlaying">
            <label class="form-check-label" :for="idPlaying">
              Playing
            </label>
          </div>
        </div>
      </div>
      <div class="form-row">
        <div class="col form-group">
          <input type="text"
                  class="form-control"
                  placeholder="Developer"
                  v-model="edited.developer">
        </div>
        <div class="col form-group">
          <input type="text"
                  class="form-control"
                  placeholder="Publisher"
                  v-model="edited.publisher">
        </div>
      </div>
      <div class="form-row">
        <div class="col form-group">
          <div class="form-check">
            <input type="checkbox"
                    class="form-check-input"
                    :id="idFinished"
                    v-model="edited.finished"
                    :true-value="1"
                    :false-value="0">
            <label class="form-check-label" :for="idFinished">
              Finished
            </label>
          </div>
        </div>
        <div class="col form-group">
          <input type="text"
                  class="form-control"
                  placeholder="--:--:-- (playtime)"
                  v-model="edited.playtime">
        </div>
      </div>
      <div class="form-row">
        <div class="col form-group">
          <textarea rows="6"
                    class="form-control"
                    placeholder="Comment"
                    v-model="edited.comment"></textarea>
        </div>
      </div>
    </div>
  </div>
  <div class="row">
    <div class="col">
      <div class="form-row">
        <div class="col form-group">
          <h5>Platforms</h5>
          <descriptor-list :items="allPlatforms" v-model="edited.platform_ids"></descriptor-list>
        </div>
        <div class="col form-group">
          <h5>Genres</h5>
          <descriptor-list :items="allGenres" v-model="edited.genre_ids"></descriptor-list>
        </div>
        <div class="col form-group">
          <h5>Tags</h5>
          <descriptor-list :items="allTags" v-model="edited.tag_ids"></descriptor-list>
        </div>
      </div>
      <div class="d-flex justify-content-between">
        <div>
          <button type="submit" class="btn btn-success" :disabled="isSaving">
            Save
          </button>
          <button type="button" class="btn btn-outline-secondary" :disabled="isSaving" @click="cancel">
            Cancel
          </button>
        </div>
        <delete-button :game="game" :disabled="isSaving" v-if="!isNew" @click="remove"></delete-button>
      </div>
    </div>
  </div>
</form>
</template>

<script>
import _ from 'lodash'
import Api from './api'
import { getLink } from './util'
import DescriptorList from './DescriptorList.vue'
import DeleteButton from './GameDeleteButton.vue'
import TitleEditor from './GameTitleEditor.vue'

export default {
  props: {
    user: Object,
    game: Object,
    allGenres: Array,
    allPlatforms: Array,
    allTags: Array,
    api: Api,
    isNew: Boolean,
    isAssisted: Boolean
  },
  data() {
    return {
      isSaving: false,
      edited: _.cloneDeep(this.game),
      imageFile: null,
      imageUrl: null,
      imageRemoved: false,
      completions: [],
      isSuggestionsOpen: false,
      idPlaying: _.uniqueId('_id'), // unique id for checkbox and label
      idFinished: _.uniqueId('_id')
    }
  },
  watch: {
    'edited.title': function (newTitle, oldTitle) {
      // for new games, update sort_as if it's following the title
      if (this.isNew && this.edited.sort_as === oldTitle)
        this.edited.sort_as = newTitle
    }
  },
  methods: {
    save() {
      const createOrUpdate = () => {
        if (this.isNew) {
          const link = getLink(this.user, 'games')
          return this.api.post(link.href, this.edited)
        }

        const link = getLink(this.game, 'self')
        return this.api.put(link.href, this.edited)
      }
      const createOrDeleteImage = () => {
        const link = getLink(this.edited, 'image')

        if (this.imageFile)
          return this.api.postForm(link.href, { 'image': this.imageFile })
            .then(withImage => this.edited.image = uniqueUrl(withImage.image))

        if (this.imageUrl)
          return this.api.post(link.href, { 'image_url': this.imageUrl })
            .then(withImage => this.edited.image = uniqueUrl(withImage.image))

        if (this.edited.image && this.imageRemoved)
          return this.api.del(link.href)
            .then(() => this.edited.image = null)
      }

      if (this.isSaving)
        return

      this.isSaving = true
      createOrUpdate()
        .then(updated => _.assign(this.edited, updated))
        .then(() => createOrDeleteImage())
        .then(() => {
          this.isSaving = false
          this.$emit('save', this.edited)
        })
        .catch(error => {
          this.isSaving = false
          alert(error)
        })
    },
    remove() {
      this.$emit('remove')
    },
    cancel() {
      if (this.isSaving)
        return

      this.$emit('cancel')
    },
    updateImage(fileEvent) {
      this.imageRemoved = false
      this.imageUrl = null
      this.imageFile = _.head(fileEvent.target.files)
    },
    removeImage() {
      // ask for confirmation for existing games (because we can't easily get the image back)
      if (!this.isNew && !confirm('Are you sure you want to delete this image?'))
        return

      this.imageRemoved = true
      this.imageFile = null
      this.imageUrl = null

      // clear the file input by setting its value to null
      this.$refs.imageInput.value = null
    },
    applyAssistance(gbGame) {
      this.edited.title = gbGame.title
      this.edited.developer = gbGame.developer
      this.edited.publisher = gbGame.publisher
      this.edited.year = gbGame.year
      this.edited.genre_ids = gbGame.genre_ids
      this.edited.platform_ids = gbGame.platform_ids

      this.imageUrl = gbGame.image_url
      this.imageRemoved = false
      this.imageFile = null
      this.$refs.imageInput.value = null
    }
  },
  components: {
    'descriptor-list': DescriptorList,
    'delete-button': DeleteButton,
    'title-editor': TitleEditor
  }
}

function uniqueUrl(url) {
  // generate a unique url to get around caching when updating images
  return `${url}?_t=${_.uniqueId()}`
}
</script>

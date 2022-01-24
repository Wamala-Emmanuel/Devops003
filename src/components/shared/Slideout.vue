<template>
  <div>
    <div class="slideout-overlay"  v-bind:class="{ active: isActive}" @click="closeSlideOut"></div>
    <div
      id="slide-out-wrapper"
      v-bind:class="{ active: isActive, display: display }"
    >
      <div class="spinner text-center" v-if="loadSlideOutSpinner && !isFormOpen"
        v-bind:class="{ loadSlideOutSpinner: loadSlideOutSpinner}">
        <i class="fa fa-spinner fa-spin"></i>
      </div>
      <div id="slide-out" v-else>
        <slot name="form"></slot>
        <slot name="details">
        </slot>
        <slot name="edit"></slot>
      </div>
    </div>
  </div>
</template>

<script>
import sharedMethodsMixin from "@/mixins/sharedMethodsMixin";
export default {
  name: "Slideout",
  mixins: [sharedMethodsMixin],
  props: {
    loadSlideOutSpinner: Boolean,
    isFormOpen: Boolean
  },
  data() {
    return {
      isActive: false,
      display: false,
    };
  },
  methods: {
    toggleSlideout() {
      this.display = false;
      this.isActive = true;
      let body = document.getElementsByTagName("body")[0];
      body.classList.add("hide-scroll");
    },
    closeSlideOut() {
      this.isActive = false;
      this.display = true;
      this.detailsToEdit = {};
      let body = document.getElementsByTagName("body")[0];
      body.classList.remove("hide-scroll");
    },
  },
};
</script>

<style scoped>
#slide-out {
  overflow-x: hidden;
}
</style>

<template>
  <div class="deactivate-activate">
    <span v-if="isActive">
      <button class="deactivate-button" @click="changeStatus">Deactivate</button>
    </span>
    <span v-if="!isActive">
      <button class="activate-button" @click="changeStatus">Activate</button>
    </span>
  </div>
</template>

<script>
export default {
  data() {
    return {
      currentState: this.userStatus
    }
  },
  props: {
    userStatus: Boolean,

  },
  watch: {
    userStatus: function (newStatus) {
      this.currentState = newStatus
    }
  },
  computed: {
    isActive() {
      return this.currentState
    },
  },

  methods: {
    changeStatus() {
      this.$emit('activate-deactivate');
    }
  },
  
}
</script>
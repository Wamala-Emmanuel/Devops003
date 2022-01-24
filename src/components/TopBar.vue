<template>
  <nav
    class="navbar navbar-static-top"
    role="navigation"
    style="margin-bottom: 0"
  >
    <div class="navbar-header">
      <a
        class="navbar-minimalize minimalize-styl-2 btn btn-primary"
        href="#"
        @click.prevent="$emit('minimize')"
      >
        <i class="fa fa-bars" aria-label="Minimize nav bar"></i>
      </a>
      <form
        role="search"
        class="navbar-form-custom"
        action="search_results.html"
        v-on:submit.prevent
      >
        <div class="form-group">
          <input
            type="text"
            :placeholder="currentRouteName"
            class="form-control"
            name="top-search"
            id="top-search"
            v-model="searchKeyWord"
            @keyup.enter="$emit('newSearch')"
            @keyup.backspace="$emit('newSearch')"
            aria-label="Type word to search"
          />
        </div>
      </form>
    </div>
    <ul class="nav navbar-top-links navbar-right">
      <li>
        <a @click.prevent="logout"> <i class="fa fa-sign-out"></i> Sign out </a>
      </li>
    </ul>
  </nav>
</template>

<script>
import auth from "@/utils/AuthService";

export default {
  name: "TopBar",
  data() {
    return {
      searchKeyWord: "",
    };
  },
  computed: {
    currentRouteName() {
      if (this.$route.name == "home" || this.$route.name == "null") {
        return `Search...`;
      } else if (this.$route.name !== "home") {
        return `Search for ${this.$route.name}...`;
      } else {
        return ``;
      }
    },
  },
  watch: {
    $route(to, from) {
      this.searchKeyWord = "";
    },
  },
  methods: {
    logout() {
      auth.logout();
      localStorage.removeItem(ACCESS_TOKEN, user.access_token);
    },
  },
};
</script>

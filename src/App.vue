<template>
  <main id="app" v-if="isLoggedIn">
    <div id="wrapper">
      <nav-bar />
      <div class="gray-bg" id="page-wrapper">
        <div class="row border-bottom">
          <top-bar
            @minimize="sidebarCollapse"
            ref="topBar"
            :searchKeyWord="searchWord"
            @newSearch="handleSearch"
          />
        </div>
        <router-view ref="component" />
        <Footer></Footer>
      </div>
    </div>
  </main>
  <main v-else class="main-login-page">
    <Login></Login>
  </main>
</template>

<script>
// @ is an alias to /src
import NavBar from "@/components/NavBar.vue";
import Footer from "@/components/Footer.vue";
import TopBar from "@/components/TopBar.vue";
import { correctHeight, detectBody } from "./utils/app.helper";
import Login from "./views/Login";
import auth from "@/utils/AuthService";
import { ACCESS_TOKEN, CALLBACK_PATH } from "./utils/constants";

export default {
  name: "users",
  data() {
    return {
      collapse: false,
      isLoggedIn: false,
      currentUser: "",
      accessTokenExpired: false,
      searchWord: "",
      notificationMessage:
        "User authentication failed. Invalid account. Please contact the systems administrator.",
    };
  },
  components: {
    Login,
    NavBar,
    Footer,
    TopBar,
  },
  mounted() {
    if (!this.isLoggedIn) {
      localStorage.setItem(CALLBACK_PATH, this.$route.path);
    }
    auth.getUser().then(async (user) => {
      if (user !== null && !user.expired) {
        localStorage.setItem(ACCESS_TOKEN, user.access_token);
        this.currentUser = user.profile.name;
        this.accessTokenExpired = user.expired;
        const userRole = (user.profile.role).toLowerCase()
        if (user && (userRole === "sys_admin" || userRole === "administrator")) {
          this.isLoggedIn = true;
        } else {
          this.$toaster.error(this.notificationMessage, {
            timeout: 10000 * 5 * 5,
          });
          setTimeout(() => {
            auth.logout().then(() => {
              localStorage.removeItem(ACCESS_TOKEN, user.access_token);
              this.isLoggedIn = false;
            });
          }, 10000 * 5);
          
        }
      } else {
        this.isLoggedIn = false;
      }
    });
    // Run correctHeight function on load and resize window event
    $(window).bind("load resize", function () {
      correctHeight();
      detectBody();
    });

    // Correct height of wrapper after metisMenu animation.
    $(".metismenu a").click(() => {
      setTimeout(() => {
        correctHeight();
      }, 300);
    });
  },
  methods: {
    handleSearch() {
      this.searchWord = this.$refs.topBar.searchKeyWord.trim();
      if (this.$route.name == "users" || this.$route.name == "home") {
        if (this.searchWord.length > 2) {
          this.$refs.component.searchUser(this.searchWord);
        } else {
          this.$refs.component.fetchUsers();
        }
      }
      if (this.$route.name == "clients") {
        if (this.searchWord.length > 0) {
          this.$refs.component.searchClient(this.searchWord);
        } else {
          this.$refs.component.fetchClients();
        }
      }
      if (this.$route.name == "resources") {
        if (this.searchWord.length > 0) {
          if (this.$refs.component.isApiResource) {
            this.$refs.component.searchApiResources(this.searchWord);
          } else if (this.$refs.component.isIdentityResource) {
            this.$refs.component.searchIdentityResources(this.searchWord);
          } else {
            console.log("None of the above");
          }
        } else {
          if (this.$refs.component.isApiResource) {
            this.$refs.component.showApiResource();
          } else this.$refs.component.showIdentityResource();
        }
      }
    },
    sidebarCollapse() {
      this.collapse = !this.collapse;
      let body = document.getElementsByTagName("body")[0];
      if (this.collapse) {
        body.classList.add("mini-navbar");
        body.classList.remove("fixed-sidebar");
      } else {
        body.classList.remove("mini-navbar");
        body.classList.add("fixed-sidebar");
      }

      if (!this.collapse || $("body").hasClass("body-small")) {
        // Hide menu in order to smoothly turn on when maximize menu
        $("#side-menu").hide();
        // For smoothly turn on menu
        setTimeout(function () {
          $("#side-menu").fadeIn(400);
        }, 200);
      } else if ($("body").hasClass("fixed-sidebar")) {
        $("#side-menu").hide();
        setTimeout(function () {
          $("#side-menu").fadeIn(400);
        }, 100);
      } else {
        // Remove all inline style from jquery fadeIn function to reset menu state
        $("#side-menu").removeAttr("style");
      }
    },
  },
};
</script>

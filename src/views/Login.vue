<template>
  <div class="login-wrapper gray-bg">
    <div class="container">
      <div class="row">
        <div class="col-5 logo-section">
          <div class="logo-placeholder">
            <img
              class="bou-logo"
              src="../assets/img/auth_images/bou_logo.svg"
              alt="Bank of Ugnada logo"
            />
          </div>
        </div>
        <div class="col-1 horizontal-rule">
          <hr />
        </div>
        <div class="col-6 login-section">
          <div class="pb-2">
            <div class="mb-3 pt-2">
              <p class="sub-heading">AUTHENTICATION & AUTHORIZATION</p>
            </div>
            <div class="pb-2">
              <form class="form">
                <button
                  type="submit"
                  class="btn block full-width"
                  @click.prevent="handleSubmit"
                >
                  sign in
                </button>
              </form>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
  <!-- <div class="gray-bg">
    <div id="logo-name">
      <h1 class="logo-name">AUTH SERVICE</h1>
    </div>
    <div class="middle-div text-center loginscreen animated fadeInDown">
      <div>
        <h3>Login</h3>
        <p>To continue to Construo Auth Service.</p>
        <form class="m-t" role="form">
          <button
            type="submit"
            class="btn btn-primary block full-width m-b"
            @click.prevent="handleSubmit"
          >
            Login
          </button>
        </form>
        <p class="m-t">
          <small>Product of Laboremus Uganda &copy; 2019</small>
        </p>
      </div>
    </div>
  </div> -->
</template>

<script>
import auth from "@/utils/AuthService";
export default {
  name: "Login",
  data() {
    return {};
  },
  created() {
    document.querySelector("body").classList.add("gray-bg");
  },
  destroyed() {
    if (this.$route.name !== "login") {
      document.querySelector("body").classList.remove("gray-bg");
    }
  },
  methods: {
    handleSubmit() {
      auth.login();
    },
  },
};
</script>

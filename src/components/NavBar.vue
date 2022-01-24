<template>
    <nav class="navbar-default navbar-static-side fixed-sidebar" role="navigation" aria-label="navigation menu">
        <div class="sidebar-collapse">
            <ul class="nav metismenu" id="side-menu">
                <li class="nav-header">
                    <div class="logo-element">
                        <img class="icon-logo" src="../assets/img/auth_images/construo-icon.png" alt="logo image"/>
                        <img class="main-logo" src="../assets/img/auth_images/construo-logo.png" alt="logo image"/>
                    </div>
                    <div class="profile-element">
                        <a class="" href="#">
                            <span class="block m-t-xs font-bold" v-if="currentUser">{{currentUser.name}}</span>
                        </a>
                    </div>
                </li>
                    <router-link tag="li" :to="{ name: 'users'}" exact>
                       <a>
                            <i class="fa fa-users"></i> <span class="nav-label ">Users</span>
                       </a>
                    </router-link>
                    <router-link tag="li" :to="{name: 'clients'}" exact>
                       <a>
                            <i class="fa fa-handshake-o"></i> <span class="nav-label">Clients</span>
                       </a>
                    </router-link>
                    <!-- <router-link tag="li" :to="{ name: 'resources'}" exact>
                       <a>
                           <i class="fa fa-briefcase"></i> <span class="nav-label">Resources </span>
                       </a>
                    </router-link> -->
            </ul>
        </div>
    </nav>
</template>

<script>
    /* eslint-disable */
    import auth from '@/utils/AuthService'

    export default {
        name: "NavBar",
        data() {
            return {
                currentUser: undefined,
                isActive: this.$route.path
            }
        },
        mounted () {
            document.querySelector("body").classList.add("pace-done", 'fixed-sidebar');
            $('#side-menu').metisMenu()
            auth.getUser().then((user) => {
                this.currentUser = user.profile;
            });
        }
    }

</script>

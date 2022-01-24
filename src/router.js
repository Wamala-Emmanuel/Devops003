import Vue from 'vue'
import Router from 'vue-router'
import Home from '@/views/Home.vue'
import Users from '@/views/Users.vue'
import Clients from '@/views/Clients.vue'
import Resources from '@/views/Resources.vue'

Vue.use(Router)

export default new Router({
  routes: [
    {
      path: '/',
      name: 'users',
      component: Users // Temporarily set the landing page to show list of users
    },
    {
      name: 'null',
      path: '/null',
      component: Users,
    },
    {
      name: 'users',
      path: '/users',
      component: Users,
    },
    // {
    //   name: 'resources',
    //   path: '/resources',
    //   component: Resources,
    // },
    {
      name: 'clients',
      path: '/clients',
      component: Clients,
    }

  ],
  linkActiveClass: "active"
})

import Vue from 'vue'
import VueRouter from 'vue-router'
import App from './App.vue'
import Setup from './Setup.vue'

const router = new VueRouter({
  routes: [
    {
      path: '/',
      component: App,
      name: 'root'
    },
    {
      path: '/setup',
      component: Setup,
      name: 'setup'
    },
    {
      path: '/:userId',
      component: App,
      name: 'user',
      props(route) {
        return { userId: parseInt(route.params.userId) }
      }
    }
  ]
})

Vue.use(VueRouter)

new Vue({
  el: '#app',
  router: router,
  render: h => h('router-view')
})

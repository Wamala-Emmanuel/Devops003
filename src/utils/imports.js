import '../assets/css/bootstrap.css'
import '../assets/font-awesome/css/font-awesome.css'
import '../assets/css/plugins/select2/select2.min.css'
import '../assets/css/animate.css'
import '../assets/css/style.css'
import '../assets/css/custom.css'
import 'vue-select/dist/vue-select.css'
import Vue from 'vue'

Vue.config.debug = process.env.NODE_ENV !== 'production'
/* ============
 * jQuery
 * ============
 *
 * Require jQuery
 *
 * http://jquery.com/
 */
import jQuery from 'jquery'

window.$ = window.jQuery = jQuery

/* ============
 * Bootstrap
 * ============
 *
 * Require bootstrap
 *
 * http://getbootstrap.com/
 */
require('bootstrap')

require('metismenu')

import ToggleButton from 'vue-js-toggle-button'
Vue.use(ToggleButton)

import { SpinnerPlugin } from 'bootstrap-vue'
Vue.use(SpinnerPlugin)

import Toaster from 'v-toaster'
import 'v-toaster/dist/v-toaster.css'

// import VJsoneditor to use
import VJsoneditor from 'v-jsoneditor/src/index';
Vue.use(VJsoneditor)

// optional set default timeout, the default is 10000 (10 seconds).
Vue.use(Toaster, { timeout: 5000 })

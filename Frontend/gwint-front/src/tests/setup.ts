import { config } from '@vue/test-utils'
import { createVuetify } from 'vuetify'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'

const vuetify = createVuetify({
    components,
    directives,
})

config.global.plugins = [vuetify]

config.global.stubs = {
    transition: false,
    'transition-group': false,
}
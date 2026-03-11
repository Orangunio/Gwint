import { mount } from '@vue/test-utils'
import { describe, expect, it } from 'vitest'
import MenuCard from '../MenuCard.vue'

describe('MenuCard', () => {
    it('renderuje tytuł i opis', () => {
        const wrapper = mount(MenuCard, {
            props: {
                title: 'Gra Online',
                description: 'Zmierz się z innymi graczami.',
                icon: 'mdi-sword-cross',
            },
        })

        expect(wrapper.text()).toContain('Gra Online')
        expect(wrapper.text()).toContain('Zmierz się z innymi graczami.')
    })

    it('renderuje badge, gdy został przekazany', () => {
        const wrapper = mount(MenuCard, {
            props: {
                title: 'Gra Online',
                description: 'Opis',
                icon: 'mdi-sword-cross',
                badge: 'Dostępne',
            },
        })

        expect(wrapper.text()).toContain('Dostępne')
    })

    it('emituje click, gdy komponent nie jest disabled', async () => {
        const wrapper = mount(MenuCard, {
            props: {
                title: 'Gra Online',
                description: 'Opis',
                icon: 'mdi-sword-cross',
                disabled: false,
            },
        })

        await wrapper.trigger('click')

        expect(wrapper.emitted('click')).toBeTruthy()
        expect(wrapper.emitted('click')).toHaveLength(1)
    })

    it('nie emituje click, gdy komponent jest disabled', async () => {
        const wrapper = mount(MenuCard, {
            props: {
                title: 'Gra Online',
                description: 'Opis',
                icon: 'mdi-sword-cross',
                disabled: true,
            },
        })

        await wrapper.trigger('click')

        expect(wrapper.emitted('click')).toBeFalsy()
    })
    it('nie renderuje badge, gdy nie został przekazany', () => {
        const wrapper = mount(MenuCard, {
            props: {
                title: 'Gra Online',
                description: 'Opis',
                icon: 'mdi-sword-cross',
            },
        })

        expect(wrapper.text()).not.toContain('Dostępne')
    })

    it('renderuje overlay z napisem "Wkrótce", gdy komponent jest disabled', () => {
        const wrapper = mount(MenuCard, {
            props: {
                title: 'Gra Online',
                description: 'Opis',
                icon: 'mdi-sword-cross',
                disabled: true,
            },
        })

        expect(wrapper.text()).toContain('Wkrótce')
    })

    it('dodaje klasę disabled, gdy komponent jest zablokowany', () => {
        const wrapper = mount(MenuCard, {
            props: {
                title: 'Gra Online',
                description: 'Opis',
                icon: 'mdi-sword-cross',
                disabled: true,
            },
        })

        expect(wrapper.classes()).toContain('menu-card--disabled')
    })
})
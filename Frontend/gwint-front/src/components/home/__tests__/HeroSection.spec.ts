import { mount } from '@vue/test-utils'
import { describe, expect, it } from 'vitest'
import HeroSection from '../HeroSection.vue'

describe('HeroSection', () => {
    it('renderuje główne teksty sekcji hero', () => {
        const wrapper = mount(HeroSection)

        expect(wrapper.text()).toContain('GWINT')
        expect(wrapper.text()).toContain('Gra karciana')
        expect(wrapper.text()).toContain('Zagraj Teraz')
        expect(wrapper.text()).toContain('Jak Grać?')
    })

    it('renderuje opis gry', () => {
        const wrapper = mount(HeroSection)

        expect(wrapper.text()).toContain('Strategiczna gra karciana')
    })

    it('renderuje wszystkie statystyki', () => {
        const wrapper = mount(HeroSection)

        expect(wrapper.text()).toContain('200+')
        expect(wrapper.text()).toContain('Kart')
        expect(wrapper.text()).toContain('5')
        expect(wrapper.text()).toContain('Frakcji')
        expect(wrapper.text()).toContain('∞')
        expect(wrapper.text()).toContain('Strategii')
    })

    it('renderuje dokładnie 3 elementy statystyk', () => {
        const wrapper = mount(HeroSection)
        const stats = wrapper.findAll('.stat-item')

        expect(stats).toHaveLength(3)
    })

    it('emituje startGame po kliknięciu przycisku "Zagraj Teraz"', async () => {
        const wrapper = mount(HeroSection)
        const buttons = wrapper.findAll('button')
        const startButton = buttons.find(button => button.text().includes('Zagraj Teraz'))

        expect(startButton).toBeDefined()

        await startButton!.trigger('click')

        expect(wrapper.emitted('startGame')).toBeTruthy()
        expect(wrapper.emitted('startGame')).toHaveLength(1)
        expect(wrapper.emitted('learnMore')).toBeFalsy()
    })

    it('emituje learnMore po kliknięciu przycisku "Jak Grać?"', async () => {
        const wrapper = mount(HeroSection)
        const buttons = wrapper.findAll('button')
        const learnMoreButton = buttons.find(button => button.text().includes('Jak Grać?'))

        expect(learnMoreButton).toBeDefined()

        await learnMoreButton!.trigger('click')

        expect(wrapper.emitted('learnMore')).toBeTruthy()
        expect(wrapper.emitted('learnMore')).toHaveLength(1)
        expect(wrapper.emitted('startGame')).toBeFalsy()
    })
})
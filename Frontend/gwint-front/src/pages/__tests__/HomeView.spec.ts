import "@/tests/setup"
import { mount } from "@vue/test-utils"
import { describe, it, expect, vi } from "vitest"
import HomeView from "../HomeView.vue"

vi.mock("@/stores/player", () => ({
  usePlayerStore: () => ({
    isLoggedIn: false,
    player: null,
  }),
}))

vi.mock("@/stores/app", () => ({
  useAppStore: () => ({
    notifications: [],
    addNotification: vi.fn(),
    removeNotification: vi.fn(),
  }),
}))

vi.mock("vue-router", () => ({
  useRouter: () => ({
    push: vi.fn(),
  }),
}))

function createWrapper() {
  return mount(HomeView, {
    global: {
    renderStubDefaultSlot: true,
      stubs: {
        AppHeader: true,
        AppFooter: true,
        HeroSection: true,
        MenuCard: true,
        PlayerStats: true,
        "v-main": true,
        "v-snackbar": true
      }
    }
  })
}

describe("HomeView", () => {

  it("renderuje komponent", () => {
    const wrapper = createWrapper()
    expect(wrapper.exists()).toBe(true)
  })

  it("renderuje nagłówek", () => {
    const wrapper = createWrapper()
    expect(wrapper.find("app-header-stub").exists()).toBe(true)
  })

  it("renderuje stopkę", () => {
    const wrapper = createWrapper()
    expect(wrapper.find("app-footer-stub").exists()).toBe(true)
  })

  it("renderuje HeroSection", () => {
    const wrapper = createWrapper()
    expect(wrapper.find("hero-section-stub").exists()).toBe(true)
  })

  it("renderuje sekcję menu", () => {
    const wrapper = createWrapper()
    expect(wrapper.find("#menu-section").exists()).toBe(true)
  })

  it("renderuje tytuł sekcji", () => {
    const wrapper = createWrapper()
    expect(wrapper.text()).toContain("Co chcesz zrobić?")
  })

  it("komponent poprawnie się montuje", () => {
    const wrapper = createWrapper()
    expect(wrapper.vm).toBeTruthy()
  })

  it("HTML zawiera menu-section", () => {
    const wrapper = createWrapper()
    expect(wrapper.html()).toContain("menu-section")
  })

})
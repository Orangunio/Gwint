import "@/tests/setup"
import { mount } from "@vue/test-utils"
import { describe, it, expect, vi } from "vitest"
import RegisterView from "../RegisterView.vue"

vi.mock("@/stores/player", () => ({
  usePlayerStore: () => ({
    login: vi.fn(),
    clearError: vi.fn(),
    isLoading: false,
    error: null,
  }),
}))

vi.mock("vue-router", () => ({
  useRouter: () => ({
    push: vi.fn(),
  }),
}))

function createWrapper() {
  return mount(RegisterView, {
    global: {
      stubs: {
        AppHeader: true,
        AppFooter: true,
        "v-main": true
      }
    }
  })
}

describe("RegisterView", () => {

  it("renderuje komponent", () => {
    const wrapper = createWrapper()
    expect(wrapper.exists()).toBe(true)
  })

  it("renderuje sekcję auth-page", () => {
    const wrapper = createWrapper()
    expect(wrapper.find(".auth-page").exists()).toBe(true)
  })

  it("komponent mountuje się poprawnie", () => {
  const wrapper = createWrapper()
  expect(wrapper.vm).toBeTruthy()
  })

  it("renderuje sekcję auth-overlay", () => {
  const wrapper = createWrapper()
  expect(wrapper.find(".auth-overlay").exists()).toBe(true)
})

it("renderuje sekcję auth-main", () => {
  const wrapper = createWrapper()
  expect(wrapper.find(".auth-main").exists()).toBe(true)
})

})
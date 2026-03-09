<template>
  <v-card
      class="menu-card"
      :class="{ 'menu-card--disabled': disabled }"
      rounded="xl"
      elevation="0"
      @click="!disabled && $emit('click')"
  >
    <div class="card-glow" :style="{ '--glow-color': glowColor }" />

    <v-card-text class="pa-8 text-center">
      <div class="icon-wrapper mb-5" :style="{ '--icon-color': iconColor }">
        <v-icon :icon="icon" size="48" :color="iconColor" />
      </div>

      <h3 class="card-title mb-2">{{ title }}</h3>
      <p class="text-body-2 text-medium-emphasis mb-4">{{ description }}</p>

      <v-chip
          v-if="badge"
          :color="badgeColor"
          size="x-small"
          variant="tonal"
          class="mt-1"
      >
        {{ badge }}
      </v-chip>

      <div v-if="disabled" class="disabled-overlay">
        <v-icon icon="mdi-lock" size="20" class="mr-1" />
        <span class="text-caption">Wkrótce</span>
      </div>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
withDefaults(
    defineProps<{
      title: string
      description: string
      icon: string
      iconColor?: string
      glowColor?: string
      badge?: string
      badgeColor?: string
      disabled?: boolean
    }>(),
    {
      iconColor: 'amber-darken-2',
      glowColor: 'rgba(255, 215, 64, 0.15)',
      badge: undefined,
      badgeColor: 'green',
      disabled: false,
    }
)

defineEmits<{ click: [] }>()
</script>

<style scoped>
.menu-card {
  position: relative;
  background: rgba(255, 255, 255, 0.03) !important;
  border: 1px solid rgba(255, 255, 255, 0.07);
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  overflow: hidden;
}

.menu-card:hover:not(.menu-card--disabled) {
  transform: translateY(-6px);
  border-color: rgba(255, 215, 64, 0.3);
  background: rgba(255, 255, 255, 0.06) !important;
}

.menu-card:hover .card-glow {
  opacity: 1;
}

.card-glow {
  position: absolute;
  inset: 0;
  background: radial-gradient(circle at 50% 0%, var(--glow-color), transparent 70%);
  opacity: 0;
  transition: opacity 0.4s ease;
  pointer-events: none;
}

.menu-card--disabled {
  opacity: 0.45;
  cursor: not-allowed;
}

.icon-wrapper {
  width: 80px;
  height: 80px;
  border-radius: 50%;
  background: rgba(255, 215, 64, 0.08);
  border: 1px solid rgba(255, 215, 64, 0.15);
  display: flex;
  align-items: center;
  justify-content: center;
  margin-inline: auto;
  transition: all 0.3s ease;
}

.menu-card:hover .icon-wrapper {
  background: rgba(255, 215, 64, 0.12);
  border-color: rgba(255, 215, 64, 0.3);
  box-shadow: 0 0 20px rgba(255, 215, 64, 0.15);
}

.card-title {
  font-size: 1.1rem;
  font-weight: 700;
  letter-spacing: 0.03rem;
}

.disabled-overlay {
  display: flex;
  align-items: center;
  justify-content: center;
  color: rgba(255, 255, 255, 0.4);
  margin-top: 0.5rem;
}
</style>
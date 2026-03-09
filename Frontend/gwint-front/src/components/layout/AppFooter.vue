<template>
  <v-footer class="app-footer" elevation="0">
    <v-container>
      <v-row align="center" justify="space-between">
        <v-col cols="12" sm="auto" class="text-center text-sm-left">
          <div class="d-flex align-center justify-center justify-sm-start ga-2">
            <v-icon icon="mdi-cards-playing" color="amber-darken-2" size="20" />
            <span class="footer-brand">GWINT</span>
          </div>
          <div class="text-caption text-medium-emphasis mt-1">
            Karciana gra strategiczna
          </div>
        </v-col>

        <v-col cols="12" sm="auto" class="text-center">
          <div class="text-caption text-medium-emphasis">
            &copy; {{ currentYear }} Gwint Project — Stworzone z
            <v-icon icon="mdi-heart" color="red-darken-2" size="14" />
          </div>
        </v-col>
      </v-row>
    </v-container>
  </v-footer>
</template>

<script setup lang="ts">
const currentYear = new Date().getFullYear()
</script>

<style scoped>
.app-footer {
  background: rgba(18, 18, 18, 0.97) !important;
  border-top: 1px solid rgba(255, 215, 64, 0.1);
}

.footer-brand {
  font-weight: 900;
  letter-spacing: 0.2rem;
  color: #ffd740;
  font-size: 0.85rem;
}
</style>
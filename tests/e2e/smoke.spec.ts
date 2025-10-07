import { test, expect } from '@playwright/test';

/**
 * Smoke test to verify app loads
 *
 * Full E2E test suite deferred to Week 10 per PRD.
 * This stub test verifies Playwright infrastructure is working.
 */
test('app should load and display title', async ({ page }) => {
  await page.goto('/');

  // Wait for app to load
  await page.waitForLoadState('networkidle');

  // Verify title contains "Ledgerly"
  await expect(page).toHaveTitle(/Ledgerly/);
});

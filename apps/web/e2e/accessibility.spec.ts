import { expect, test } from "@playwright/test";

test.describe("Frontend WCAG 2.1 Accessibility Audit", () => {
  test("index page must have correct html language and skip-link landmark", async ({ page }) => {
    await page.goto("/");

    // 1. Verify language parameter
    const htmlLang = await page.locator("html").getAttribute("lang");
    expect(htmlLang).toBe("en");

    // 2. Verify skip-link is the first focusable element
    const skipLink = page.locator("a.skip-link, a:has-text('Skip to main content')");
    await expect(skipLink).toBeAttached();

    // 3. Tab to skip link and assert focus is active
    await page.keyboard.press("Tab");
    await expect(skipLink).toBeFocused();
  });

  test("forms must have valid accessible labels and input descriptions", async ({ page }) => {
    await page.goto("/");

    // Verify email input has associated label
    const emailInput = page.locator("input[type='email']");
    if (await emailInput.isVisible()) {
      const emailId = await emailInput.getAttribute("id");
      expect(emailId).not.toBeNull();
      
      const label = page.locator(`label[for='${emailId}']`);
      await expect(label).toBeVisible();
    }
  });
});

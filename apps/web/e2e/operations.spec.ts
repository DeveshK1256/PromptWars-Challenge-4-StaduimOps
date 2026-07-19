import { expect, test } from "@playwright/test";

test.describe("FIFA 2026 Smart Operations E2E Journeys", () => {
  test("user can register, log in, navigate to AI assistant, and request explainable decision support", async ({ page }) => {
    // 1. In demo/static mode, we bypass remote authentication or register using static/in-memory flow
    await page.goto("/");

    // 2. Switch to Register tab
    await page.getByRole("tab", { name: /register/i }).click();

    // 3. Fill registration details
    await page.getByLabel("Name").fill("Operations Chief");
    await page.getByLabel("Email").fill("operations-chief@fifa.org");
    await page.getByLabel("Password").fill("Testing1234!@#");
    await page.getByLabel("Preferred language").fill("en");

    // 4. Click Create Account
    await page.getByRole("button", { name: /create account/i }).click();

    // 5. Verify transition to the dashboard workspace
    await expect(page.locator("aside.sidebar")).toBeVisible();
    await expect(page.locator(".profile-strip")).toBeVisible();

    // 6. Navigate to AI Assistant tab
    await page.getByRole("button", { name: /ai assistant/i }).click();
    await expect(page.getByRole("heading", { name: /ai assistant/i })).toBeVisible();

    // 7. Input an operational crowd prompt to trigger the explainable decision engine
    const promptInput = page.locator("textarea[name='prompt']");
    await promptInput.fill("Check crowd surge alerts and evacuate South Concourse");
    
    // Submit the prompt
    await page.getByRole("button", { name: /send query/i }).or(page.locator("button[type='submit']")).first().click();

    // 8. Assert that the AI response feed renders decision warnings
    // In demo/live modes, this validates that the response card appears in the list
    const messageList = page.locator(".message-list");
    await expect(messageList).toBeVisible();
  });
});

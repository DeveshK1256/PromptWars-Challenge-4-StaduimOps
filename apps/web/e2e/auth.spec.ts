import { expect, test } from "@playwright/test";

test("auth screen exposes login and registration controls", async ({ page }) => {
  await page.goto("/");

  await expect(page.getByRole("heading", { name: /sign in to operations/i })).toBeVisible();
  await page.getByRole("tab", { name: /register/i }).click();
  await expect(page.getByLabel("Role")).toBeVisible();
  await expect(page.getByRole("button", { name: /create account/i })).toBeVisible();
});

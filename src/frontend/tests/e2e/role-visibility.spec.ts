import { test, expect } from '@playwright/test';

test.describe('Upload Meter Readings Section Visibility', () => {
  test('should show upload section when user role is manager', async ({ page }) => {
    // Navigate to meter readings page
    await page.goto('/meter-readings');
    
    // Wait for the page to load
    await expect(page.locator('h2')).toHaveText('Meter Readings');
    
    // By default, the app initializes with 'anonymous' role
    // Need to check the current role and toggle to manager if needed
    
    // Look for the role toggle checkbox in header
    const roleToggle = page.getByTestId('role-toggle');
    
    // If current role is not manager (checkbox not checked), toggle to manager
    const isChecked = await roleToggle.isChecked();
    if (!isChecked) {
      await roleToggle.click();
      // Wait for role change to take effect
      await expect(roleToggle).toBeChecked({ timeout: 5000 });
    }
    
    // Check that section description mentions uploading for managers
    await expect(page.getByTestId('section-description')).toContainText(
      'and upload new readings via CSV'
    );
    
    // Check that CSV upload component is visible
    await expect(page.getByTestId('csv-upload')).toBeVisible({ timeout: 5000 });
    
    // Verify upload elements are present
    await expect(page.getByTestId('csv-file-input')).toBeVisible();
    await expect(page.getByTestId('upload-button')).toBeVisible();
  });

  test('should hide upload section when user role is anonymous', async ({ page }) => {
    // Navigate to meter readings page
    await page.goto('/meter-readings');
    
    // Wait for the page to load
    await expect(page.locator('h2')).toHaveText('Meter Readings');
    
    // Look for the role toggle checkbox
    const roleToggle = page.getByTestId('role-toggle');
    
    // Ensure we're in anonymous mode
    const isChecked = await roleToggle.isChecked();
    if (isChecked) {
      await roleToggle.click();
      // Wait for role change to take effect
      await expect(roleToggle).not.toBeChecked({ timeout: 5000 });
    }
    
    // Check that section description does NOT mention uploading
    const description = page.getByTestId('section-description');
    await expect(description).toHaveText('View all meter readings in the system');
    await expect(description).not.toContainText('and upload new readings via CSV');
    
    // Check that CSV upload component is NOT visible
    await expect(page.getByTestId('csv-upload')).not.toBeVisible();
    
    // Verify upload elements are NOT present
    await expect(page.getByTestId('csv-file-input')).not.toBeVisible();
  });

  test('should toggle upload section visibility when role changes', async ({ page }) => {
    // Navigate to meter readings page
    await page.goto('/meter-readings');
    
    // Wait for the page to load
    await expect(page.locator('h2')).toHaveText('Meter Readings');
    
    // Find the role toggle checkbox
    const roleToggle = page.getByTestId('role-toggle');
    
    // Start with anonymous role (default)
    await expect(roleToggle).not.toBeChecked({ timeout: 5000 });
    await expect(page.getByTestId('csv-upload')).not.toBeVisible();
    
    // Toggle to manager role
    await roleToggle.click();
    await expect(roleToggle).toBeChecked({ timeout: 5000 });
    
    // Upload section should now be visible
    await expect(page.getByTestId('csv-upload')).toBeVisible({ timeout: 5000 });
    
    // Toggle back to anonymous role
    await roleToggle.click();
    await expect(roleToggle).not.toBeChecked({ timeout: 5000 });
    
    // Upload section should be hidden again
    await expect(page.getByTestId('csv-upload')).not.toBeVisible();
  });
});

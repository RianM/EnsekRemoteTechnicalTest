import { test, expect } from '@playwright/test';

test.describe('Accounts Page', () => {
  test('should show accounts when there are accounts coming from the API', async ({ page }) => {
    // Navigate to the accounts page
    await page.goto('/accounts');
    
    // Wait for the page to load
    await expect(page.locator('h2')).toHaveText('Accounts');
    
    // Check that the section description is present
    await expect(page.getByTestId('section-description')).toHaveText(
      'View all energy meter accounts in the system'
    );
    
    // Wait for accounts to load (should show loading state first, then data)
    await expect(page.getByTestId('data-table')).toBeVisible({ timeout: 10000 });
    
    // Check that accounts are displayed in the table
    // The table should have header row
    await expect(page.getByTestId('data-table').locator('thead tr')).toBeVisible();
    
    // Check for account ID column header
    await expect(page.getByTestId('data-table').locator('thead')).toContainText('Account ID');
    
    // Check for first and last name columns
    await expect(page.getByTestId('data-table').locator('thead')).toContainText('First Name');
    await expect(page.getByTestId('data-table').locator('thead')).toContainText('Last Name');
    
    // Verify that account data rows are present (tbody should have rows)
    const tableRows = page.getByTestId('data-table').locator('tbody tr');
    await expect(tableRows.first()).toBeVisible({ timeout: 5000 });
    
    // Verify at least one account is displayed
    await expect(tableRows).not.toHaveCount(0);
  });

  test('should handle loading state', async ({ page }) => {
    await page.goto('/accounts');
    
    // Should show the accounts section immediately
    await expect(page.locator('h2')).toHaveText('Accounts');
    
    // Loading state should be handled gracefully
    // (The component shows loading prop to AccountsTable)
    await expect(page.getByTestId('data-table')).toBeVisible({ timeout: 10000 });
  });

  test('should handle error state', async ({ page }) => {
    // Intercept API call and make it fail
    await page.route('**/api/accounts', route => {
      route.fulfill({
        status: 500,
        body: 'Internal Server Error'
      });
    });

    await page.goto('/accounts');
    
    // Should show error message
    await expect(page.getByTestId('error-message')).toHaveText('Failed to fetch accounts');
  });
});

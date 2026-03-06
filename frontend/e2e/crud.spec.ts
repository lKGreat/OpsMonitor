import { expect, test } from '@playwright/test';

test('admin can complete channels/users/monitors CRUD flows', async ({ page }) => {
  test.setTimeout(180000);

  const stamp = Date.now();
  const channelName = `E2E Channel ${stamp}`;
  const channelUpdatedName = `${channelName} Updated`;
  const userName = `e2e_user_${stamp}`;
  const userUpdatedDisplayName = `E2E User ${stamp} Updated`;
  const monitorName = `E2E Monitor ${stamp}`;
  const monitorUpdatedName = `${monitorName} Updated`;

  await page.goto('/login');
  await page.getByTestId('login-username').fill('admin');
  await page.getByTestId('login-password').fill('ChangeMe123!');
  await page.getByTestId('login-submit').click();
  await expect(page).toHaveURL(/\/dashboard$/);

  await page.goto('/settings/channels');
  await page.getByTestId('channel-create-name').fill(channelName);
  await page.getByTestId('channel-create-webhook').fill(`https://oapi.dingtalk.com/robot/send?access_token=${stamp}`);
  await page.getByTestId('channel-create-secret').fill('e2e-secret');
  await page.getByTestId('channel-create-submit').click();

  const channelRow = page.locator('tbody tr').filter({ hasText: channelName });
  await expect(channelRow).toBeVisible();

  await channelRow.locator('button[data-testid^="channel-edit-"]').click();
  await channelRow.locator('input[data-testid^="channel-edit-name-"]').fill(channelUpdatedName);
  await channelRow
    .locator('input[data-testid^="channel-edit-webhook-"]')
    .fill(`https://oapi.dingtalk.com/robot/send?access_token=${stamp}99`);
  await channelRow.locator('button[data-testid^="channel-save-"]').click();

  const channelUpdatedRow = page.locator('tbody tr').filter({ hasText: channelUpdatedName });
  await expect(channelUpdatedRow).toBeVisible();
  await channelUpdatedRow.locator('button[data-testid^="channel-delete-"]').click();
  await expect(page.locator('tbody tr').filter({ hasText: channelUpdatedName })).toHaveCount(0);

  await page.goto('/settings/users');
  await page.getByTestId('user-create-username').fill(userName);
  await page.getByTestId('user-create-display-name').fill(`E2E User ${stamp}`);
  await page.getByTestId('user-create-password').fill('TempPass123!');
  await page.getByTestId('user-create-submit').click();

  const userRow = page.locator('tbody tr').filter({ hasText: userName });
  await expect(userRow).toBeVisible();

  await userRow.locator('button[data-testid^="user-edit-"]').click();
  await userRow.locator('input[data-testid^="user-edit-display-name-"]').fill(userUpdatedDisplayName);
  await userRow.locator('select[data-testid^="user-edit-role-"]').selectOption('Admin');
  await userRow.locator('input[data-testid^="user-edit-new-password-"]').fill('TempPass456!');
  await userRow.locator('button[data-testid^="user-save-"]').click();

  const userUpdatedRow = page.locator('tbody tr').filter({ hasText: userUpdatedDisplayName });
  await expect(userUpdatedRow).toBeVisible();
  await userUpdatedRow.locator('button[data-testid^="user-delete-"]').click();
  await expect(page.locator('tbody tr').filter({ hasText: userName })).toHaveCount(0);

  await page.goto('/monitors');
  await page.getByTestId('monitor-create-entry').click();
  await expect(page).toHaveURL(/\/monitors\/new$/);

  await page.getByTestId('monitor-wizard-name').fill(monitorName);
  await page.getByTestId('monitor-wizard-group').fill('E2E');
  await page.getByTestId('monitor-wizard-next').click();

  await page.getByTestId('monitor-wizard-target').fill('example.com');
  await page.getByTestId('monitor-wizard-port').fill('443');
  await page.getByTestId('monitor-wizard-path').fill('/health');
  await page.getByTestId('monitor-wizard-next').click();

  await page.getByTestId('monitor-wizard-interval').fill('60');
  await page.getByTestId('monitor-wizard-timeout').fill('5000');
  await page.getByTestId('monitor-wizard-retry').fill('1');
  await page.getByTestId('monitor-wizard-success-rule').fill('200-399');
  await page.getByTestId('monitor-wizard-next').click();

  await page.getByTestId('monitor-wizard-fail-threshold').fill('3');
  await page.getByTestId('monitor-wizard-next').click();

  await page.getByTestId('monitor-wizard-channel-ids').fill('[]');
  await page.getByTestId('monitor-wizard-submit').click();
  await expect(page).toHaveURL(/\/monitors\/\d+$/);
  await expect(page.locator('strong')).toContainText(monitorName);

  await page.getByTestId('monitor-edit').click();
  await page.getByTestId('monitor-name-input').fill(monitorUpdatedName);
  await page.getByTestId('monitor-group-input').fill('E2E-Updated');
  await page.getByTestId('monitor-save').click();
  await expect(page.locator('strong')).toContainText(monitorUpdatedName);

  await page.goto('/monitors');
  const monitorRow = page.locator('tbody tr').filter({ hasText: monitorUpdatedName });
  await expect(monitorRow).toBeVisible();
  await monitorRow.locator('button[data-testid^="monitor-delete-"]').click();
  await expect(page.locator('tbody tr').filter({ hasText: monitorUpdatedName })).toHaveCount(0);
});

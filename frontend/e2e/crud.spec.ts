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
  const loginResponsePromise = page.waitForResponse(
    (response) => response.url().includes('/api/auth/login') && response.request().method() === 'POST'
  );
  const dashboardResponsePromise = page.waitForResponse((response) =>
    response.url().includes('/api/dashboard/summary')
  );
  await page.getByTestId('login-submit').click();
  const loginResponse = await loginResponsePromise;
  const loginResponseText = await loginResponse.text();
  expect(loginResponse.status(), `login failed: ${loginResponseText}`).toBe(200);
  const loginPayload = JSON.parse(loginResponseText) as Record<string, unknown>;
  const tokenCandidate = loginPayload.accessToken ?? loginPayload.AccessToken ?? loginPayload.token ?? loginPayload.Token;
  expect(tokenCandidate, `unexpected login payload keys: ${Object.keys(loginPayload).join(',')}; payload=${loginResponseText}`).toBeTruthy();
  const dashboardResponse = await dashboardResponsePromise;
  expect(dashboardResponse.status(), `dashboard failed with status ${dashboardResponse.status()}`).toBe(200);
  await expect(page).toHaveURL(/\/dashboard$/);

  await page.locator('a[href="/settings/channels"]').click();
  await expect(page).toHaveURL(/\/settings\/channels$/);
  await page.getByTestId('channel-create-name').fill(channelName);
  await page.getByTestId('channel-create-webhook').fill(`https://oapi.dingtalk.com/robot/send?access_token=${stamp}`);
  await page.getByTestId('channel-create-secret').fill('e2e-secret');
  await page.getByTestId('channel-create-submit').click();

  const channelRow = page.locator('tbody tr').filter({ hasText: channelName });
  await expect(channelRow).toBeVisible();

  await channelRow.locator('button[data-testid^="channel-edit-"]').click();
  await page.locator('input[data-testid^="channel-edit-name-"]').first().fill(channelUpdatedName);
  await page
    .locator('input[data-testid^="channel-edit-webhook-"]')
    .first()
    .fill(`https://oapi.dingtalk.com/robot/send?access_token=${stamp}99`);
  await page.locator('button[data-testid^="channel-save-"]').first().click();

  const channelUpdatedRow = page.locator('tbody tr').filter({ hasText: channelUpdatedName });
  await expect(channelUpdatedRow).toBeVisible();
  await channelUpdatedRow.locator('button[data-testid^="channel-delete-"]').click();
  await expect(page.locator('tbody tr').filter({ hasText: channelUpdatedName })).toHaveCount(0);

  await page.locator('a[href="/settings/users"]').click();
  await expect(page).toHaveURL(/\/settings\/users$/);
  await page.getByTestId('user-create-username').fill(userName);
  await page.getByTestId('user-create-display-name').fill(`E2E User ${stamp}`);
  await page.getByTestId('user-create-password').fill('TempPass123!');
  const createUserResponsePromise = page.waitForResponse(
    (response) => response.url().includes('/api/users') && response.request().method() === 'POST'
  );
  await page.getByTestId('user-create-submit').click();
  const createUserResponse = await createUserResponsePromise;
  const createUserResponseBody = await createUserResponse.text();
  expect(createUserResponse.status(), `create user failed: ${createUserResponseBody}`).toBe(200);

  const userRow = page.locator('tbody tr').filter({ hasText: userName });
  await expect(userRow).toBeVisible();

  await userRow.locator('button[data-testid^="user-edit-"]').click();
  await page.locator('input[data-testid^="user-edit-display-name-"]').first().fill(userUpdatedDisplayName);
  await page.locator('select[data-testid^="user-edit-role-"]').first().selectOption('Admin');
  await page.locator('input[data-testid^="user-edit-new-password-"]').first().fill('TempPass456!');
  await page.locator('button[data-testid^="user-save-"]').first().click();

  const userUpdatedRow = page.locator('tbody tr').filter({ hasText: userUpdatedDisplayName });
  await expect(userUpdatedRow).toBeVisible();
  await userUpdatedRow.locator('button[data-testid^="user-delete-"]').click();
  await expect(page.locator('tbody tr').filter({ hasText: userName })).toHaveCount(0);

  await page.locator('a[href="/monitors"]').click();
  await expect(page).toHaveURL(/\/monitors$/);
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

  await page.locator('a[href="/monitors"]').click();
  await expect(page).toHaveURL(/\/monitors$/);
  const monitorRow = page.locator('tbody tr').filter({ hasText: monitorUpdatedName });
  await expect(monitorRow).toBeVisible();
  await monitorRow.locator('button[data-testid^="monitor-delete-"]').click();
  await expect(page.locator('tbody tr').filter({ hasText: monitorUpdatedName })).toHaveCount(0);
});

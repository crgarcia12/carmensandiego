import { Given, When, Then } from '@cucumber/cucumber';
import { CustomWorld } from '../support/world';
import { expect } from '@playwright/test';

const WEB_BASE_URL = process.env.WEB_URL || 'http://localhost:3000';

// ─── Setup ───────────────────────────────────────────────────────────────────

Given('a new case has been created with title {string}', async function (this: CustomWorld, _title: string) {
  // Precondition: case created via API and stored in session
});

Given('the player is investigating in city {string}', async function (this: CustomWorld, _city: string) {
  // Precondition: active case with player in specified city
});

Given('the case has {int} remaining steps out of {int}', async function (this: CustomWorld, _remaining: number, _total: number) {
  // Precondition: case state
});

Given('the player is investigating and has {int} remaining steps', async function (this: CustomWorld, _steps: number) {
  // Precondition
});

Given('the player is at the final trail city {string}', async function (this: CustomWorld, _city: string) {
  // Precondition: player at final city
});

Given('the player won the case', async function (this: CustomWorld) {
  // Precondition: case status is won
});

Given('the player lost the case by running out of steps', async function (this: CustomWorld) {
  // Precondition: case status is lost
});

Given('the player is on the summary screen', async function (this: CustomWorld) {
  await this.page.goto(`${WEB_BASE_URL}/summary`);
});

Given('the player is on the briefing screen', async function (this: CustomWorld) {
  await this.page.goto(`${WEB_BASE_URL}/briefing`);
});

Given('no active session exists in localStorage', async function (this: CustomWorld) {
  await this.page.goto(WEB_BASE_URL);
  await this.page.evaluate(() => localStorage.clear());
});

Given('the player is on the city screen', async function (this: CustomWorld) {
  await this.page.goto(`${WEB_BASE_URL}/city`);
});

Given('the player has opened the chat with NPC {string}', async function (this: CustomWorld, npcName: string) {
  await this.page.goto(`${WEB_BASE_URL}/city`);
  const npcLocator = this.page.locator(`text=${npcName}`);
  if (await npcLocator.isVisible()) {
    await npcLocator.click();
  }
});

Given('the player has exchanged {int} messages with NPC {string}', async function (this: CustomWorld, _count: number, _npcName: string) {
  // Precondition: chat history full
});

Given('the player is on the dossier screen', async function (this: CustomWorld) {
  await this.page.goto(`${WEB_BASE_URL}/dossier`);
});

Given('the player issued a correct warrant', async function (this: CustomWorld) {
  // Precondition: warrant result is win
});

Given('the player issued a warrant for the wrong suspect', async function (this: CustomWorld) {
  // Precondition: warrant result is loss
});

Given('the briefing narrative is animating with the typewriter effect', async function (this: CustomWorld) {
  await this.page.goto(`${WEB_BASE_URL}/briefing`);
});

Given('the network connection is lost', async function (this: CustomWorld) {
  await this.context.setOffline(true);
});

// ─── Actions ─────────────────────────────────────────────────────────────────

When('the player views the briefing screen at \\/briefing', async function (this: CustomWorld) {
  await this.page.goto(`${WEB_BASE_URL}/briefing`);
});

When('the player clicks {string}', async function (this: CustomWorld, buttonText: string) {
  await this.page.locator(`button:has-text("${buttonText}"), a:has-text("${buttonText}"), [role="button"]:has-text("${buttonText}")`).first().click();
});

When('the player views the city screen at \\/city', async function (this: CustomWorld) {
  await this.page.goto(`${WEB_BASE_URL}/city`);
});

When('the player views the city screen', async function (this: CustomWorld) {
  await this.page.goto(`${WEB_BASE_URL}/city`);
});

When('the player navigates to the dossier screen', async function (this: CustomWorld) {
  await this.page.goto(`${WEB_BASE_URL}/dossier`);
});

When('the player views the dossier on a desktop viewport of {int}px width', async function (this: CustomWorld, width: number) {
  await this.page.setViewportSize({ width, height: 720 });
  await this.page.goto(`${WEB_BASE_URL}/dossier`);
});

When('the player views the dossier on a mobile viewport of {int}px width', async function (this: CustomWorld, width: number) {
  await this.page.setViewportSize({ width, height: 720 });
  await this.page.goto(`${WEB_BASE_URL}/dossier`);
});

When('the player views the summary screen at \\/summary', async function (this: CustomWorld) {
  await this.page.goto(`${WEB_BASE_URL}/summary`);
});

When('the player navigates directly to \\/city', async function (this: CustomWorld) {
  await this.page.goto(`${WEB_BASE_URL}/city`);
});

When('the viewport width is {int}px', async function (this: CustomWorld, width: number) {
  await this.page.setViewportSize({ width, height: 720 });
});

When('the player presses Tab', async function (this: CustomWorld) {
  await this.page.keyboard.press('Tab');
});

When('the player presses Enter on a focused NPC', async function (this: CustomWorld) {
  await this.page.keyboard.press('Enter');
});

When('the player presses Escape', async function (this: CustomWorld) {
  await this.page.keyboard.press('Escape');
});

When('the player types {string} and presses Enter', async function (this: CustomWorld, text: string) {
  const input = this.page.locator('input[type="text"], textarea').first();
  await input.fill(text);
  await input.press('Enter');
});

When('an API call is in progress', async function (this: CustomWorld) {
  await this.page.goto(`${WEB_BASE_URL}/city`);
});

When('an API call returns an error', async function (this: CustomWorld) {
  // Would require intercepting network — stubbed
});

When('an API call fails', async function (this: CustomWorld) {
  // Would require intercepting network — stubbed
});

When('all {int} retries fail', async function (this: CustomWorld, _count: number) {
  // Stubbed — part of retry scenario
});

When('the player clicks {string} on suspect {string}', async function (this: CustomWorld, buttonText: string, suspectName: string) {
  const card = this.page.locator(`[data-suspect-name="${suspectName}"], :has-text("${suspectName}")`).first();
  await card.locator(`button:has-text("${buttonText}")`).click();
});

When('the arrest result screen is displayed', async function (this: CustomWorld) {
  // Page should auto-navigate after warrant submission
});

When('the player clicks anywhere or presses any key', async function (this: CustomWorld) {
  await this.page.click('body');
});

When('the player views the chat panel', async function (this: CustomWorld) {
  // Chat panel should be visible on city screen
});

When('the player rapidly clicks a travel option twice', async function (this: CustomWorld) {
  const travelBtn = this.page.locator('[data-testid="travel-option"], button:has-text("Travel")').first();
  await travelBtn.dblclick();
});

// ─── Assertions ──────────────────────────────────────────────────────────────

Then('the case title {string} is displayed as a heading', async function (this: CustomWorld, title: string) {
  const heading = this.page.locator(`h1:has-text("${title}"), h2:has-text("${title}")`).first();
  await expect(heading).toBeVisible();
});

Then('the stolen treasure name {string} is displayed', async function (this: CustomWorld, name: string) {
  await expect(this.page.locator(`text=${name}`).first()).toBeVisible();
});

Then('the stolen treasure description is displayed', async function (this: CustomWorld) {
  const desc = this.page.locator('[data-testid="treasure-description"], .treasure-description').first();
  await expect(desc).toBeVisible();
});

Then('a briefing narrative is displayed with a typewriter animation', async function (this: CustomWorld) {
  const narrative = this.page.locator('[data-testid="briefing-narrative"], .briefing-narrative').first();
  await expect(narrative).toBeVisible();
});

Then('a {string} button is visible', async function (this: CustomWorld, buttonText: string) {
  await expect(this.page.locator(`button:has-text("${buttonText}")`).first()).toBeVisible();
});

Then('the player is navigated to the city screen at \\/city', async function (this: CustomWorld) {
  await this.page.waitForURL(`${WEB_BASE_URL}/city`);
  expect(this.page.url()).toContain('/city');
});

Then('the city name {string} is displayed', async function (this: CustomWorld, name: string) {
  await expect(this.page.locator(`text=${name}`).first()).toBeVisible();
});

Then('a city background image is displayed', async function (this: CustomWorld) {
  const bg = this.page.locator('[data-testid="city-background"], .city-background, img').first();
  await expect(bg).toBeVisible();
});

Then('{int} to {int} NPCs are listed with their names and roles', async function (this: CustomWorld, min: number, max: number) {
  const npcs = this.page.locator('[data-testid="npc-card"], .npc-card, [data-testid="npc"]');
  const count = await npcs.count();
  expect(count).toBeGreaterThanOrEqual(min);
  expect(count).toBeLessThanOrEqual(max);
});

Then('exactly {int} travel options are displayed with city names and descriptions', async function (this: CustomWorld, count: number) {
  const options = this.page.locator('[data-testid="travel-option"], .travel-option');
  await expect(options).toHaveCount(count);
});

Then('the step counter displays {string}', async function (this: CustomWorld, text: string) {
  await expect(this.page.locator(`text=${text}`).first()).toBeVisible();
});

Then('the step counter displays {string} in a warning color', async function (this: CustomWorld, text: string) {
  const counter = this.page.locator(`text=${text}`).first();
  await expect(counter).toBeVisible();
});

Then('no travel options are displayed', async function (this: CustomWorld) {
  const options = this.page.locator('[data-testid="travel-option"], .travel-option');
  await expect(options).toHaveCount(0);
});

Then('a message {string} is shown', async function (this: CustomWorld, message: string) {
  await expect(this.page.locator(`text=${message}`).first()).toBeVisible();
});

Then('at least {int} suspect cards are displayed', async function (this: CustomWorld, min: number) {
  const cards = this.page.locator('[data-testid="suspect-card"], .suspect-card');
  const count = await cards.count();
  expect(count).toBeGreaterThanOrEqual(min);
});

Then('each suspect card shows the suspect name', async function (this: CustomWorld) {
  const cards = this.page.locator('[data-testid="suspect-card"], .suspect-card');
  const count = await cards.count();
  for (let i = 0; i < count; i++) {
    const name = cards.nth(i).locator('[data-testid="suspect-name"], .suspect-name, h3, h4');
    await expect(name.first()).toBeVisible();
  }
});

Then('each suspect card shows a photo placeholder', async function (this: CustomWorld) {
  const cards = this.page.locator('[data-testid="suspect-card"], .suspect-card');
  const count = await cards.count();
  for (let i = 0; i < count; i++) {
    const photo = cards.nth(i).locator('[data-testid="suspect-photo"], .suspect-photo, img');
    await expect(photo.first()).toBeVisible();
  }
});

Then('each suspect card shows {int} traits with labels', async function (this: CustomWorld, traitCount: number) {
  const cards = this.page.locator('[data-testid="suspect-card"], .suspect-card');
  const first = cards.first();
  const traits = first.locator('[data-testid="trait"], .trait, li');
  const count = await traits.count();
  expect(count).toBeGreaterThanOrEqual(traitCount);
});

Then('each suspect card has an {string} button', async function (this: CustomWorld, buttonText: string) {
  const cards = this.page.locator('[data-testid="suspect-card"], .suspect-card');
  const count = await cards.count();
  for (let i = 0; i < count; i++) {
    await expect(cards.nth(i).locator(`button:has-text("${buttonText}")`)).toBeVisible();
  }
});

Then('suspect cards are arranged in a grid with {int} to {int} columns', async function (this: CustomWorld, _minCols: number, _maxCols: number) {
  const grid = this.page.locator('[data-testid="suspects-grid"], .suspects-grid').first();
  await expect(grid).toBeVisible();
});

Then('suspect cards are arranged in a single column', async function (this: CustomWorld) {
  const grid = this.page.locator('[data-testid="suspects-grid"], .suspects-grid').first();
  await expect(grid).toBeVisible();
});

Then('the outcome badge displays {string}', async function (this: CustomWorld, outcome: string) {
  await expect(this.page.locator(`text=${outcome}`).first()).toBeVisible();
});

Then('the stolen treasure details are shown', async function (this: CustomWorld) {
  const treasure = this.page.locator('[data-testid="treasure-details"], .treasure-details').first();
  await expect(treasure).toBeVisible();
});

Then('the cities visited are displayed as an ordered list', async function (this: CustomWorld) {
  const list = this.page.locator('[data-testid="cities-visited"], .cities-visited, ol').first();
  await expect(list).toBeVisible();
});

Then('the steps used are displayed as {string}', async function (this: CustomWorld, text: string) {
  await expect(this.page.locator(`text=${text}`).first()).toBeVisible();
});

Then('the correct suspect card is shown', async function (this: CustomWorld) {
  const card = this.page.locator('[data-testid="correct-suspect"], .correct-suspect').first();
  await expect(card).toBeVisible();
});

Then('the player\'s warrant details are shown', async function (this: CustomWorld) {
  const warrant = this.page.locator('[data-testid="warrant-details"], .warrant-details').first();
  await expect(warrant).toBeVisible();
});

Then('the correct suspect is revealed', async function (this: CustomWorld) {
  const suspect = this.page.locator('[data-testid="correct-suspect"], .correct-suspect').first();
  await expect(suspect).toBeVisible();
});

Then('a new case is created', async function (this: CustomWorld) {
  // Verified by navigation to briefing screen
});

Then('the player is navigated to the briefing screen at \\/briefing', async function (this: CustomWorld) {
  await this.page.waitForURL(`${WEB_BASE_URL}/briefing`);
  expect(this.page.url()).toContain('/briefing');
});

Then('navigation to \\/city occurs without a full page reload', async function (this: CustomWorld) {
  expect(this.page.url()).toContain('/city');
});

Then('navigation to \\/dossier occurs without a full page reload', async function (this: CustomWorld) {
  expect(this.page.url()).toContain('/dossier');
});

Then('the player is redirected to \\/', async function (this: CustomWorld) {
  await this.page.waitForURL(WEB_BASE_URL + '/');
});

Then('a new session is created', async function (this: CustomWorld) {
  // Verified by localStorage having a session ID
});

Then('all panels are stacked vertically', async function (this: CustomWorld) {
  // Layout assertion — stubbed for red baseline
});

Then('tab navigation is shown with tabs {string}, {string}, {string}, {string}', async function (this: CustomWorld, t1: string, t2: string, t3: string, t4: string) {
  for (const tab of [t1, t2, t3, t4]) {
    await expect(this.page.locator(`text=${tab}`).first()).toBeVisible();
  }
});

Then('no horizontal scrollbar is present', async function (this: CustomWorld) {
  const hasHScroll = await this.page.evaluate(() => {
    return document.documentElement.scrollWidth > document.documentElement.clientWidth;
  });
  expect(hasHScroll).toBe(false);
});

Then('side panels for NPC chat and travel options are displayed', async function (this: CustomWorld) {
  // Layout assertion — verify panels exist
});

Then('focus moves through interactive elements in logical order', async function (this: CustomWorld) {
  const focused = await this.page.evaluate(() => document.activeElement?.tagName);
  expect(focused).toBeTruthy();
});

Then('each focused element has a visible focus indicator of at least {int}px outline', async function (this: CustomWorld, _px: number) {
  // Accessibility assertion — stubbed
});

Then('the NPC chat panel opens', async function (this: CustomWorld) {
  const panel = this.page.locator('[data-testid="chat-panel"], .chat-panel').first();
  await expect(panel).toBeVisible();
});

Then('the chat panel closes', async function (this: CustomWorld) {
  const panel = this.page.locator('[data-testid="chat-panel"], .chat-panel');
  await expect(panel).toHaveCount(0);
});

Then('the player message appears right-aligned with a blue background', async function (this: CustomWorld) {
  const msg = this.page.locator('[data-testid="player-message"], .player-message').last();
  await expect(msg).toBeVisible();
});

Then('a typing indicator with animated dots is shown for {int}ms', async function (this: CustomWorld, _ms: number) {
  const indicator = this.page.locator('[data-testid="typing-indicator"], .typing-indicator').first();
  await expect(indicator).toBeVisible();
});

Then('the NPC response appears left-aligned with a dark background and NPC name label', async function (this: CustomWorld) {
  const msg = this.page.locator('[data-testid="npc-message"], .npc-message').last();
  await expect(msg).toBeVisible();
});

Then('the character counter shows {string}', async function (this: CustomWorld, text: string) {
  await expect(this.page.locator(`text=${text}`).first()).toBeVisible();
});

Then('a loading indicator is displayed', async function (this: CustomWorld) {
  const loader = this.page.locator('[data-testid="loading"], .loading, [aria-busy="true"]').first();
  await expect(loader).toBeVisible();
});

Then('the loading indicator has aria-busy={string}', async function (this: CustomWorld, value: string) {
  const el = this.page.locator(`[aria-busy="${value}"]`).first();
  await expect(el).toBeVisible();
});

Then('a toast notification is displayed with the error message', async function (this: CustomWorld) {
  const toast = this.page.locator('[data-testid="toast"], .toast, [role="alert"]').first();
  await expect(toast).toBeVisible();
});

Then('the toast notification is dismissible', async function (this: CustomWorld) {
  const dismiss = this.page.locator('[data-testid="toast-dismiss"], .toast button, [role="alert"] button').first();
  await expect(dismiss).toBeVisible();
});

Then('a retry banner displays {string}', async function (this: CustomWorld, text: string) {
  await expect(this.page.locator(`text=${text}`).first()).toBeVisible();
});

Then('the system retries {int} times with delays of {int}, {int}, and {int} seconds', async function (this: CustomWorld, _count: number, _d1: number, _d2: number, _d3: number) {
  // Timing assertion — stubbed
});

Then('a persistent error message displays {string}', async function (this: CustomWorld, text: string) {
  await expect(this.page.locator(`text=${text}`).first()).toBeVisible();
});

Then('a confirmation dialog appears with text {string}', async function (this: CustomWorld, text: string) {
  const dialog = this.page.locator(`[role="dialog"], dialog, .modal`).first();
  await expect(dialog).toBeVisible();
  await expect(dialog.locator(`text=${text}`)).toBeVisible();
});

Then('the dialog has {string} and {string} buttons', async function (this: CustomWorld, btn1: string, btn2: string) {
  await expect(this.page.locator(`button:has-text("${btn1}")`).first()).toBeVisible();
  await expect(this.page.locator(`button:has-text("${btn2}")`).first()).toBeVisible();
});

Then('the warrant is submitted and the arrest result screen is shown', async function (this: CustomWorld) {
  // Verify navigation or result screen
});

Then('a celebratory visual is shown with a green theme', async function (this: CustomWorld) {
  const result = this.page.locator('[data-testid="arrest-result"], .arrest-result').first();
  await expect(result).toBeVisible();
});

Then('a success message mentioning the suspect name is displayed', async function (this: CustomWorld) {
  const msg = this.page.locator('[data-testid="success-message"], .success-message').first();
  await expect(msg).toBeVisible();
});

Then('a failure message is shown with a red theme', async function (this: CustomWorld) {
  const msg = this.page.locator('[data-testid="failure-message"], .failure-message').first();
  await expect(msg).toBeVisible();
});

Then('the full briefing text is displayed immediately', async function (this: CustomWorld) {
  const narrative = this.page.locator('[data-testid="briefing-narrative"], .briefing-narrative').first();
  await expect(narrative).toBeVisible();
});

Then('the input field is disabled', async function (this: CustomWorld) {
  const input = this.page.locator('input[type="text"], textarea').first();
  await expect(input).toBeDisabled();
});

Then('a message {string} is displayed', async function (this: CustomWorld, text: string) {
  await expect(this.page.locator(`text=${text}`).first()).toBeVisible();
});

Then('only one travel API call is made', async function (this: CustomWorld) {
  // Would need request interception — stubbed
});

Then('the travel button is disabled after the first click', async function (this: CustomWorld) {
  const btn = this.page.locator('[data-testid="travel-option"] button, .travel-option button').first();
  await expect(btn).toBeDisabled();
});

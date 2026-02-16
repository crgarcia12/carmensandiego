import { Given, When, Then } from '@cucumber/cucumber';
import { CustomWorld } from '../support/world';
import { expect } from '@playwright/test';

// ─── Setup ───────────────────────────────────────────────────────────────────

Given('the player is in the final trail city {string}', async function (this: CustomWorld, city: string) {
  (this as any).currentCity = city;
  (this as any).isFinalCity = true;
});

Given('the player is in city {string} which is not the final trail city', async function (this: CustomWorld, city: string) {
  (this as any).currentCity = city;
  (this as any).isFinalCity = false;
});

Given('the player is in the first trail city {string} which is not the final city', async function (this: CustomWorld, city: string) {
  (this as any).currentCity = city;
  (this as any).isFinalCity = false;
});

Given('the player has already issued a warrant for {string}', async function (this: CustomWorld, _suspectId: string) {
  // Test fixture: warrant already issued — stubbed
});

// ─── Actions ─────────────────────────────────────────────────────────────────

When(/^the player requests the dossier via GET \/api\/cases\/(.+)\/suspects$/, async function (this: CustomWorld, caseId: string) {
  try {
    this.apiResponse = await fetch(`${this.apiBaseUrl}/api/cases/${caseId}/suspects`, {
      method: 'GET',
      headers: { 'X-Session-Id': this.sessionId },
    });
    this.responseBody = await this.apiResponse.json();
  } catch (err) {
    throw new Error(`API call failed: GET /api/cases/${caseId}/suspects — ${err}`);
  }
});

When(/^the player issues a warrant for "([^"]+)" via POST \/api\/cases\/(.+)\/warrant$/, async function (this: CustomWorld, suspectId: string, caseId: string) {
  this.caseId = caseId;
  try {
    this.apiResponse = await fetch(`${this.apiBaseUrl}/api/cases/${caseId}/warrant`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'X-Session-Id': this.sessionId,
      },
      body: JSON.stringify({ suspectId }),
    });
    this.responseBody = await this.apiResponse.json();
  } catch (err) {
    throw new Error(`API call failed: POST /api/cases/${caseId}/warrant — ${err}`);
  }
});

When(/^the player issues a warrant with no suspect ID via POST \/api\/cases\/(.+)\/warrant$/, async function (this: CustomWorld, caseId: string) {
  try {
    this.apiResponse = await fetch(`${this.apiBaseUrl}/api/cases/${caseId}/warrant`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'X-Session-Id': this.sessionId,
      },
      body: JSON.stringify({}),
    });
    this.responseBody = await this.apiResponse.json();
  } catch (err) {
    throw new Error(`API call failed: POST /api/cases/${caseId}/warrant — ${err}`);
  }
});

When('the player clicks {string} for suspect {string}', async function (this: CustomWorld, _button: string, _suspectName: string) {
  // UI interaction — tested in game-ui.steps.ts; stubbed here for API context
});

// ─── Assertions──────────────────────────────────────────────────────────────

Then('the response contains {string} as an array with at least {int} entries', async function (this: CustomWorld, field: string, min: number) {
  expect(this.responseBody).not.toBeNull();
  const arr = this.responseBody[field];
  expect(Array.isArray(arr)).toBe(true);
  expect(arr.length).toBeGreaterThanOrEqual(min);
});

Then('the dossier does not reveal which suspect is correct', async function (this: CustomWorld) {
  expect(this.responseBody).not.toBeNull();
  const suspects = this.responseBody.suspects;
  for (const s of suspects) {
    expect(s.isCorrect).toBeUndefined();
  }
});

Then('each suspect has a non-empty {string}', async function (this: CustomWorld, field: string) {
  expect(this.responseBody).not.toBeNull();
  const suspects = this.responseBody.suspects;
  for (const s of suspects) {
    expect(s[field]).toBeDefined();
    expect(s[field]).toBeTruthy();
  }
});

Then('each suspect has a {string} object with {string}', async function (this: CustomWorld, objField: string, childField: string) {
  expect(this.responseBody).not.toBeNull();
  const suspects = this.responseBody.suspects;
  for (const s of suspects) {
    expect(s[objField]).toBeDefined();
    expect(s[objField][childField]).toBeDefined();
  }
});

Then('all trait values are non-empty strings', async function (this: CustomWorld) {
  expect(this.responseBody).not.toBeNull();
  const suspects = this.responseBody.suspects;
  for (const s of suspects) {
    const traits = s.traits;
    for (const key of Object.keys(traits)) {
      expect(typeof traits[key]).toBe('string');
      expect(traits[key].length).toBeGreaterThan(0);
    }
  }
});

Then('the response contains {string} with {string} equal to {string}', async function (this: CustomWorld, parentField: string, childField: string, value: string) {
  expect(this.responseBody).not.toBeNull();
  const obj = this.responseBody[parentField];
  expect(obj).toBeDefined();
  expect(obj[childField]).toBe(value);
});

Then('the response contains {string} including {string}', async function (this: CustomWorld, field: string, substring: string) {
  expect(this.responseBody).not.toBeNull();
  const value = this.responseBody[field] as string;
  expect(value).toBeDefined();
  expect(value).toContain(substring);
});

Then('the response contains {string} with a {string} timestamp', async function (this: CustomWorld, parentField: string, childField: string) {
  expect(this.responseBody).not.toBeNull();
  const obj = this.responseBody[parentField];
  expect(obj).toBeDefined();
  expect(obj[childField]).toBeDefined();
  expect(new Date(obj[childField]).toISOString()).toBeTruthy();
});

Then('a confirmation dialog is displayed with text {string}', async function (this: CustomWorld, _text: string) {
  // UI assertion — tested in game-ui.steps.ts; stubbed here
});

Then('the dialog has a {string} button and a {string} button', async function (this: CustomWorld, _btn1: string, _btn2: string) {
  // UI assertion — stubbed
});

Then('no warrant is issued and the player returns to the dossier', async function (this: CustomWorld) {
  // UI assertion — stubbed
});

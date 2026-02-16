import { Given, When, Then } from '@cucumber/cucumber';
import { CustomWorld } from '../support/world';
import { expect } from '@playwright/test';

// ─── Background / Setup ─────────────────────────────────────────────────────

Given('the player is currently in {string}',async function (this: CustomWorld, city: string) {
  (this as any).currentCity = city;
});

Given('the travel options include {string}, {string}, and {string}', async function (this: CustomWorld, city1: string, city2: string, city3: string) {
  (this as any).travelOptions = [city1, city2, city3];
});

Given('the travel options are {string}, {string}, and {string}', async function (this: CustomWorld, city1: string, city2: string, city3: string) {
  (this as any).travelOptions = [city1, city2, city3];
});

Given('the case {string} has {int} remaining steps', async function (this: CustomWorld, caseId: string, _steps: number) {
  this.caseId = caseId;
  // Test fixture — set remaining steps
});

Given('the case {string} has {int} remaining step', async function (this: CustomWorld, caseId: string, _steps: number) {
  this.caseId = caseId;
});

Given('no warrant has been issued', async function (this: CustomWorld) {
  // Precondition — default state
});

Given('the player is at the final city {string} in the trail', async function (this: CustomWorld, city: string) {
  (this as any).currentCity = city;
  (this as any).isFinalCity = true;
});

Given('the case {string} has status {string}', async function (this: CustomWorld, caseId: string, _status: string) {
  this.caseId = caseId;
});

Given('the player travels to decoy city {string}', async function (this: CustomWorld, city: string) {
  try {
    this.apiResponse = await fetch(`${this.apiBaseUrl}/api/cases/${this.caseId}/travel`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'X-Session-Id': this.sessionId,
      },
      body: JSON.stringify({ cityId: city }),
    });
    this.responseBody = await this.apiResponse.json();
  } catch (err) {
    throw new Error(`API call failed — ${err}`);
  }
});

// ─── Actions ─────────────────────────────────────────────────────────────────

When(/^the player requests the current city via GET \/api\/cases\/(.+)\/city$/, async function (this: CustomWorld, caseId: string) {
  try {
    this.apiResponse = await fetch(`${this.apiBaseUrl}/api/cases/${caseId}/city`, {
      method: 'GET',
      headers: { 'X-Session-Id': this.sessionId },
    });
    this.responseBody = await this.apiResponse.json();
  } catch (err) {
    throw new Error(`API call failed: GET /api/cases/${caseId}/city — ${err}`);
  }
});

When(/^the player travels to "([^"]+)" via POST \/api\/cases\/(.+)\/travel$/, async function (this: CustomWorld, cityId: string, caseId: string) {
  try {
    this.apiResponse = await fetch(`${this.apiBaseUrl}/api/cases/${caseId}/travel`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'X-Session-Id': this.sessionId,
      },
      body: JSON.stringify({ cityId }),
    });
    this.responseBody = await this.apiResponse.json();
  } catch (err) {
    throw new Error(`API call failed: POST /api/cases/${caseId}/travel — ${err}`);
  }
});

When(/^the player requests the current city twice via GET \/api\/cases\/(.+)\/city$/, async function (this: CustomWorld, caseId: string) {
  try {
    const res1 = await fetch(`${this.apiBaseUrl}/api/cases/${caseId}/city`, {
      method: 'GET',
      headers: { 'X-Session-Id': this.sessionId },
    });
    const body1 = await res1.json();

    const res2 = await fetch(`${this.apiBaseUrl}/api/cases/${caseId}/city`, {
      method: 'GET',
      headers: { 'X-Session-Id': this.sessionId },
    });
    const body2 = await res2.json();

    (this as any).responsePair = [body1, body2];
    this.apiResponse = res2;
    this.responseBody = body2;
  } catch (err) {
    throw new Error(`API call failed: GET /api/cases/${caseId}/city (x2) — ${err}`);
  }
});

// ─── Assertions ──────────────────────────────────────────────────────────────

Then('the response contains a {string} object with {string} equal to {string}', async function (this: CustomWorld, objField: string, childField: string, value: string) {
  expect(this.responseBody).not.toBeNull();
  const obj = this.responseBody[objField];
  expect(obj).toBeDefined();
  expect(obj[childField]).toBe(value);
});

Then('the response contains a {string} object with {string} and {string}', async function (this: CustomWorld, objField: string, field1: string, field2: string) {
  expect(this.responseBody).not.toBeNull();
  const obj = this.responseBody[objField];
  expect(obj).toBeDefined();
  expect(obj[field1]).toBeDefined();
  expect(obj[field2]).toBeDefined();
});

Then('the response contains a {string} object with {string}', async function (this: CustomWorld, objField: string, childField: string) {
  expect(this.responseBody).not.toBeNull();
  const obj = this.responseBody[objField];
  expect(obj).toBeDefined();
  expect(obj[childField]).toBeDefined();
});

Then('the response contains {string} as an array with {int} to {int} entries', async function (this: CustomWorld, field: string, min: number, max: number) {
  expect(this.responseBody).not.toBeNull();
  const arr = this.responseBody[field];
  expect(Array.isArray(arr)).toBe(true);
  expect(arr.length).toBeGreaterThanOrEqual(min);
  expect(arr.length).toBeLessThanOrEqual(max);
});

Then('the response contains {string} as an array with exactly {int} entries', async function (this: CustomWorld, field: string, count: number) {
  expect(this.responseBody).not.toBeNull();
  const arr = this.responseBody[field];
  expect(Array.isArray(arr)).toBe(true);
  expect(arr.length).toBe(count);
});

Then('each NPC has {string}, {string}, and {string}', async function (this: CustomWorld, f1: string, f2: string, f3: string) {
  expect(this.responseBody).not.toBeNull();
  const npcs = this.responseBody.npcs;
  expect(Array.isArray(npcs)).toBe(true);
  for (const npc of npcs) {
    expect(npc[f1]).toBeDefined();
    expect(npc[f2]).toBeDefined();
    expect(npc[f3]).toBeDefined();
  }
});

Then('the response contains {string} equal to false', async function (this: CustomWorld, field: string) {
  expect(this.responseBody).not.toBeNull();
  expect(this.responseBody[field]).toBe(false);
});

Then('the response contains {string} equal to true', async function (this: CustomWorld, field: string) {
  expect(this.responseBody).not.toBeNull();
  expect(this.responseBody[field]).toBe(true);
});

Then('each travel option has {string}, {string}, and {string}', async function (this: CustomWorld, f1: string, f2: string, f3: string) {
  expect(this.responseBody).not.toBeNull();
  const options = this.responseBody.travelOptions;
  expect(Array.isArray(options)).toBe(true);
  for (const opt of options) {
    expect(opt[f1]).toBeDefined();
    expect(opt[f2]).toBeDefined();
    expect(opt[f3]).toBeDefined();
  }
});

Then('exactly {int} travel option is the correct next trail city', async function (this: CustomWorld, count: number) {
  expect(this.responseBody).not.toBeNull();
  const options = this.responseBody.travelOptions;
  const correct = options.filter((o: any) => o.isCorrect === true);
  expect(correct.length).toBe(count);
});

Then('exactly {int} travel options are decoy cities', async function (this: CustomWorld, count: number) {
  expect(this.responseBody).not.toBeNull();
  const options = this.responseBody.travelOptions;
  const decoys = options.filter((o: any) => o.isCorrect !== true);
  expect(decoys.length).toBe(count);
});

Then('the response contains {string} equal to {string}', async function (this: CustomWorld, field: string, value: string) {
  expect(this.responseBody).not.toBeNull();
  expect(String(this.responseBody[field])).toBe(value);
});

Then('{string} is appended to the visited cities list', async function (this: CustomWorld, city: string) {
  expect(this.responseBody).not.toBeNull();
  const visited = this.responseBody.visitedCities;
  expect(Array.isArray(visited)).toBe(true);
  expect(visited).toContain(city);
});

Then('the response contains {string} as an empty array', async function (this: CustomWorld, field: string) {
  expect(this.responseBody).not.toBeNull();
  expect(this.responseBody[field]).toEqual([]);
});

Then('{int} of the travel options is the correct trail city the player missed', async function (this: CustomWorld, count: number) {
  expect(this.responseBody).not.toBeNull();
  const options = this.responseBody.travelOptions;
  const correct = options.filter((o: any) => o.isCorrect === true);
  expect(correct.length).toBe(count);
});

Then('both responses contain the same {int} city IDs in {string}', async function (this: CustomWorld, count: number, field: string) {
  const pair = (this as any).responsePair;
  expect(pair).toBeDefined();
  const ids1 = pair[0][field].map((o: any) => o.cityId).sort();
  const ids2 = pair[1][field].map((o: any) => o.cityId).sort();
  expect(ids1.length).toBe(count);
  expect(ids1).toEqual(ids2);
});

Then('the order of travel options may differ between requests', async function (this: CustomWorld) {
  // Non-deterministic assertion — just verify both responses exist
  const pair = (this as any).responsePair;
  expect(pair).toBeDefined();
  expect(pair.length).toBe(2);
});

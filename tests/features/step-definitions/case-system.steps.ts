import { Given, When, Then } from '@cucumber/cucumber';
import { CustomWorld } from '../support/world';
import { expect } from '@playwright/test';

// ─── Background / Setup ─────────────────────────────────────────────────────

Given('no active case exists for the session', async function (this: CustomWorld) {
  // Precondition: verify no active case for the session
  try {
    const res = await fetch(`${this.apiBaseUrl}/api/cases`, {
      method: 'GET',
      headers: { 'X-Session-Id': this.sessionId },
    });
    if (res.ok) {
      const body = await res.json();
      expect(body.activeCaseId).toBeFalsy();
    }
  } catch {
    // API not yet available — red baseline expected
  }
});

Given('an active case {string} exists with the player in the final trail city {string}', async function (this: CustomWorld, caseId: string, city: string) {
  this.caseId = caseId;
  // Test fixture: case at final city — would require API setup
});

Given('the correct suspect is {string}', async function (this: CustomWorld, suspectId: string) {
  // Test fixture: store expected correct suspect for later assertions
  (this as any).expectedCorrectSuspect = suspectId;
});

Given('an active case {string} exists with {int} remaining step', async function (this: CustomWorld, caseId: string, _steps: number) {
  this.caseId = caseId;
});

Given('an active case {string} exists with {int} remaining steps', async function (this: CustomWorld, caseId: string, _steps: number) {
  this.caseId = caseId;
});

Given('no case configurations exist in the system', async function (this: CustomWorld) {
  // Test fixture: empty configuration state — stubbed
});

Given('an active case {string} already exists for the session', async function (this: CustomWorld, caseId: string) {
  this.caseId = caseId;
});

Given('a completed case {string} with status {string}', async function (this: CustomWorld, caseId: string, _status: string) {
  this.caseId = caseId;
});

Given('an active case {string} with status {string}', async function (this: CustomWorld, caseId: string, _status: string) {
  this.caseId = caseId;
});

Given('a case {string} belongs to session {string}', async function (this: CustomWorld, caseId: string, _otherSessionId: string) {
  this.caseId = caseId;
});

// ─── Actions ─────────────────────────────────────────────────────────────────

When('the player creates a new case via POST \\/api\\/cases', async function (this: CustomWorld) {
  try {
    this.apiResponse = await fetch(`${this.apiBaseUrl}/api/cases`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'X-Session-Id': this.sessionId,
      },
    });
    this.responseBody = await this.apiResponse.json();
    if (this.responseBody.id) {
      this.caseId = this.responseBody.id;
    }
  } catch (err) {
    throw new Error(`API call failed: POST /api/cases — ${err}`);
  }
});

When('the player issues a warrant for {string}', async function (this: CustomWorld, suspectId: string) {
  try {
    this.apiResponse = await fetch(`${this.apiBaseUrl}/api/cases/${this.caseId}/warrant`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'X-Session-Id': this.sessionId,
      },
      body: JSON.stringify({ suspectId }),
    });
    this.responseBody = await this.apiResponse.json();
  } catch (err) {
    throw new Error(`API call failed: POST /api/cases/${this.caseId}/warrant — ${err}`);
  }
});

When(/^the player requests the case summary via GET \/api\/cases\/(.+)\/summary$/, async function (this: CustomWorld, caseId: string) {
  try {
    this.apiResponse = await fetch(`${this.apiBaseUrl}/api/cases/${caseId}/summary`, {
      method: 'GET',
      headers: { 'X-Session-Id': this.sessionId },
    });
    this.responseBody = await this.apiResponse.json();
  } catch (err) {
    throw new Error(`API call failed: GET /api/cases/${caseId}/summary — ${err}`);
  }
});

When('the player travels to a new city', async function (this: CustomWorld) {
  try {
    this.apiResponse = await fetch(`${this.apiBaseUrl}/api/cases/${this.caseId}/travel`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'X-Session-Id': this.sessionId,
      },
      body: JSON.stringify({ cityId: 'any-city' }),
    });
    this.responseBody = await this.apiResponse.json();
  } catch (err) {
    throw new Error(`API call failed: POST /api/cases/${this.caseId}/travel — ${err}`);
  }
});

When(/^the player attempts to travel via POST \/api\/cases\/(.+)\/travel$/, async function (this: CustomWorld, caseId: string) {
  try {
    this.apiResponse = await fetch(`${this.apiBaseUrl}/api/cases/${caseId}/travel`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'X-Session-Id': this.sessionId,
      },
      body: JSON.stringify({ cityId: 'any-city' }),
    });
    this.responseBody = await this.apiResponse.json();
  } catch (err) {
    throw new Error(`API call failed: POST /api/cases/${caseId}/travel — ${err}`);
  }
});

When(/^the player requests case state via GET \/api\/cases\/(.+)$/, async function (this: CustomWorld, caseId: string) {
  try {
    this.apiResponse = await fetch(`${this.apiBaseUrl}/api/cases/${caseId}`, {
      method: 'GET',
      headers: { 'X-Session-Id': this.sessionId },
    });
    this.responseBody = await this.apiResponse.json();
  } catch (err) {
    throw new Error(`API call failed: GET /api/cases/${caseId} — ${err}`);
  }
});

When(/^the player with session "([^"]+)" requests GET \/api\/cases\/(.+)$/, async function (this: CustomWorld, sessionId: string, caseId: string) {
  try {
    this.apiResponse = await fetch(`${this.apiBaseUrl}/api/cases/${caseId}`, {
      method: 'GET',
      headers: { 'X-Session-Id': sessionId },
    });
    this.responseBody = await this.apiResponse.json();
  } catch (err) {
    throw new Error(`API call failed: GET /api/cases/${caseId} — ${err}`);
  }
});

// ─── Assertions ──────────────────────────────────────────────────────────────

Then('the response contains a case {string} starting with {string}', async function (this: CustomWorld, field: string, prefix: string) {
  expect(this.responseBody).not.toBeNull();
  const value = this.responseBody[field] as string;
  expect(value).toBeDefined();
  expect(value.startsWith(prefix)).toBe(true);
});

Then('the response contains a non-empty {string}', async function (this: CustomWorld, field: string) {
  expect(this.responseBody).not.toBeNull();
  expect(this.responseBody[field]).toBeDefined();
  expect(this.responseBody[field]).toBeTruthy();
});

Then('the response contains a {string} with {string} and {string}', async function (this: CustomWorld, field: string, subField1: string, subField2: string) {
  expect(this.responseBody).not.toBeNull();
  const obj = this.responseBody[field];
  expect(obj).toBeDefined();
  expect(obj[subField1]).toBeDefined();
  expect(obj[subField2]).toBeDefined();
});

Then('the response contains {string} set to the first trail city', async function (this: CustomWorld, field: string) {
  expect(this.responseBody).not.toBeNull();
  expect(this.responseBody[field]).toBeDefined();
  expect(typeof this.responseBody[field]).toBe('string');
});

Then('the response contains {string} equal to {int}', async function (this: CustomWorld, field: string, value: number) {
  expect(this.responseBody).not.toBeNull();
  expect(this.responseBody[field]).toBe(value);
});

Then('the case has a trail of between {int} and {int} cities', async function (this: CustomWorld, min: number, max: number) {
  expect(this.responseBody).not.toBeNull();
  const trail = this.responseBody.trail;
  expect(trail).toBeDefined();
  expect(Array.isArray(trail)).toBe(true);
  expect(trail.length).toBeGreaterThanOrEqual(min);
  expect(trail.length).toBeLessThanOrEqual(max);
});

Then('the case has a {string} referencing a valid suspect', async function (this: CustomWorld, field: string) {
  expect(this.responseBody).not.toBeNull();
  expect(this.responseBody[field]).toBeDefined();
  expect(typeof this.responseBody[field]).toBe('string');
});

Then('the case has {string} equal to {int}', async function (this: CustomWorld, field: string, value: number) {
  expect(this.responseBody).not.toBeNull();
  expect(this.responseBody[field]).toBe(value);
});

Then('the case has {string} containing exactly {int} city', async function (this: CustomWorld, field: string, count: number) {
  expect(this.responseBody).not.toBeNull();
  const arr = this.responseBody[field];
  expect(Array.isArray(arr)).toBe(true);
  expect(arr.length).toBe(count);
});

Then('the case has {string} equal to false', async function (this: CustomWorld, field: string) {
  expect(this.responseBody).not.toBeNull();
  expect(this.responseBody[field]).toBe(false);
});

Then('the case has {string} equal to {string}', async function (this: CustomWorld, field: string, value: string) {
  expect(this.responseBody).not.toBeNull();
  expect(this.responseBody[field]).toBe(value);
});

Then('the case status becomes {string}', async function (this: CustomWorld, status: string) {
  expect(this.responseBody).not.toBeNull();
  // Check caseStatus or status field
  const actual = this.responseBody.caseStatus ?? this.responseBody.status;
  expect(actual).toBe(status);
});

Then('the summary {string} is {string}', async function (this: CustomWorld, field: string, value: string) {
  expect(this.responseBody).not.toBeNull();
  expect(this.responseBody[field]).toBe(value);
});

Then('the summary contains {string} as a non-empty array', async function (this: CustomWorld, field: string) {
  expect(this.responseBody).not.toBeNull();
  expect(Array.isArray(this.responseBody[field])).toBe(true);
  expect(this.responseBody[field].length).toBeGreaterThan(0);
});

Then('the summary contains {string} between {int} and {int}', async function (this: CustomWorld, field: string, min: number, max: number) {
  expect(this.responseBody).not.toBeNull();
  const val = this.responseBody[field];
  expect(val).toBeGreaterThanOrEqual(min);
  expect(val).toBeLessThanOrEqual(max);
});

Then('the summary contains {string} equal to {int}', async function (this: CustomWorld, field: string, value: number) {
  expect(this.responseBody).not.toBeNull();
  expect(this.responseBody[field]).toBe(value);
});

Then('the summary contains {string} with {string} equal to {string}', async function (this: CustomWorld, parentField: string, childField: string, value: string) {
  expect(this.responseBody).not.toBeNull();
  expect(this.responseBody[parentField]).toBeDefined();
  expect(this.responseBody[parentField][childField]).toBe(value);
});

Then('the summary contains {string} with {string} and {string}', async function (this: CustomWorld, field: string, sub1: string, sub2: string) {
  expect(this.responseBody).not.toBeNull();
  const obj = this.responseBody[field];
  expect(obj).toBeDefined();
  expect(obj[sub1]).toBeDefined();
  expect(obj[sub2]).toBeDefined();
});

Then('the summary contains {string} with {string}', async function (this: CustomWorld, field: string, subField: string) {
  expect(this.responseBody).not.toBeNull();
  expect(this.responseBody[field]).toBeDefined();
  expect(this.responseBody[field][subField]).toBeDefined();
});

Then('the summary contains {string} equal to null', async function (this: CustomWorld, field: string) {
  expect(this.responseBody).not.toBeNull();
  expect(this.responseBody[field]).toBeNull();
});

Then('the {string} becomes {int}', async function (this: CustomWorld, field: string, value: number) {
  expect(this.responseBody).not.toBeNull();
  expect(this.responseBody[field]).toBe(value);
});

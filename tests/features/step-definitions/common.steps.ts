import { Given, Then } from '@cucumber/cucumber';
import { CustomWorld } from '../support/world';
import { expect } from '@playwright/test';

// ─── Shared: Session setup ───────────────────────────────────────────────────

Given('a valid session {string} exists', async function (this: CustomWorld, sessionId: string) {
  this.sessionId = sessionId;
  try {
    const res = await fetch(`${this.apiBaseUrl}/api/sessions`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ id: sessionId }),
    });
    if (res.ok) {
      const body = await res.json();
      this.sessionId = body.id ?? sessionId;
    }
    // If session creation fails, we still set sessionId for test to proceed (red baseline)
  } catch {
    // API not available yet — expected during red baseline
  }
});

// ─── Shared: Response status assertion ───────────────────────────────────────

Then('the response status is {int}', async function (this: CustomWorld, status: number) {
  expect(this.apiResponse).not.toBeNull();
  expect(this.apiResponse!.status).toBe(status);
});

// ─── Shared: Response body field/value assertions ────────────────────────────

Then('the response body contains {string} with value {string}', async function (this: CustomWorld, field: string, value: string) {
  expect(this.responseBody).not.toBeNull();
  expect(this.responseBody[field]).toBe(value);
});

Then('the response body contains {string} with message {string}', async function (this: CustomWorld, field: string, message: string) {
  expect(this.responseBody).not.toBeNull();
  expect(this.responseBody[field]).toBe(message);
});

Then('the response body contains {string} with {string} equal to {string}', async function (this: CustomWorld, parentField: string, childField: string, value: string) {
  expect(this.responseBody).not.toBeNull();
  expect(this.responseBody[parentField]).toBeDefined();
  expect(this.responseBody[parentField][childField]).toBe(value);
});

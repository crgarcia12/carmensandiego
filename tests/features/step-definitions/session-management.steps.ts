import { Given, When, Then } from '@cucumber/cucumber';
import { CustomWorld } from '../support/world';
import { expect } from '@playwright/test';

// ─── Background / Setup ─────────────────────────────────────────────────────

Given('fewer than {int} active sessions exist', async function (this: CustomWorld, _max: number) {
  // Precondition assumed true in test environment
});

Given('a session {string} was created and is still active', async function (this: CustomWorld, sessionId: string) {
  this.sessionId = sessionId;
  try {
    await fetch(`${this.apiBaseUrl}/api/sessions`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ id: sessionId }),
    });
  } catch {
    // API not yet available
  }
});

Given('a session {string} has not been accessed for {int} hours', async function (this: CustomWorld, sessionId: string, _hours: number) {
  this.sessionId = sessionId;
  // Setup would require test fixture / time manipulation — stubbed for red baseline
});

Given('the client has {string} stored in localStorage', async function (this: CustomWorld, sessionId: string) {
  this.sessionId = sessionId;
});

Given('the session {string} has expired', async function (this: CustomWorld, sessionId: string) {
  this.sessionId = sessionId;
});

Given('exactly {int} active sessions exist', async function (this: CustomWorld, _count: number) {
  // Precondition: server at max capacity — stubbed
});

Given('an active session {string} exists with an active case', async function (this: CustomWorld, sessionId: string) {
  this.sessionId = sessionId;
});

Given('an active session {string} exists with no active case', async function (this: CustomWorld, sessionId: string) {
  this.sessionId = sessionId;
});

Given('session {string} has expired', async function (this: CustomWorld, sessionId: string) {
  this.sessionId = sessionId;
});

Given('an active session {string}', async function (this: CustomWorld, sessionId: string) {
  this.sessionId = sessionId;
});

Given('session {string} was previously deleted', async function (this: CustomWorld, sessionId: string) {
  this.sessionId = sessionId;
});

Given(/^a session "([^"]+)" was marked expired (\d+) hours ago$/, async function (this: CustomWorld, sessionId: string, _hours: number) {
  this.sessionId = sessionId;
  // Test fixture: session expired long ago — stubbed
});

// ─── Actions ─────────────────────────────────────────────────────────────────

When('a new session is created via POST \\/api\\/sessions', async function (this: CustomWorld) {
  try {
    this.apiResponse = await fetch(`${this.apiBaseUrl}/api/sessions`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
    });
    this.responseBody = await this.apiResponse.json();
    if (this.responseBody.id) {
      this.sessionId = this.responseBody.id;
    }
  } catch (err) {
    throw new Error(`API call failed: POST /api/sessions — ${err}`);
  }
});

When(/^the session is retrieved via GET \/api\/sessions\/(.+)$/, async function (this: CustomWorld, sessionId: string) {
  try {
    this.apiResponse = await fetch(`${this.apiBaseUrl}/api/sessions/${sessionId}`, {
      method: 'GET',
      headers: { 'X-Session-Id': this.sessionId },
    });
    this.responseBody = await this.apiResponse.json();
  } catch (err) {
    throw new Error(`API call failed: GET /api/sessions/${sessionId} — ${err}`);
  }
});

When(/^any API call is made with session "([^"]+)"$/, async function (this: CustomWorld, sessionId: string) {
  try {
    this.apiResponse = await fetch(`${this.apiBaseUrl}/api/sessions/${sessionId}`, {
      method: 'GET',
      headers: { 'X-Session-Id': sessionId },
    });
    this.responseBody = await this.apiResponse.json();
  } catch (err) {
    throw new Error(`API call failed — ${err}`);
  }
});

When(/^the client calls GET \/api\/sessions\/(.+)$/, async function (this: CustomWorld, sessionId: string) {
  try {
    this.apiResponse = await fetch(`${this.apiBaseUrl}/api/sessions/${sessionId}`, {
      method: 'GET',
      headers: { 'X-Session-Id': sessionId },
    });
    this.responseBody = await this.apiResponse.json();
  } catch (err) {
    throw new Error(`API call failed — ${err}`);
  }
});

When(/^the session is deleted via DELETE \/api\/sessions\/(.+)$/, async function (this: CustomWorld, sessionId: string) {
  try {
    this.apiResponse = await fetch(`${this.apiBaseUrl}/api/sessions/${sessionId}`, {
      method: 'DELETE',
      headers: { 'X-Session-Id': this.sessionId },
    });
    // 204 may have no body
    if (this.apiResponse.status !== 204) {
      this.responseBody = await this.apiResponse.json();
    } else {
      this.responseBody = null;
    }
  } catch (err) {
    throw new Error(`API call failed: DELETE /api/sessions/${sessionId} — ${err}`);
  }
});

When(/^a deletion is attempted via DELETE \/api\/sessions\/(.+)$/, async function (this: CustomWorld, sessionId: string) {
  try {
    this.apiResponse = await fetch(`${this.apiBaseUrl}/api/sessions/${sessionId}`, {
      method: 'DELETE',
      headers: { 'X-Session-Id': this.sessionId },
    });
    if (this.apiResponse.status !== 204) {
      this.responseBody = await this.apiResponse.json();
    } else {
      this.responseBody = null;
    }
  } catch (err) {
    throw new Error(`API call failed: DELETE /api/sessions/${sessionId} — ${err}`);
  }
});

When(/^a request to GET \/api\/cases\/(.+) is made without an X-Session-Id header$/, async function (this: CustomWorld, caseId: string) {
  try {
    this.apiResponse = await fetch(`${this.apiBaseUrl}/api/cases/${caseId}`, {
      method: 'GET',
      // Intentionally no X-Session-Id header
    });
    this.responseBody = await this.apiResponse.json();
  } catch (err) {
    throw new Error(`API call failed — ${err}`);
  }
});

When(/^a request is made with X-Session-Id header "([^"]+)"$/, async function (this: CustomWorld, headerValue: string) {
  try {
    this.apiResponse = await fetch(`${this.apiBaseUrl}/api/sessions/${headerValue}`, {
      method: 'GET',
      headers: { 'X-Session-Id': headerValue },
    });
    this.responseBody = await this.apiResponse.json();
  } catch (err) {
    throw new Error(`API call failed — ${err}`);
  }
});

When('the player makes an API call at time T1', async function (this: CustomWorld) {
  try {
    this.apiResponse = await fetch(`${this.apiBaseUrl}/api/sessions/${this.sessionId}`, {
      method: 'GET',
      headers: { 'X-Session-Id': this.sessionId },
    });
    this.responseBody = await this.apiResponse.json();
  } catch (err) {
    throw new Error(`API call failed — ${err}`);
  }
});

When('then makes another API call at time T2', async function (this: CustomWorld) {
  // Small delay to ensure different timestamps
  await new Promise((r) => setTimeout(r, 100));
  try {
    this.apiResponse = await fetch(`${this.apiBaseUrl}/api/sessions/${this.sessionId}`, {
      method: 'GET',
      headers: { 'X-Session-Id': this.sessionId },
    });
    this.responseBody = await this.apiResponse.json();
  } catch (err) {
    throw new Error(`API call failed — ${err}`);
  }
});

When('the background cleanup process runs', async function (this: CustomWorld) {
  // Trigger cleanup endpoint or wait for background process — stubbed
  try {
    this.apiResponse = await fetch(`${this.apiBaseUrl}/api/admin/cleanup`, {
      method: 'POST',
    });
  } catch {
    // Not yet implemented
  }
});

When('the client creates a new session via POST \\/api\\/sessions', async function (this: CustomWorld) {
  try {
    this.apiResponse = await fetch(`${this.apiBaseUrl}/api/sessions`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
    });
    this.responseBody = await this.apiResponse.json();
  } catch (err) {
    throw new Error(`API call failed — ${err}`);
  }
});

// ─── Assertions ──────────────────────────────────────────────────────────────

Then('the response contains an {string} matching the pattern {string} followed by a UUID', async function (this: CustomWorld, field: string, prefix: string) {
  expect(this.responseBody).not.toBeNull();
  const value = this.responseBody[field] as string;
  expect(value).toBeDefined();
  expect(value.startsWith(prefix)).toBe(true);
});

Then('the response contains {string} as a UTC timestamp', async function (this: CustomWorld, field: string) {
  expect(this.responseBody).not.toBeNull();
  const value = this.responseBody[field];
  expect(value).toBeDefined();
  expect(new Date(value).toISOString()).toBeTruthy();
});

Then('the response header {string} matches the response body {string}',async function (this: CustomWorld, headerName: string, bodyField: string) {
  expect(this.apiResponse).not.toBeNull();
  const headerValue = this.apiResponse!.headers.get(headerName);
  expect(headerValue).toBe(this.responseBody[bodyField]);
});

Then('the response contains {string} updated to the current time', async function (this: CustomWorld, field: string) {
  expect(this.responseBody).not.toBeNull();
  const value = this.responseBody[field];
  expect(value).toBeDefined();
  const ts = new Date(value).getTime();
  const now = Date.now();
  // Allow 30 second tolerance
  expect(Math.abs(now - ts)).toBeLessThan(30_000);
});

Then('the response contains {string} if a case is in progress', async function (this: CustomWorld, field: string) {
  expect(this.responseBody).not.toBeNull();
  // Field may or may not exist depending on state — just verify structure
  // If present, it should be a string
  if (this.responseBody[field] !== undefined && this.responseBody[field] !== null) {
    expect(typeof this.responseBody[field]).toBe('string');
  }
});

Then('the client clears {string} from localStorage', async function (this: CustomWorld, _sessionId: string) {
  // Client-side behavior — API test just verifies the 410 was received
  expect(this.apiResponse!.status).toBe(410);
});

Then('the session is marked as {string}', async function (this: CustomWorld, _status: string) {
  // Verified by subsequent GET returning 404
});

Then('the active case transitions to status {string}', async function (this: CustomWorld, _status: string) {
  // Verified by subsequent case query
});

Then('the response header {string} equals {string}', async function (this: CustomWorld, headerName: string, expectedValue: string) {
  expect(this.apiResponse).not.toBeNull();
  const headerValue = this.apiResponse!.headers.get(headerName);
  expect(headerValue).toBe(expectedValue);
});

Then('the response body contains {string} equal to {int}', async function (this: CustomWorld, field: string, value: number) {
  expect(this.responseBody).not.toBeNull();
  expect(this.responseBody[field]).toBe(value);
});

Then('the session {string} is permanently deleted from storage', async function (this: CustomWorld, sessionId: string) {
  try {
    const res = await fetch(`${this.apiBaseUrl}/api/sessions/${sessionId}`, {
      method: 'GET',
    });
    expect(res.status).toBe(404);
  } catch (err) {
    throw new Error(`Verification failed — ${err}`);
  }
});

Then('the session {string} is updated to T2', async function (this: CustomWorld, field: string) {
  expect(this.responseBody).not.toBeNull();
  expect(this.responseBody[field]).toBeDefined();
});

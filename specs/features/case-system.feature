@api
Feature: Case System
  As a player, I want to start and manage detective cases
  so that I can investigate stolen treasures and catch suspects.

  Background:
    Given a valid session "sess-test-001" exists
    And no active case exists for the session

  @smoke
  Scenario: Starting a new case returns briefing info
    When the player creates a new case via POST /api/cases
    Then the response status is 201
    And the response contains a case "id" starting with "case-"
    And the response contains a non-empty "title"
    And the response contains a non-empty "briefing"
    And the response contains a "stolenTreasure" with "name" and "description"
    And the response contains "currentCity" set to the first trail city
    And the response contains "remainingSteps" equal to 10
    And the response contains "status" equal to "active"

  @api
  Scenario: New case has correct structure
    When the player creates a new case via POST /api/cases
    Then the case has a trail of between 4 and 6 cities
    And the case has a "correctSuspectId" referencing a valid suspect
    And the case has "remainingSteps" equal to 10
    And the case has "currentCityIndex" equal to 0
    And the case has "visitedCities" containing exactly 1 city
    And the case has "warrantIssued" equal to false
    And the case has "status" equal to "active"

  @api
  Scenario: Case completion summary on win
    Given an active case "case-win-001" exists with the player in the final trail city "cairo"
    And the correct suspect is "suspect-carmen"
    When the player issues a warrant for "suspect-carmen"
    Then the case status becomes "won"
    When the player requests the case summary via GET /api/cases/case-win-001/summary
    Then the response status is 200
    And the summary "outcome" is "won"
    And the summary contains "citiesVisited" as a non-empty array
    And the summary contains "stepsUsed" between 1 and 10
    And the summary contains "totalSteps" equal to 10
    And the summary contains "correctSuspect" with "id" equal to "suspect-carmen"
    And the summary contains "playerWarrant" with "suspectId" equal to "suspect-carmen"
    And the summary contains "stolenTreasure" with "name" and "description"

  @api
  Scenario: Case completion summary on lose by step exhaustion
    Given an active case "case-lose-001" exists with 1 remaining step
    When the player travels to a new city
    Then the "remainingSteps" becomes 0
    And the case status becomes "lost"
    When the player requests the case summary via GET /api/cases/case-lose-001/summary
    Then the response status is 200
    And the summary "outcome" is "lost"
    And the summary contains "stepsUsed" equal to 10
    And the summary contains "correctSuspect" with "name"
    And the summary contains "playerWarrant" equal to null

  @api
  Scenario: Case data load failure when no configurations available
    Given no case configurations exist in the system
    When the player creates a new case via POST /api/cases
    Then the response status is 404
    And the response body contains "error" with message "No case configurations available"

  @api
  Scenario: Cannot create a case when session already has an active case
    Given an active case "case-existing-001" already exists for the session
    When the player creates a new case via POST /api/cases
    Then the response status is 409
    And the response body contains "error" with message "Active case already exists"

  @api
  Scenario: Actions rejected on completed case
    Given a completed case "case-done-001" with status "won"
    When the player attempts to travel via POST /api/cases/case-done-001/travel
    Then the response status is 409
    And the response body contains "error" with message "Case is already completed"

  @api
  Scenario: Summary not available for active case
    Given an active case "case-active-001" with status "active"
    When the player requests the case summary via GET /api/cases/case-active-001/summary
    Then the response status is 400
    And the response body contains "error" with message "Case is still active"

  @api
  Scenario: Case not found returns 404
    When the player requests case state via GET /api/cases/case-nonexistent
    Then the response status is 404
    And the response body contains "error" with message "Case not found"

  @api
  Scenario: Case belonging to another session returns 403
    Given a case "case-other-001" belongs to session "sess-other-001"
    When the player with session "sess-test-001" requests GET /api/cases/case-other-001
    Then the response status is 403

@api
Feature: Suspect Dossier & Warrants
  As a player, I want to view suspects and issue arrest warrants
  so that I can identify and apprehend the correct criminal.

  Background:
    Given a valid session "sess-test-001" exists
    And an active case "case-warrant-001" exists

  @smoke
  Scenario: Viewing the full suspect dossier returns 10 or more suspects
    When the player requests the dossier via GET /api/cases/case-warrant-001/suspects
    Then the response status is 200
    And the response contains "suspects" as an array with at least 10 entries
    And the dossier does not reveal which suspect is correct

  @api
  Scenario: Each suspect has all required traits
    When the player requests the dossier via GET /api/cases/case-warrant-001/suspects
    Then each suspect has a non-empty "id"
    And each suspect has a non-empty "name"
    And each suspect has a non-empty "photoKey"
    And each suspect has a "traits" object with "hairColor"
    And each suspect has a "traits" object with "eyeColor"
    And each suspect has a "traits" object with "hobby"
    And each suspect has a "traits" object with "favoriteFood"
    And each suspect has a "traits" object with "vehicle"
    And each suspect has a "traits" object with "distinguishingFeature"
    And all trait values are non-empty strings

  @smoke
  Scenario: Issuing a warrant for the correct suspect in the correct city wins the case
    Given the player is in the final trail city "cairo"
    And the correct suspect is "suspect-carmen"
    When the player issues a warrant for "suspect-carmen" via POST /api/cases/case-warrant-001/warrant
    Then the response status is 200
    And the response contains "result" equal to "won"
    And the response contains "caseStatus" equal to "won"
    And the response contains "message" including "Carmen Sandiego"
    And the response contains "warrant" with "suspectId" equal to "suspect-carmen"
    And the response contains "warrant" with "cityId" equal to "cairo"
    And the response contains "warrant" with a "issuedAt" timestamp

  @api
  Scenario: Issuing a warrant for the wrong suspect loses the case
    Given the player is in the final trail city "cairo"
    And the correct suspect is "suspect-carmen"
    When the player issues a warrant for "suspect-vic" via POST /api/cases/case-warrant-001/warrant
    Then the response status is 200
    And the response contains "result" equal to "lost"
    And the response contains "reason" equal to "wrong_suspect"
    And the response contains "caseStatus" equal to "lost"
    And the response contains "correctSuspect" with "id" equal to "suspect-carmen"
    And the response contains "correctSuspect" with "name" equal to "Carmen Sandiego"

  @api
  Scenario: Issuing a warrant for the right suspect in the wrong city loses the case
    Given the player is in city "tokyo" which is not the final trail city
    And the correct suspect is "suspect-carmen"
    When the player issues a warrant for "suspect-carmen" via POST /api/cases/case-warrant-001/warrant
    Then the response status is 200
    And the response contains "result" equal to "lost"
    And the response contains "reason" equal to "wrong_city"
    And the response contains "caseStatus" equal to "lost"
    And the response contains "warrant" with "cityId" equal to "tokyo"

  @api
  Scenario: Warrant confirmation dialog is required before submission
    When the player clicks "Issue Warrant" for suspect "Carmen Sandiego"
    Then a confirmation dialog is displayed with text "Are you sure you want to issue a warrant for Carmen Sandiego? This action is irreversible."
    And the dialog has a "Confirm" button and a "Cancel" button
    When the player clicks "Cancel"
    Then no warrant is issued and the player returns to the dossier

  @api
  Scenario: Cannot issue a warrant twice for the same case
    Given the player has already issued a warrant for "suspect-carmen"
    When the player issues a warrant for "suspect-vic" via POST /api/cases/case-warrant-001/warrant
    Then the response status is 409
    And the response body contains "error" with value "Warrant already issued for this case"
    And the response body contains "code" with value "WARRANT_ALREADY_ISSUED"

  @api
  Scenario: Cannot issue a warrant when case is already completed
    Given the case "case-warrant-001" has status "lost"
    When the player issues a warrant for "suspect-carmen" via POST /api/cases/case-warrant-001/warrant
    Then the response status is 409
    And the response body contains "error" with value "Case is already completed"
    And the response body contains "code" with value "CASE_COMPLETED"

  @api
  Scenario: Invalid suspect ID returns 400
    When the player issues a warrant for "suspect-nonexistent" via POST /api/cases/case-warrant-001/warrant
    Then the response status is 400
    And the response body contains "error" with value "Suspect not found"
    And the response body contains "code" with value "INVALID_SUSPECT"

  @api
  Scenario: Missing suspect ID in request body returns 400
    When the player issues a warrant with no suspect ID via POST /api/cases/case-warrant-001/warrant
    Then the response status is 400
    And the response body contains "error" with value "Suspect ID is required"
    And the response body contains "code" with value "MISSING_SUSPECT_ID"

  @api
  Scenario: Dossier is accessible even after case completion
    Given the case "case-warrant-001" has status "won"
    When the player requests the dossier via GET /api/cases/case-warrant-001/suspects
    Then the response status is 200
    And the response contains "suspects" as an array with at least 10 entries

  @api
  Scenario: Warrant can be issued from the first city
    Given the player is in the first trail city "bangkok" which is not the final city
    And the correct suspect is "suspect-carmen"
    When the player issues a warrant for "suspect-carmen" via POST /api/cases/case-warrant-001/warrant
    Then the response status is 200
    And the response contains "result" equal to "lost"
    And the response contains "reason" equal to "wrong_city"

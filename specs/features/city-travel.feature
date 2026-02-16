@api
Feature: City & Travel
  As a player, I want to view cities and travel between them
  so that I can follow the suspect's trail and gather clues.

  Background:
    Given a valid session "sess-test-001" exists
    And an active case "case-travel-001" exists with 8 remaining steps
    And the player is currently in "bangkok"

  @smoke
  Scenario: Viewing the current city shows background and NPCs
    When the player requests the current city via GET /api/cases/case-travel-001/city
    Then the response status is 200
    And the response contains a "city" object with "id" equal to "bangkok"
    And the response contains a "city" object with "name" equal to "Bangkok"
    And the response contains a "city" object with "region" and "continent"
    And the response contains a "city" object with "backgroundKey"
    And the response contains "npcs" as an array with 2 to 3 entries
    And each NPC has "id", "name", and "role"
    And the response contains "remainingSteps" equal to 8
    And the response contains "isFinalCity" equal to false

  @smoke
  Scenario: Travel options show 3 cities including 1 correct and 2 decoys
    When the player requests the current city via GET /api/cases/case-travel-001/city
    Then the response contains "travelOptions" as an array with exactly 3 entries
    And each travel option has "cityId", "cityName", and "description"
    And exactly 1 travel option is the correct next trail city
    And exactly 2 travel options are decoy cities

  @api
  Scenario: Traveling to a valid city decrements steps
    Given the travel options include "tokyo", "mumbai", and "sydney"
    When the player travels to "tokyo" via POST /api/cases/case-travel-001/travel
    Then the response status is 200
    And the response contains a "city" object with "id" equal to "tokyo"
    And the response contains "remainingSteps" equal to 7
    And the response contains "caseStatus" equal to "active"
    And "tokyo" is appended to the visited cities list

  @api
  Scenario: Cannot travel when remaining steps is 0
    Given the case "case-travel-001" has 0 remaining steps
    When the player travels to "tokyo" via POST /api/cases/case-travel-001/travel
    Then the response status is 409
    And the response body contains "error" with value "No remaining steps"
    And the response body contains "code" with value "NO_STEPS"

  @api
  Scenario: Cannot travel to a city not in the offered options
    Given the travel options are "tokyo", "mumbai", and "sydney"
    When the player travels to "london" via POST /api/cases/case-travel-001/travel
    Then the response status is 400
    And the response body contains "error" with value "Invalid travel destination"
    And the response body contains "code" with value "INVALID_DESTINATION"

  @api
  Scenario: Cannot travel to the current city
    When the player travels to "bangkok" via POST /api/cases/case-travel-001/travel
    Then the response status is 400
    And the response body contains "error" with value "Already in this city"
    And the response body contains "code" with value "SAME_CITY"

  @api
  Scenario: Lose condition triggered when steps reach 0
    Given the case "case-travel-001" has 1 remaining step
    And no warrant has been issued
    When the player travels to "tokyo" via POST /api/cases/case-travel-001/travel
    Then the response contains "remainingSteps" equal to 0
    And the response contains "caseStatus" equal to "lost"

  @api
  Scenario: No travel options at the final trail city
    Given the player is at the final city "cairo" in the trail
    When the player requests the current city via GET /api/cases/case-travel-001/city
    Then the response contains "travelOptions" as an empty array
    And the response contains "isFinalCity" equal to true

  @api
  Scenario: Cannot travel when case is completed
    Given the case "case-travel-001" has status "won"
    When the player travels to "tokyo" via POST /api/cases/case-travel-001/travel
    Then the response status is 409
    And the response body contains "error" with value "Case is already completed"
    And the response body contains "code" with value "CASE_COMPLETED"

  @api
  Scenario: Traveling to a decoy city still allows investigation
    Given the player travels to decoy city "mumbai"
    When the player requests the current city via GET /api/cases/case-travel-001/city
    Then the response contains a "city" object with "id" equal to "mumbai"
    And the response contains "npcs" as an array with 2 to 3 entries
    And the response contains "travelOptions" as an array with exactly 3 entries
    And 1 of the travel options is the correct trail city the player missed

  @api
  Scenario: Travel options are randomized in order
    When the player requests the current city twice via GET /api/cases/case-travel-001/city
    Then both responses contain the same 3 city IDs in "travelOptions"
    And the order of travel options may differ between requests

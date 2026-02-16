@ui
Feature: Game User Interface
  As a player, I want a visually engaging and accessible game interface
  so that I can enjoy investigating cases across different screens.

  @smoke @ui
  Scenario: Briefing screen displays case information
    Given a new case has been created with title "The Stolen Sapphire of Siam"
    When the player views the briefing screen at /briefing
    Then the case title "The Stolen Sapphire of Siam" is displayed as a heading
    And the stolen treasure name "Sapphire of Siam" is displayed
    And the stolen treasure description is displayed
    And a briefing narrative is displayed with a typewriter animation
    And a "Start Investigation" button is visible
    When the player clicks "Start Investigation"
    Then the player is navigated to the city screen at /city

  @smoke @ui
  Scenario: City screen shows city name, background, NPCs, travel options, and steps remaining
    Given the player is investigating in city "Bangkok"
    And the case has 7 remaining steps out of 10
    When the player views the city screen at /city
    Then the city name "Bangkok" is displayed
    And a city background image is displayed
    And 2 to 3 NPCs are listed with their names and roles
    And exactly 3 travel options are displayed with city names and descriptions
    And the step counter displays "Steps: 7/10"
    And a "Dossier" button is visible

  @ui
  Scenario: City screen shows warning when steps are low
    Given the player is investigating and has 3 remaining steps
    When the player views the city screen
    Then the step counter displays "Steps: 3/10" in a warning color

  @ui
  Scenario: City screen shows no travel options at final city
    Given the player is at the final trail city "Cairo"
    When the player views the city screen
    Then no travel options are displayed
    And a message "You've reached the end of the trail. Issue a warrant to make your arrest." is shown

  @smoke @ui
  Scenario: Dossier screen shows all suspects with traits
    When the player navigates to the dossier screen
    Then at least 10 suspect cards are displayed
    And each suspect card shows the suspect name
    And each suspect card shows a photo placeholder
    And each suspect card shows 6 traits with labels
    And each suspect card has an "Issue Warrant" button

  @ui
  Scenario: Dossier screen grid layout adapts to screen size
    When the player views the dossier on a desktop viewport of 1024px width
    Then suspect cards are arranged in a grid with 2 to 3 columns
    When the player views the dossier on a mobile viewport of 375px width
    Then suspect cards are arranged in a single column

  @smoke @ui
  Scenario: Summary screen shows outcome after winning
    Given the player won the case
    When the player views the summary screen at /summary
    Then the outcome badge displays "Won"
    And the stolen treasure details are shown
    And the cities visited are displayed as an ordered list
    And the steps used are displayed as "7/10"
    And the correct suspect card is shown
    And the player's warrant details are shown
    And a "Play Again" button is visible

  @ui
  Scenario: Summary screen shows outcome after losing
    Given the player lost the case by running out of steps
    When the player views the summary screen at /summary
    Then the outcome badge displays "Lost"
    And the correct suspect is revealed
    And a "Play Again" button is visible

  @ui
  Scenario: Play Again button starts a new case
    Given the player is on the summary screen
    When the player clicks "Play Again"
    Then a new case is created
    And the player is navigated to the briefing screen at /briefing

  @smoke @ui
  Scenario: SPA navigation without full page reloads
    Given the player is on the briefing screen
    When the player clicks "Start Investigation"
    Then navigation to /city occurs without a full page reload
    When the player clicks "Dossier"
    Then navigation to /dossier occurs without a full page reload

  @ui
  Scenario: Direct URL access without session redirects to home
    Given no active session exists in localStorage
    When the player navigates directly to /city
    Then the player is redirected to /
    And a new session is created

  @ui
  Scenario: Responsive layout on mobile viewport
    Given the player is on the city screen
    When the viewport width is 375px
    Then all panels are stacked vertically
    And tab navigation is shown with tabs "City", "Chat", "Travel", "Dossier"
    And no horizontal scrollbar is present

  @ui
  Scenario: Responsive layout on desktop viewport
    Given the player is on the city screen
    When the viewport width is 1024px
    Then side panels for NPC chat and travel options are displayed
    And no horizontal scrollbar is present

  @ui
  Scenario: Keyboard navigation through interactive elements
    Given the player is on the city screen
    When the player presses Tab
    Then focus moves through interactive elements in logical order
    And each focused element has a visible focus indicator of at least 2px outline
    When the player presses Enter on a focused NPC
    Then the NPC chat panel opens
    When the player presses Escape
    Then the chat panel closes

  @ui
  Scenario: NPC chat interface displays messages correctly
    Given the player has opened the chat with NPC "Somchai"
    When the player types "Have you seen anyone suspicious?" and presses Enter
    Then the player message appears right-aligned with a blue background
    And a typing indicator with animated dots is shown for 500ms
    And the NPC response appears left-aligned with a dark background and NPC name label
    And the character counter shows "0/280"

  @ui
  Scenario: Loading state during API calls
    When an API call is in progress
    Then a loading indicator is displayed
    And the loading indicator has aria-busy="true"

  @ui
  Scenario: API error shows dismissible toast notification
    When an API call returns an error
    Then a toast notification is displayed with the error message
    And the toast notification is dismissible

  @ui
  Scenario: Network error triggers retry with exponential backoff
    Given the network connection is lost
    When an API call fails
    Then a retry banner displays "Connection lost. Retrying..."
    And the system retries 3 times with delays of 1, 2, and 4 seconds
    When all 3 retries fail
    Then a persistent error message displays "Unable to connect. Please check your connection and refresh."

  @ui
  Scenario: Warrant confirmation dialog
    Given the player is on the dossier screen
    When the player clicks "Issue Warrant" on suspect "Carmen Sandiego"
    Then a confirmation dialog appears with text "Are you sure you want to issue a warrant for Carmen Sandiego? This action is irreversible."
    And the dialog has "Confirm" and "Cancel" buttons
    When the player clicks "Confirm"
    Then the warrant is submitted and the arrest result screen is shown

  @ui
  Scenario: Arrest result screen on win
    Given the player issued a correct warrant
    When the arrest result screen is displayed
    Then a celebratory visual is shown with a green theme
    And a success message mentioning the suspect name is displayed
    And a "View Summary" button is visible

  @ui
  Scenario: Arrest result screen on lose with wrong suspect
    Given the player issued a warrant for the wrong suspect
    When the arrest result screen is displayed
    Then a failure message is shown with a red theme
    And the correct suspect is revealed
    And a "View Summary" button is visible

  @ui
  Scenario: Briefing typewriter animation is skippable
    Given the briefing narrative is animating with the typewriter effect
    When the player clicks anywhere or presses any key
    Then the full briefing text is displayed immediately

  @ui
  Scenario: Chat input disabled when message cap reached
    Given the player has exchanged 20 messages with NPC "Somchai"
    When the player views the chat panel
    Then the input field is disabled
    And a message "This NPC has nothing more to say." is displayed

  @ui
  Scenario: Double-click prevention on travel button
    Given the player is on the city screen
    When the player rapidly clicks a travel option twice
    Then only one travel API call is made
    And the travel button is disabled after the first click

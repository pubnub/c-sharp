Feature: Running a test

Scenario: As a valid user I can log into my app
  Given my app is running
  And I wait for "PubNubMessaging" to appear
  Then I enter text "hello_world" into field with id "txtChannel"
  Then I press "Launch"

@featureSet=publish
Feature: Signal
  As a PubNub user
  I want to send signals
  So I can implement features in my application?

  @contract=successfulSignal @beta
  Scenario: Sending a signal
    Given the demo keyset
    When I send a signal
    Then I receive successful response
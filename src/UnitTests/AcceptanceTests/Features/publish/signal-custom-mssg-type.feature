@featureSet=signalWithCustomMssgType @beta
Feature: Send a signal to Space
  As a PubNub user I want to send some signals to Space with type.
  Client should be able to pass optional custom message type to the signal endpoint.

  Background:
    Given the demo keyset

  @contract=signalWithType
  Scenario: Send a signal success
    When I send a signal with 'test_message_type' customMessageType
    Then I receive a successful response

  @contract=signalWithTooShortType
  Scenario: Send a signal fails when type is too short
    When I send a signal with 'ts' customMessageType
    Then I receive an error response

  @contract=signalWithTooLongType
  Scenario: Send a signal fails when type is too long
    When I send a signal with 'this-is-really-long-message-type-to-be-used-with-publish' customMessageType
    Then I receive an error response

  @contract=signalWithTypeStartingWithReservedStrings
  Scenario: Send a signal fails when type starts with reserved 'pn-' (hyphen) string
    When I send a signal with 'pn-test_message_type' customMessageType
    Then I receive an error response

  @contract=signalWithTypeStartingWithReservedStrings
  Scenario: Send a signal fails when type starts with reserved 'pn_' (underscore) string
    When I send a signal with 'pn_test_message_type' customMessageType
    Then I receive an error response

  @contract=signalWithTypeStartingWithNotAllowedCharacter
  Scenario: Send a signal fails when type starts with not allowed '-' (hyphen) character
    When I send a signal with '-test_message_type' customMessageType
    Then I receive an error response

  @contract=signalWithTypeStartingWithNotAllowedCharacter
  Scenario: Send a signal fails when type starts with not allowed '_' (underscore) character
    When I send a signal with '_test_message_type' customMessageType
    Then I receive an error response

  @contract=signalWithTypeContainingNotAllowedCharacter
  Scenario: Send a signal fails when type contains not allowed characters
    When I send a signal with 'test:message_type' customMessageType
    Then I receive an error response

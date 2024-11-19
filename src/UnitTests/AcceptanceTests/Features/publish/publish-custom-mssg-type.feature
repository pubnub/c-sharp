@featureSet=publishWithCustomMssgType @beta
Feature: Publish to Space
  As a PubNub user I want to publish messages to Space with type.
  Client should be able to pass optional custom message type to publish endpoint.

  Background:
    Given the demo keyset

  @contract=publishWithType
  Scenario: Publish message success
    When I publish message with 'test_step' customMessageType
    Then I receive a successful response

  @contract=publishWithTooShortType
  Scenario: Publish message fails when type is too short
    When I publish message with 'ts' customMessageType
    Then I receive an error response

  @contract=publishWithTooLongType
  Scenario: Publish message fails when type is too long
    When I publish message with 'this-is-really-long-message-type-to-be-used-with-publish' customMessageType
    Then I receive an error response

  @contract=publishWithTypeStartingWithReservedStrings
  Scenario: Publish message fails when type starts with reserved 'pn-' (hyphen) string
    When I publish message with 'pn-test-step' customMessageType
    Then I receive an error response

  @contract=publishWithTypeStartingWithReservedStrings
  Scenario: Publish message fails when type starts with reserved 'pn_' (underscore) string
    When I publish message with 'pn_test-step' customMessageType
    Then I receive an error response

  @contract=publishWithTypeStartingWithNotAllowedCharacter
  Scenario: Publish message fails when type starts with not allowed '-' (hyphen) character
    When I publish message with '-test-step' customMessageType
    Then I receive an error response

  @contract=publishWithTypeStartingWithNotAllowedCharacter
  Scenario: Publish message fails when type starts with not allowed '_' (underscore) character
    When I publish message with '_test-step' customMessageType
    Then I receive an error response
	
  @contract=publishWithTypeContainingNotAllowedCharacter
  Scenario: Publish message fails when type contains not allowed characters
    When I publish message with 'test:step' customMessageType
    Then I receive an error response

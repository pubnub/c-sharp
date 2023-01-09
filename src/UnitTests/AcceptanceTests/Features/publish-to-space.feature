@featureSet=publishToSpace @beta
Feature: Publish to Space
  As a PubNub user I want to publish messages to Space with message type.
  Client should be able to pass optional spaceId and messageType to publish endpoint.

  Background:
    Given the demo keyset

  @contract=publishWithSpaceIdAndMessageType
  Scenario: Publish message to space success
    When I publish message with 'space-id' space id and 'test_step' message type
    Then I receive a successful response

  @contract=publishWithTooShortMessageType
  Scenario: Publish message to space fails when message type is too short
    When I publish message with 'test-space' space id and 'ts' message type
    Then I receive error response

  @contract=publishWithTooLongMessageType
  Scenario: Publish message to space fails when message type is too long
    When I publish message with 'test-space' space id and 'this-is-really-long-message-type-to-be-used-with-publish' message type
    Then I receive error response

  @contract=publishWithTooShortSpaceId
  Scenario: Publish message to space fails when space id is too short
    When I publish message with 'ts' space id and 'test-step' message type
    Then I receive error response

  @contract=publishWithTooLongSpaceId
  Scenario: Publish message to space fails when space id is too long
    When I publish message with 'this-is-really-long-identifier-for-space-id-to-be-used-with-publish' space id and 'test-step' message type
    Then I receive error response

  @contract=publishWithSpaceIdStartingWithReservedStrings
  Scenario: Publish message to space fails when space id starts with reserved 'pn-' (hyphen) string
    When I publish message with 'pn-test-space' space id and 'test-step' message type
    Then I receive error response

  @contract=publishWithSpaceIdStartingWithReservedStrings
  Scenario: Publish message to space fails when space id starts with reserved 'pn_' (underscore) string
    When I publish message with 'pn_test-space' space id and 'test-step' message type
    Then I receive error response

  @contract=publishWithSpaceIdStartingWithNotAllowedCharacter
  Scenario: Publish message to space fails when space id starts with not allowed '-' (hyphen) character
    When I publish message with '-test-space' space id and 'test-step' message type
    Then I receive error response

  @contract=publishWithSpaceIdStartingWithNotAllowedCharacter
  Scenario: Publish message to space fails when space id starts with not allowed '_' (underscore) character
    When I publish message with '_test-space' space id and 'test-step' message type
    Then I receive error response

  @contract=publishWithSpaceIdContainingNotAllowedCharacter
  Scenario: Publish message to space fails when space id contains not allowed characters
    When I publish message with 'test@space.com' space id and 'test-step' message type
    Then I receive error response

  @contract=publishWithMessageTypeStartingWithReservedStrings
  Scenario: Publish message to space fails when message type starts with reserved 'pn-' (hyphen) string
    When I publish message with 'test-space' space id and 'pn-test-step' message type
    Then I receive error response

  @contract=publishWithMessageTypeStartingWithReservedStrings
  Scenario: Publish message to space fails when message type starts with reserved 'pn_' (underscore) string
    When I publish message with 'test-space' space id and 'pn_test-step' message type
    Then I receive error response

  @contract=publishWithMessageTypeStartingWithNotAllowedCharacter
  Scenario: Publish message to space fails when message type starts with not allowed '-' (hyphen) character
    When I publish message with 'test-space' space id and '-test-step' message type
    Then I receive error response

  @contract=publishWithMessageTypeStartingWithNotAllowedCharacter
  Scenario: Publish message to space fails when message type starts with not allowed '_' (underscore) character
    When I publish message with 'test-space' space id and '_test-step' message type
    Then I receive error response

  @contract=publishWithMessageTypeContainingNotAllowedCharacter
  Scenario: Publish message to space fails when message type contains not allowed characters
    When I publish message with 'test-space' space id and 'test:step' message type
    Then I receive error response

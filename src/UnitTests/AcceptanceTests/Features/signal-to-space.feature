@featureSet=signalToSpace @beta
Feature: Send a signal to Space
  As a PubNub user I want to send some signals to Space with message type.
  Client should be able to pass optional spaceId and messageType to the signal endpoint.

  Background:
    Given the demo keyset

  @contract=signalWithSpaceIdAndMessageType
  Scenario: Send a signal to space success
    When I send a signal with 'space-id' space id and 'test_message_type' message type
    Then I receive a successful response

  @contract=signalWithTooShortMessageType
  Scenario: Send a signal to space fails when message type is too short
    When I send a signal with 'test-space' space id and 'ts' message type
    Then I receive an error response

  @contract=signalWithTooLongMessageType
  Scenario: Send a signal to space fails when message type is too long
    When I send a signal with 'test-space' space id and 'this-is-really-long-message-type-to-be-used-with-publish' message type
    Then I receive an error response

  @contract=signalWithTooShortSpaceId
  Scenario: Send a signal to space fails when space id is too short
    When I send a signal with 'ts' space id and 'test_message_type' message type
    Then I receive an error response

  @contract=signalWithTooLongSpaceId
  Scenario: Send a signal to space fails when space id is too long
    When I send a signal with 'this-is-really-long-identifier-for-space-id-to-be-used-with-publish' space id and 'test-step' message type
    Then I receive an error response

  @contract=signalWithSpaceIdStartingWithReservedStrings
  Scenario: Send a signal to space fails when space id starts with reserved 'pn-' (hyphen) string
    When I send a signal with 'pn-test-space' space id and 'test_message_type' message type
    Then I receive an error response

  @contract=signalWithSpaceIdStartingWithReservedStrings
  Scenario: Send a signal to space fails when space id starts with reserved 'pn_' (underscore) string
    When I send a signal with 'pn_test-space' space id and 'test_message_type' message type
    Then I receive an error response

  @contract=signalWithSpaceIdStartingWithNotAllowedCharacter
  Scenario: Send a signal to space fails when space id starts with not allowed '-' (hyphen) character
    When I send a signal with '-test-space' space id and 'test_message_type' message type
    Then I receive an error response

  @contract=signalWithSpaceIdStartingWithNotAllowedCharacter
  Scenario: Send a signal to space fails when space id starts with not allowed '_' (underscore) character
    When I send a signal with '_test-space' space id and 'test_message_type' message type
    Then I receive an error response

  @contract=signalWithSpaceIdContainingNotAllowedCharacter
  Scenario: Send a signal to space fails when space id contains not allowed characters
    When I send a signal with 'test@space.com' space id and 'test_message_type' message type
    Then I receive an error response

  @contract=signalWithMessageTypeStartingWithReservedStrings
  Scenario: Send a signal to space fails when message type starts with reserved 'pn-' (hyphen) string
    When I send a signal with 'test-space' space id and 'pn-test_message_type' message type
    Then I receive an error response

  @contract=signalWithMessageTypeStartingWithReservedStrings
  Scenario: Send a signal to space fails when message type starts with reserved 'pn_' (underscore) string
    When I send a signal with 'test-space' space id and 'pn_test_message_type' message type
    Then I receive an error response

  @contract=signalWithMessageTypeStartingWithNotAllowedCharacter
  Scenario: Send a signal to space fails when message type starts with not allowed '-' (hyphen) character
    When I send a signal with 'test-space' space id and '-test_message_type' message type
    Then I receive an error response

  @contract=signalWithMessageTypeStartingWithNotAllowedCharacter
  Scenario: Send a signal to space fails when message type starts with not allowed '_' (underscore) character
    When I send a signal with 'test-space' space id and '_test_message_type' message type
    Then I receive an error response

  @contract=signalWithMessageTypeContainingNotAllowedCharacter
  Scenario: Send a signal to space fails when message type contains not allowed characters
    When I send a signal with 'test-space' space id and 'test:message_type' message type
    Then I receive an error response

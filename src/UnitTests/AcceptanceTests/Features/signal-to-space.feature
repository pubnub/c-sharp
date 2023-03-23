@featureSet=signalToSpace @beta
Feature: Send a signal to Space
  As a PubNub user I want to send some signals to Space with type.
  Client should be able to pass optional spaceId and type to the signal endpoint.

  Background:
    Given the demo keyset

  @contract=signalWithSpaceIdAndType
  Scenario: Send a signal to space success
    When I send a signal with 'space-id' space id and 'test_message_type' type
    Then I receive a successful response

  @contract=signalWithTooShortType
  Scenario: Send a signal to space fails when type is too short
    When I send a signal with 'test-space' space id and 'ts' type
    Then I receive an error response

  @contract=signalWithTooLongType
  Scenario: Send a signal to space fails when type is too long
    When I send a signal with 'test-space' space id and 'this-is-really-long-message-type-to-be-used-with-publish' type
    Then I receive an error response

  @contract=signalWithTooShortSpaceId
  Scenario: Send a signal to space fails when space id is too short
    When I send a signal with 'ts' space id and 'test_message_type' type
    Then I receive an error response

  @contract=signalWithTooLongSpaceId
  Scenario: Send a signal to space fails when space id is too long
    When I send a signal with 'this-is-really-long-identifier-for-space-id-to-be-used-with-publish' space id and 'test-step' type
    Then I receive an error response

  @contract=signalWithSpaceIdStartingWithReservedStrings
  Scenario: Send a signal to space fails when space id starts with reserved 'pn-' (hyphen) string
    When I send a signal with 'pn-test-space' space id and 'test_message_type' type
    Then I receive an error response

  @contract=signalWithSpaceIdStartingWithReservedStrings
  Scenario: Send a signal to space fails when space id starts with reserved 'pn_' (underscore) string
    When I send a signal with 'pn_test-space' space id and 'test_message_type' type
    Then I receive an error response

  @contract=signalWithSpaceIdStartingWithNotAllowedCharacter
  Scenario: Send a signal to space fails when space id starts with not allowed '-' (hyphen) character
    When I send a signal with '-test-space' space id and 'test_message_type' type
    Then I receive an error response

  @contract=signalWithSpaceIdStartingWithNotAllowedCharacter
  Scenario: Send a signal to space fails when space id starts with not allowed '_' (underscore) character
    When I send a signal with '_test-space' space id and 'test_message_type' type
    Then I receive an error response

  @contract=signalWithSpaceIdContainingNotAllowedCharacter
  Scenario: Send a signal to space fails when space id contains not allowed characters
    When I send a signal with 'test@space.com' space id and 'test_message_type' type
    Then I receive an error response

  @contract=signalWithTypeStartingWithReservedStrings
  Scenario: Send a signal to space fails when type starts with reserved 'pn-' (hyphen) string
    When I send a signal with 'test-space' space id and 'pn-test_message_type' type
    Then I receive an error response

  @contract=signalWithTypeStartingWithReservedStrings
  Scenario: Send a signal to space fails when type starts with reserved 'pn_' (underscore) string
    When I send a signal with 'test-space' space id and 'pn_test_message_type' type
    Then I receive an error response

  @contract=signalWithTypeStartingWithNotAllowedCharacter
  Scenario: Send a signal to space fails when type starts with not allowed '-' (hyphen) character
    When I send a signal with 'test-space' space id and '-test_message_type' type
    Then I receive an error response

  @contract=signalWithTypeStartingWithNotAllowedCharacter
  Scenario: Send a signal to space fails when type starts with not allowed '_' (underscore) character
    When I send a signal with 'test-space' space id and '_test_message_type' type
    Then I receive an error response

  @contract=signalWithTypeContainingNotAllowedCharacter
  Scenario: Send a signal to space fails when type contains not allowed characters
    When I send a signal with 'test-space' space id and 'test:message_type' type
    Then I receive an error response

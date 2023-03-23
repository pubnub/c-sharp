@featureSet=publishToSpace @beta
Feature: Publish to Space
  As a PubNub user I want to publish messages to Space with type.
  Client should be able to pass optional spaceId and type to publish endpoint.

  Background:
    Given the demo keyset

  @contract=publishWithSpaceIdAndType
  Scenario: Publish message to space success
    When I publish message with 'space-id' space id and 'test_step' type
    Then I receive a successful response

  @contract=publishWithTooShortType
  Scenario: Publish message to space fails when type is too short
    When I publish message with 'test-space' space id and 'ts' type
    Then I receive an error response

  @contract=publishWithTooLongType
  Scenario: Publish message to space fails when type is too long
    When I publish message with 'test-space' space id and 'this-is-really-long-message-type-to-be-used-with-publish' type
    Then I receive an error response

  @contract=publishWithTooShortSpaceId
  Scenario: Publish message to space fails when space id is too short
    When I publish message with 'ts' space id and 'test-step' type
    Then I receive an error response

  @contract=publishWithTooLongSpaceId
  Scenario: Publish message to space fails when space id is too long
    When I publish message with 'this-is-really-long-identifier-for-space-id-to-be-used-with-publish' space id and 'test-step' type
    Then I receive an error response

  @contract=publishWithSpaceIdStartingWithReservedStrings
  Scenario: Publish message to space fails when space id starts with reserved 'pn-' (hyphen) string
    When I publish message with 'pn-test-space' space id and 'test-step' type
    Then I receive an error response

  @contract=publishWithSpaceIdStartingWithReservedStrings
  Scenario: Publish message to space fails when space id starts with reserved 'pn_' (underscore) string
    When I publish message with 'pn_test-space' space id and 'test-step' type
    Then I receive an error response

  @contract=publishWithSpaceIdStartingWithNotAllowedCharacter
  Scenario: Publish message to space fails when space id starts with not allowed '-' (hyphen) character
    When I publish message with '-test-space' space id and 'test-step' type
    Then I receive an error response

  @contract=publishWithSpaceIdStartingWithNotAllowedCharacter
  Scenario: Publish message to space fails when space id starts with not allowed '_' (underscore) character
    When I publish message with '_test-space' space id and 'test-step' type
    Then I receive an error response

  @contract=publishWithSpaceIdContainingNotAllowedCharacter
  Scenario: Publish message to space fails when space id contains not allowed characters
    When I publish message with 'test@space.com' space id and 'test-step' type
    Then I receive an error response

  @contract=publishWithTypeStartingWithReservedStrings
  Scenario: Publish message to space fails when type starts with reserved 'pn-' (hyphen) string
    When I publish message with 'test-space' space id and 'pn-test-step' type
    Then I receive an error response

  @contract=publishWithTypeStartingWithReservedStrings
  Scenario: Publish message to space fails when type starts with reserved 'pn_' (underscore) string
    When I publish message with 'test-space' space id and 'pn_test-step' type
    Then I receive an error response

  @contract=publishWithTypeStartingWithNotAllowedCharacter
  Scenario: Publish message to space fails when type starts with not allowed '-' (hyphen) character
    When I publish message with 'test-space' space id and '-test-step' type
    Then I receive an error response

  @contract=publishWithTypeStartingWithNotAllowedCharacter
  Scenario: Publish message to space fails when type starts with not allowed '_' (underscore) character
    When I publish message with 'test-space' space id and '_test-step' type
    Then I receive an error response
	
  @contract=publishWithTypeContainingNotAllowedCharacter
  Scenario: Publish message to space fails when type contains not allowed characters
    When I publish message with 'test-space' space id and 'test:step' type
    Then I receive an error response

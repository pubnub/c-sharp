@featureSet=publishToSpace @beta
Feature: Publish to Space
  As a PubNub user I want to publish messages to Space with message type.
  Client should be able to pass optional spaceId and messageType to publish endpoint.

  Background:
    Given the demo keyset

  @contract=publishWithSpaceIdAndMessageType
  Scenario: Publish message with space id and message type
    When I publish message with 'space-id' space id and 'test_step' message type
    Then I receive a successful response

  @contract=publishWithInvalidSpaceIdAndMessageType
  Scenario: Publish message with space id and too short message type
    When I publish message with 'test-space' space id and 'ts' message type
    Then I receive error response

  @contract=publishWithInvalidSpaceIdAndMessageType
  Scenario: Publish message with space id and too long message type
    When I publish message with 'test-space' space id and 'this-is-really-long-message-type-to-be-used-with-publish' message type
    Then I receive error response

  @contract=publishWithInvalidSpaceIdAndMessageType
  Scenario: Publish message with short too space id and message type
    When I publish message with 'ts' space id and 'test-step' message type
    Then I receive error response

  @contract=publishWithInvalidSpaceIdAndMessageType
  Scenario: Publish message with too long space id and message type
    When I publish message with 'this-is-really-long-identifier-for-space-id-to-be-used-with-publish' space id and 'test-step' message type
    Then I receive error response

  @contract=publishWithInvalidSpaceIdAndMessageType
  Scenario: Publish message with space id containing unexpected chars
    When I publish message with 'test@space.com' space id and 'test-step' message type
    Then I receive error response

  @contract=publishWithInvalidSpaceIdAndMessageType
  Scenario: Publish message with space id containing reserved 'pn' prefix
    When I publish message with 'pntest-space' space id and 'test-step' message type
    Then I receive error response

  @contract=publishWithInvalidSpaceIdAndMessageType
  Scenario: Publish message with message type containing unexpected chars
    When I publish message with 'test-space' space id and 'test:step' message type
    Then I receive error response

  @contract=publishWithInvalidSpaceIdAndMessageType
  Scenario: Publish message with message type containing reserved 'pn' prefix
    When I publish message with 'test-space' space id and 'pntest-step' message type
    Then I receive error response

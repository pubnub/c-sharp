@featureSet=subscribeVSP @beta
Feature: Subscribe for VSP
  As a PubNub user I want to subscribe and receive message type and space id information.
  Client should be able to receive message type and space id from subscribe response without any
  additional options set (like `includeMessageType` and `includeSpaceId` for other API).

  Background:
    Given the demo keyset

  @contract=subscribeReceiveMessagesWithPubNubMessageTypes
  Scenario: Client can subscribe and receive messages with PubNub message types
    When I subscribe to 'simple-channel' channel
    Then I receive 2 messages in my subscribe response
    And response contains messages with 'message' and 'signal' message types
    And response contains messages without space ids

  @contract=subscribeReceiveMessagesWithUserAndPubNubMessageTypes
  Scenario: Client can subscribe and receive messages with PubNub and user-defined message types
    When I subscribe to 'vsp-channel' channel
    Then I receive 2 messages in my subscribe response
    And response contains messages with 'message' and 'vc-message' message types
    And response contains messages with space ids
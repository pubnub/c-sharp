@featureSet=subscribeVSP @beta
Feature: Subscribe for VSP
  As a PubNub user I want to subscribe and receive type and space id information.
  Client should be able to receive message type and space id from subscribe response without any
  additional options set (like `includeType` and `includeSpaceId` for other API).

  Background:
    Given the demo keyset

  @contract=subscribeReceiveMessagesWithTypes
  Scenario: Client can subscribe and receive messages with types
    When I subscribe to 'vsp-channel' channel
    Then I receive 2 messages in my subscribe response
    And response contains messages with 'custom-message' and 'vc-message' types
    And response contains messages with space ids
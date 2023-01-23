@featureSet=historyVSP @beta
Feature: History for VSP
  As a PubNub user I want to fetch history with message type and space id information.
  Client should be able to opt-out default `includeMessageType` and specify whether space id should
  be returned with `includeSpaceId`.

  Background:
    Given the demo keyset with enabled storage

  @contract=fetchHistoryWithPubNubMessageTypes
  Scenario: Client can fetch history with PubNub message types using defaults
    When I fetch message history for 'simple-channel' channel
    Then I receive a successful response
    And history response contains messages with 'message' and 'file' message types
    And history response contains messages without space ids

  @contract=fetchHistoryWithUserAndPubNubMessageTypes
  Scenario: Client can fetch history with PubNub and user-defined message types using defaults
    When I fetch message history for 'vsp-channel' channel
    Then I receive a successful response
    And history response contains messages with 'message' and 'vc-message' message types
    And history response contains messages without space ids

  @contract=fetchHistoryWithoutMessageTypes
  Scenario: Client can fetch history without message types enabled by default
    When I fetch message history with 'includeMessageType' set to 'false' for 'vsp-channel' channel
    Then I receive a successful response
    And history response contains messages without message types
    And history response contains messages without space ids

  @contract=fetchHistoryWithSpaceIdAndMessageType
  Scenario: Client can fetch history with space id disabled by default
    When I fetch message history with 'includeSpaceId' set to 'true' for 'vsp-channel' channel
    Then I receive a successful response
    And history response contains messages with message types
    And history response contains messages with space ids



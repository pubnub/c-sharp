@featureSet=historyVSP @beta
Feature: History for VSP
  As a PubNub user I want to fetch history with message type and space id information.
  Client should be able to opt-out default `includeType` and specify whether space id should
  be returned with `includeSpaceId`.

  Background:
    Given the demo keyset with enabled storage

  @contract=fetchHistoryWithPubNubMessageTypes
  Scenario: Client can fetch history with message types
    When I fetch message history for 'simple-channel' channel
    Then I receive a successful response
    And history response contains messages with '0' and '4' message types
    And history response contains messages without space ids

  @contract=fetchHistoryWithUserAndPubNubTypes
  Scenario: Client can fetch history with types
    When I fetch message history for 'vsp-channel' channel
    Then I receive a successful response
    And history response contains messages with 'custom-message' and 'vc-message' types
    And history response contains messages without space ids

  @contract=fetchHistoryWithoutTypes
  Scenario: Client can fetch history without types enabled by default
    When I fetch message history with 'includeType' set to 'false' for 'vsp-channel' channel
    Then I receive a successful response
    And history response contains messages without types
    And history response contains messages without space ids

  @contract=fetchHistoryWithSpaceId
  Scenario: Client can fetch history with space id disabled by default
    When I fetch message history with 'includeSpaceId' set to 'true' for 'vsp-channel' channel
    Then I receive a successful response
    And history response contains messages with space ids



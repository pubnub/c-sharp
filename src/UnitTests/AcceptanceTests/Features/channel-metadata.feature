@featureSet=objectsV2 @beta
Feature: Objects V2 Channel metadata
  As a PubNub customer I want to create, update, remove channels.

  Background:
    Given I have a keyset with Objects V2 enabled

  @contract=getChannelMetadataOfChat
  Scenario: Get a channel metadata for id
    Given the id for 'Chat' channel
    When I get the channel metadata
    Then I receive a successful response
    And the channel metadata for 'Chat' channel

  @contract=getChannelMetadataOfDMWithCustom
  Scenario: Get a channel with custom metadata
    Given the id for 'DM' channel
    When I get the channel metadata with custom
    Then I receive a successful response
    And the channel metadata for 'DM' channel

  @contract=setChannelMetadataForChat
  Scenario: Set a channel metadata
    Given the data for 'Chat' channel
    When I set the channel metadata
    Then I receive a successful response
    And the channel metadata for 'Chat' channel contains updated

  @contract=removeChannelMetadataOfChat
  Scenario: Remove a channel metadata for id
    Given the id for 'Chat' channel
    When I remove the channel metadata
    Then I receive a successful response

  @contract=getAllChannelMetadata
  Scenario: Get all channel metadata
    When I get all channel metadata
    Then I receive a successful response
    And the response contains list with 'Chat' and 'Patient' channel metadata

  @contract=getAllChannelMetadataWithCustom
  Scenario: Get all channel metadata with custom
    When I get all channel metadata with custom
    Then I receive a successful response
    And the response contains list with 'DM' and 'VipChat' channel metadata

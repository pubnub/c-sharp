@featureSet=objectsV2 @beta
Feature: Objects V2 Members
  As a PubNub customer I want to create, get, remove and update channel members(UUIDs).

  Background:
    Given I have a keyset with Objects V2 enabled

  @contract=getMembersOfChatChannel
  Scenario: Get members for a channel
    Given the id for 'Chat' channel
    When I get the channel members
    Then I receive a successful response
    And the response contains list with 'AmeliaMember' and 'EvaMember' members

  @contract=getMembersOfVipChatChannelWithCustomAndUuidWithCustom
  Scenario: Get members for VipChat channel with custom and UUID with custom
    Given the id for 'VipChat' channel
    When I get the channel members including custom and UUID custom information
    Then I receive a successful response
    And the response contains list with 'OliverMember' and 'PeterMember' members

  @contract=setMembersForChatChannel
  Scenario: Set member for a channel
    Given the data for 'AmeliaMember' member
    And the id for 'Chat' channel
    When I set a channel member
    Then I receive a successful response
    And the response contains list with 'AmeliaMember' member

  @contract=setMembersForChatChannelWithCustomAndUuidWithCustom
  Scenario: Set member with custom for a channel and UUID with custom
    Given the data for 'PeterMember' member
    And the id for 'Chat' channel
    When I set a channel member including custom and UUID with custom
    Then I receive a successful response
    And the response contains list with 'PeterMember' member

  @contract=removeMembersForChatChannel
  Scenario: Remove member for a channel
    Given the id for 'Chat' channel
    And the data for 'AmeliaMember' member that we want to remove
    When I remove a channel member
    Then I receive a successful response

  @contract=manageMembersForChatChannel @na=ruby @na=js
  Scenario: Manage members for a channel
    Given the data for 'PeterMember' member
    And the data for 'AmeliaMember' member that we want to remove
    And the id for 'Chat' channel
    When I manage channel members
    Then I receive a successful response
    And the response contains list with 'PeterMember' member
    And the response does not contain list with 'AmeliaMember' member

# @featureSet=objectsV2 @beta
# Feature: Objects V2 Memberships
#   As a PubNub customer I want to create, update, remove channels.

#   Background:
#     Given I have a keyset with Objects V2 enabled

#   @contract=getAliceMemberships
#   Scenario: Get memberships for UUID
#     Given the id for 'Alice' persona
#     When I get the memberships
#     Then I receive a successful response
#     And the response contains list with 'ChatMembership' and 'PatientMembership' memberships

#   @contract=getAliceMemberships
#   Scenario: Get memberships for current user
#     Given current user is 'Alice' persona
#     When I get the memberships for current user
#     Then I receive a successful response
#     And the response contains list with 'ChatMembership' and 'PatientMembership' memberships

#   @contract=getBobMembershipWithCustomAndChannelCustom
#   Scenario: Get memberships for UUID with custom and channel custom
#     Given the id for 'Bob' persona
#     When I get the memberships including custom and channel custom information
#     Then I receive a successful response
#     And the response contains list with 'VipChatMembership' and 'DMMembership' memberships

#   @contract=setAliceMembership
#   Scenario: Set membership
#     Given the id for 'Alice' persona
#     And the data for 'ChatMembership' membership
#     When I set the membership
#     Then I receive a successful response
#     And the response contains list with 'ChatMembership' membership

#   @contract=setAliceMembership
#   Scenario: Set membership for current user
#     Given current user is 'Alice' persona
#     And the data for 'ChatMembership' membership
#     When I set the membership for current user
#     Then I receive a successful response
#     And the response contains list with 'ChatMembership' membership

#   @contract=removeAliceMembership
#   Scenario: Remove membership
#     Given the id for 'Alice' persona
#     And the data for 'ChatMembership' membership
#     When I remove the membership
#     Then I receive a successful response

#   @contract=removeAliceMembership
#   Scenario: Remove membership for current user
#     Given current user is 'Alice' persona
#     And the data for 'ChatMembership' membership that we want to remove
#     When I remove the membership for current user
#     Then I receive a successful response

#   @contract=manageAliceMemberships
#   Scenario: Manage memberships for a UUID
#     Given the id for 'Alice' persona
#     And the data for 'ChatMembership' membership
#     And the data for 'PatientMembership' membership that we want to remove
#     When I manage memberships
#     Then I receive a successful response
#     And the response contains list with 'ChatMembership' membership
#     And the response does not contain list with 'PatientMembership' membership

@featureSet=files @beta
  Feature: Send a file to Space
    As a PubNub user I want to send a file with custom message type.
    Client should be able to pass optional custom message type to the File endpoint.

    Background:
      Given the demo keyset

    @contract=sendFileWithType
    Scenario: Send a file success
      When I send a file with 'test_message_type' customMessageType
      Then I receive a successful response
#
    @contract=sendFileWithTooShortType
    Scenario: Send a file fails when message type is too short, shorten than 3 characters
      When I send a file with 'ts' customMessageType
      Then I receive an error response
#
    @contract=sendFileWithTooLongType
    Scenario: Send a file fails when message type is too long, longer than 50 characters
      When I send a file with 'this-is-really-long-message-type-to-be-used-with-publish' customMessageType
      Then I receive an error response
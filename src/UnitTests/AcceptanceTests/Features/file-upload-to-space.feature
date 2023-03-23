@featureSet=files @beta
  Feature: Send a file to Space
    As a PubNub user I want to send a file to Space with message type.
    Client should be able to pass optional spaceId and type to the File endpoint.

    Background:
      Given the demo keyset

    @contract=sendFileWithSpaceIdAndType
    Scenario: Send a file to space success
      When I send a file with 'space-id' space id and 'test_message_type' type
      Then I receive a successful response

    @contract=sendFileWithTooShortType
    Scenario: Send a file to space fails when message type is too short, shorten than 3 characters
      When I send a file with 'space-id' space id and 'ts' type
      Then I receive an error response

    @contract=sendFileWithTooLongType
    Scenario: Send a file to space fails when message type is too long, longer than 50 characters
      When I send a file with 'space-id' space id and 'this-is-really-long-message-type-to-be-used-with-publish' type
      Then I receive an error response
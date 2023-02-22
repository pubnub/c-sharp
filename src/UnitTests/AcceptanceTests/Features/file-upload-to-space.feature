@featureSet=files
  Feature: Send a file to Space
    As a PubNub user I want to send a file to Space with message type.
    Client should be able to pass optional spaceId and messageType to the File endpoint.

    Background:
      Given the demo keyset

    @contract=sendFileWithSpaceIdAndMessageType
    Scenario: Send a file to space success
      When I send a file with 'space-id' space id and 'test_message_type' message type
      Then I receive a successful response

    @contract=sendFileWithTooShortMessageType
    Scenario: Send a file to space fails when message type is too short, shorten than 3 characters
      When I send a file with 'space-id' space id and 'ts' message type
      Then I receive an error response

    @contract=sendFileWithTooLongMessageType
    Scenario: Send a file to space fails when message type is too long, longer than 50 characters
      When I send a file with 'space-id' space id and 'this-is-really-long-message-type-to-be-used-with-publish' message type
      Then I receive an error response
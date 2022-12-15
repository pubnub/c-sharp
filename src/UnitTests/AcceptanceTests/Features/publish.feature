@featureSet=publish
Feature: Publish a message
  As a PubNub user
  I want to publish messages
  So I can use PubNub

  @contract=simplePublish @beta
  Scenario: Publishing a message
    Given the demo keyset
    When I publish a message
    Then I receive successful response

  @contract=simplePublish @beta
  Scenario: Publishing a message with JSON metadata
    Given the demo keyset
    When I publish a message with JSON metadata
    Then I receive successful response

  @contract=simplePublish @beta
  Scenario: Publishing a message with string metadata
    Given the demo keyset
    When I publish a message with string metadata
    Then I receive successful response

  @contract=invalidPublish @beta
  Scenario: Failing publish
    Given the invalid keyset
    When I publish a message
    Then I receive error response

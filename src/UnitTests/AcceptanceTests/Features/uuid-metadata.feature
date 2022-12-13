@featureSet=objectsV2 @beta
Feature: Objects V2 UUID metadata
  As a PubNub customer I want to create, update, remove uuids.

  Background:
    Given I have a keyset with Objects V2 enabled

  @contract=getUUIDMetadataOfAlice
  Scenario: Get a UUID metadata for id
    Given the id for 'Alice' persona
    When I get the UUID metadata
    Then I receive a successful response
    And the UUID metadata for 'Alice' persona

  @contract=getUUIDMetadataOfBobWithCustom
  Scenario: Get a UUID with custom metadata, id stored in config
    Given current user is 'Bob' persona
    When I get the UUID metadata with custom for current user
    Then I receive a successful response
    And the UUID metadata for 'Bob' persona

  @contract=setUUIDMetadataForAlice
  Scenario: Set a UUID metadata
    Given the data for 'Alice' persona
    When I set the UUID metadata
    Then I receive a successful response
    And the UUID metadata for 'Alice' persona contains updated

  @contract=removeUUIDMetadataOfAlice
  Scenario: Remove a UUID metadata for id
    Given the id for 'Alice' persona
    When I remove the UUID metadata
    Then I receive a successful response

  @contract=removeUUIDMetadataOfAlice
  Scenario: Remove a UUID metadata, id stored in config
    Given current user is 'Alice' persona
    When I remove the UUID metadata for current user
    Then I receive a successful response

  @contract=getAllUUIDMetadata
  Scenario: Get all UUID metadata
    When I get all UUID metadata
    Then I receive a successful response
    And the response contains list with 'Alice' and 'James' UUID metadata

  @contract=getAllUUIDMetadataWithCustom
  Scenario: Get all UUID metadata with custom
    When I get all UUID metadata with custom
    Then I receive a successful response
    And the response contains list with 'Bob' and 'Lisa' UUID metadata

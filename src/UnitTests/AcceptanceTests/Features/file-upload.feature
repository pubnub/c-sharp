@featureSet=files
Feature: Files
  As a PubNub user
  I want to upload, manage and download files
  So my customers can share files

  @contract=listingFiles @beta
  Scenario: Listing files
    Given the demo keyset
    When I list files
    Then I receive successful response

  @contract=publishingFileMessage @beta
  Scenario: Publishing file message
    Given the demo keyset
    When I publish file message
    Then I receive successful response

  @contract=publishingFileMessageFailure @beta
  Scenario: Publishing file message failure
    Given the demo keyset
    When I publish file message
    Then I receive error response

  @contract=deletingFile @beta
  Scenario: Deleting a file
    Given the demo keyset
    When I delete file
    Then I receive successful response

  @contract=downloadingFile @beta
  Scenario: Downloading a file
    Given the demo keyset
    When I download file
    Then I receive successful response

  @skip @beta
  Scenario: Sending file
    Given the demo keyset
    When I send file
    Then I receive successful response

  @skip @beta
  Scenario: Sending file with retries
    Given the demo keyset
    When I send file
    Then I receive successful response

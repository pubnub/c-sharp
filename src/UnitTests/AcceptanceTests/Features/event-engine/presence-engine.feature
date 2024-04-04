# @featureSet=presenceEventEngine @beta
# Feature: Presence Event Engine
#   Validating the correctness of EE for Presence events

#   Background:
#     Given the demo keyset with Presence EE enabled

#   @contract=presenceTestMultipleWait
#   Scenario: Successfully joined a few channels
#     Given heartbeatInterval set to '1', timeout set to '60' and suppressLeaveEvents set to 'false'
#     When I join 'first', 'second', 'third' channels
#     Then I wait '3' seconds
#     And I observe the following Events and Invocations of the Presence EE:
#       | type       | name              |
#       | event      | JOINED            |
#       | invocation | HEARTBEAT         |
#       | event      | HEARTBEAT_SUCCESS |
#       | invocation | WAIT              |
#       | event      | TIMES_UP          |
#       | invocation | CANCEL_WAIT       |
#       | invocation | HEARTBEAT         |
#       | event      | HEARTBEAT_SUCCESS |
#       | invocation | WAIT              |
#       | event      | TIMES_UP          |
#       | invocation | CANCEL_WAIT       |
#       | invocation | HEARTBEAT         |

#   @contract=presenceJoin
#   Scenario: Successfully joined a few channels with presence
#     Given heartbeatInterval set to '1', timeout set to '60' and suppressLeaveEvents set to 'false'
#     When I join 'first', 'second', 'third' channels with presence
#     Then I wait for getting Presence joined events
#     And I observe the following Events and Invocations of the Presence EE:
#       | type       | name              |
#       | event      | JOINED            |
#       | invocation | HEARTBEAT         |
#       | event      | HEARTBEAT_SUCCESS |
#       | invocation | WAIT              |

#   @contract=presenceJoinWithAnError
#   Scenario: Recovery from one unexpected error along the way
#     Given heartbeatInterval set to '1', timeout set to '60' and suppressLeaveEvents set to 'false'
#     Given a linear reconnection policy with 3 retries
#     When I join 'first', 'second', 'third' channels with presence
#     Then I wait for getting Presence joined events
#     And I observe the following Events and Invocations of the Presence EE:
#       | type       | name                     |
#       | event      | JOINED                   |
#       | invocation | HEARTBEAT                |
#       | event      | HEARTBEAT_FAILURE        |
#       | invocation | DELAYED_HEARTBEAT        |
#       | event      | HEARTBEAT_SUCCESS        |
#       | invocation | CANCEL_DELAYED_HEARTBEAT |
#       | invocation | WAIT                     |

#   @contract=presenceJoinWithContinuousFailures
#   Scenario: Complete handshake failure
#     Given heartbeatInterval set to '1', timeout set to '60' and suppressLeaveEvents set to 'false'
#     Given a linear reconnection policy with 3 retries
#     When I join 'first', 'second', 'third' channels
#     Then I receive an error in my heartbeat response
#     And I observe the following Events and Invocations of the Presence EE:
#       | type       | name                     |
#       | event      | JOINED                   |
#       | invocation | HEARTBEAT                |
#       | event      | HEARTBEAT_FAILURE        |
#       | invocation | DELAYED_HEARTBEAT        |
#       | event      | HEARTBEAT_FAILURE        |
#       | invocation | CANCEL_DELAYED_HEARTBEAT |
#       | invocation | DELAYED_HEARTBEAT        |
#       | event      | HEARTBEAT_FAILURE        |
#       | invocation | CANCEL_DELAYED_HEARTBEAT |
#       | invocation | DELAYED_HEARTBEAT        |
#       | event      | HEARTBEAT_FAILURE        |
#       | invocation | CANCEL_DELAYED_HEARTBEAT |
#       | invocation | DELAYED_HEARTBEAT        |
#       | event      | HEARTBEAT_GIVEUP         |
#       | invocation | CANCEL_DELAYED_HEARTBEAT |

#   @contract=presenceLeave
#   Scenario: Joining and leaving channels
#     Given heartbeatInterval set to '20', timeout set to '60' and suppressLeaveEvents set to 'false'
#     When I join 'first', 'second', 'third' channels with presence
#     Then I wait for getting Presence joined events
#     Then I leave 'first' and 'second' channels with presence
#     Then I wait '3' seconds
#     And I observe the following Events and Invocations of the Presence EE:
#       | type       | name              |
#       | event      | JOINED            |
#       | invocation | HEARTBEAT         |
#       | event      | HEARTBEAT_SUCCESS |
#       | invocation | WAIT              |
#       | event      | LEFT              |
#       | invocation | CANCEL_WAIT       |
#       | invocation | LEAVE             |
#       | invocation | HEARTBEAT         |
#       | event      | HEARTBEAT_SUCCESS |
#       | invocation | WAIT              |

#   @contract=presenceJoinWithHeartbeatDisabled
#   Scenario: Joining a few channels with heartbeat interval set to 0
#     Given heartbeatInterval set to '0', timeout set to '60' and suppressLeaveEvents set to 'false'
#     When I join 'first', 'second', 'third' channels
#     Then I don't observe any Events and Invocations of the Presence EE
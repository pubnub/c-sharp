$BASE = "http://objekts.core.az1.pdx1.aws.int.ps.pn/datasync/subkeys/sub-c-bd4cd136-8eca-45c2-b5db-73d3b43d6552"

# ========================
# 1. Entity class: integration-test-vehicle v1
#    Used by: Entity tests, Relationship tests (as both entity A and B)
# ========================
#Write-Host "`n=== 1. CREATE ENTITY CLASS: integration-test-vehicle v1 ===" -ForegroundColor Cyan
#'{"data":{"config":{"ttlSec":-1}}}' | `
#  curl.exe -X POST `
#  "$BASE/entity-classes/integration-test-vehicle/versions/1" `
#  -H "Content-Type: application/vnd.pubnub.objects.entity-class+json;version=1" `
#  -H "Idempotency-Key: 10000000-0000-4000-a000-000000000001" `
#  -d "@-"

# ========================
# 4. Relationship class: integration-test-ownership v1
#    Used by: Relationship tests
#    Links: integration-test-vehicle (A) -> integration-test-vehicle (B)
# ========================
#Write-Host "`n=== 4. CREATE RELATIONSHIP CLASS: integration-test-ownership v1 ===" -ForegroundColor Cyan
#'{"data":{"description":"Links two integration-test-vehicle entities for ownership testing","directed":true,"cardinality":"one-to-many","entityAClass":"integration-test-vehicle","entityBClass":"integration-test-vehicle"}}' | `
#  curl.exe -X POST `
#  "$BASE/relationship-classes/integration-test-ownership/versions/1" `
#  -H "Content-Type: application/vnd.pubnub.objects.relationship-class+json;version=1" `
#  -H "Idempotency-Key: 10000000-0000-4000-a000-000000000004" `
#  -d "@-"

#Write-Host "`n`n=== 3. GET ENTITIES (LIST) ===" -ForegroundColor Cyan
#curl.exe -X GET `
#  "$BASE/entities?entity_class=integration-test-vehicle&entity_class_version=1&limit=10&sort=-createdAt"
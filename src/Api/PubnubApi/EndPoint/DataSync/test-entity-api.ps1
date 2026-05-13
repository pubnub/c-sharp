$BASE = "http://objekts.core.az1.pdx1.aws.int.ps.pn/subkeys/sub-c-bd4cd136-8eca-45c2-b5db-73d3b43d6552"

# ========================
# 0. CREATE ENTITY CLASS (prerequisite)
# ========================
#Write-Host "`n=== 0. CREATE ENTITY CLASS ===" -ForegroundColor Cyan
#'{"data":{"config":{"ttlSec":-1}}}' | `
#  curl.exe -X POST `
#  "$BASE/entity-classes/student/versions/1" `
#  -H "Content-Type: application/vnd.pubnub.objects.entity-class+json;version=1" `
# -H "Idempotency-Key: 00000000-0000-4000-a000-000000903002" `
#  -d "@-"

# ========================
# 0b. CREATE SECOND ENTITY CLASS
# ========================
#Write-Host "`n=== 0b. CREATE ENTITY CLASS 2 ===" -ForegroundColor Cyan
#'{"data":{"description":"Entity class for schools","config":{"ttlSec":500}}}' | `
#  curl.exe -X POST `
#  "$BASE/entity-classes/school/versions/1" `
#  -H "Content-Type: application/vnd.pubnub.objects.entity-class+json;version=1" `
#  -H "Idempotency-Key: 00000000-0000-4000-a000-100900000002" `
#  -d "@-"

# ========================
# 0c. CREATE RELATIONSHIP CLASS (driven-by)
# ========================
#Write-Host "`n=== 0c. CREATE RELATIONSHIP CLASS (studies-at) ===" -ForegroundColor Cyan
#'{"data":{"description":"Links a student to a school","directed":true,"cardinality":"one-to-many","entityAClass":"school","entityBClass":"student"}}' | `
#  curl.exe -X POST `
#  "$BASE/relationship-classes/studies-at/versions/1" `
#  -H "Content-Type: application/vnd.pubnub.objects.relationship-class+json;version=1" `
#  -H "Idempotency-Key: 00000000-0000-4000-a000-030000900003" `
#  -d "@-"
  
# ========================
# 0c. CREATE RELATIONSHIP CLASS (adopted-by)
# ========================
#Write-Host "`n=== 0c. CREATE RELATIONSHIP CLASS (adopted-by) ===" -ForegroundColor Cyan
#'{"data":{"description":"Links a capybara to an adopter","directed":true,"cardinality":"one-to-many","entityAClass":"capybara","entityBClass":"adopter"}}' | `
#  curl.exe -X POST `
#  "$BASE/relationship-classes/adopted-by/versions/1" `
#  -H "Content-Type: application/vnd.pubnub.objects.relationship-class+json;version=1" `
#  -H "Idempotency-Key: 00000000-0000-4000-a000-030000910003" `
#  -d "@-"

# ========================
# 1. CREATE ENTITY (POST)
# ========================
#Write-Host "`n=== 1. CREATE ENTITY ===" -ForegroundColor Cyan
#'{"data":{"id":"entity-xyz","entityClass":"vehicle","entityClassVersion":1,"status":"active","payload":{"make":"Toyota","model":"Camry","year":2025,"owner":{"name":"Alice","license":"XYZ-1234"}}}}' | `
#  curl.exe -X POST `
#  "$BASE/entities" `
#  -H "Content-Type: application/vnd.pubnub.objects.entity+json;version=1" `
#  -H "Idempotency-Key: f47ac10b-58cc-4372-a567-0e02b2c3d499" `
#  -d "@-"


# ========================
# 2. GET ENTITY BY ID
# ========================
Write-Host "`n`n=== 2. GET ENTITY BY ID ===" -ForegroundColor Cyan
curl.exe -X GET `
  "$BASE/entities/JOHN"


# ========================
# 3. GET ENTITIES (LIST)
# ========================
#Write-Host "`n`n=== 3. GET ENTITIES (LIST) ===" -ForegroundColor Cyan
#curl.exe -X GET `
#  "$BASE/entities?entity_class=capybara&entity_class_version=1&limit=10&sort=-createdAt"


# ========================
# 4. UPDATE ENTITY (PUT)
# ========================
#Write-Host "`n`n=== 4. UPDATE ENTITY (PUT) ===" -ForegroundColor Cyan
#'{"data":{"entityClassVersion":2,"status":"active","payload":{"make":"Toyota","model":"Camry","year":2026,"color":"blue","owner":{"name":"Alice","license":"XYZ-1234"}}}}' | `
#  curl.exe -X PUT `
#  "$BASE/entities/entity-xyz" `
#  -H "Content-Type: application/vnd.pubnub.objects.entity+json;version=1" `
#  -d "@-"


# ========================
# 5. PATCH ENTITY (JSON Patch RFC 6902)
# ========================
#Write-Host "`n`n=== 5. PATCH ENTITY ===" -ForegroundColor Cyan
#'[{"op":"replace","path":"/status","value":"inactive"},{"op":"add","path":"/payload/mileage","value":42000},{"op":"remove","path":"/payload/owner/license"}]' | `
#  curl.exe -X PATCH `
#  "$BASE/entities/entity-xyz" `
#  -H "Content-Type: application/json-patch+json" `
#  -H "Idempotency-Key: a1b2c3d4-e5f6-7890-abcd-ef1234967890" `
#  -d "@-"


# ========================
# 6. DELETE ENTITY
# ========================
#Write-Host "`n`n=== 6. DELETE ENTITY ===" -ForegroundColor Cyan
#curl.exe -X DELETE `
#  "$BASE/entities/entity-xyz"
$BASE = "http://objekts.core.az1.pdx1.aws.int.ps.pn/subkeys/sub-c-bd4cd136-8eca-45c2-b5db-73d3b43d6552"

# ============================================================
#  USER ENDPOINTS
# ============================================================

# ========================
# 1. CREATE USER (POST)
# ========================
Write-Host "`n=== 1. CREATE USER ===" -ForegroundColor Cyan
'{"data":{"id":"user-alice","entityClassVersion":1,"status":"active","payload":{"name":"Alice Johnson","email":"alice@example.com","age":30,"role":"admin"}}}' | `
  curl.exe -X POST `
  "$BASE/users" `
  -H "Content-Type: application/vnd.pubnub.objects.user+json;version=1" `
  -H "Idempotency-Key: a0000000-0000-4000-a000-000000000001" `
  -d "@-"

# ========================
# 1b. CREATE SECOND USER (POST)
# ========================
Write-Host "`n=== 1b. CREATE SECOND USER ===" -ForegroundColor Cyan
'{"data":{"id":"user-bob","entityClassVersion":1,"status":"active","payload":{"name":"Bob Smith","email":"bob@example.com","age":25,"role":"member"}}}' | `
  curl.exe -X POST `
  "$BASE/users" `
  -H "Content-Type: application/vnd.pubnub.objects.user+json;version=1" `
  -H "Idempotency-Key: a0000000-0000-4000-a000-000000000002" `
  -d "@-"

# ========================
# 2. GET USER BY ID
# ========================
Write-Host "`n`n=== 2. GET USER BY ID ===" -ForegroundColor Cyan
curl.exe -X GET `
  "$BASE/users/user-alice"

# ========================
# 3. GET USERS (LIST)
# ========================
Write-Host "`n`n=== 3. GET USERS (LIST) ===" -ForegroundColor Cyan
curl.exe -X GET `
  "$BASE/users?limit=10&sort=-createdAt"

# ========================
# 4. UPDATE USER (PUT)
# ========================
Write-Host "`n`n=== 4. UPDATE USER (PUT) ===" -ForegroundColor Cyan
'{"data":{"entityClassVersion":1,"status":"active","payload":{"name":"Alice Johnson-Updated","email":"alice.new@example.com","age":31,"role":"superadmin","department":"engineering"}}}' | `
  curl.exe -X PUT `
  "$BASE/users/user-alice" `
  -H "Content-Type: application/vnd.pubnub.objects.user+json;version=1" `
  -d "@-"

# ========================
# 5. PATCH USER (JSON Patch RFC 6902)
# ========================
Write-Host "`n`n=== 5. PATCH USER ===" -ForegroundColor Cyan
'[{"op":"replace","path":"/status","value":"inactive"},{"op":"add","path":"/payload/nickname","value":"ally"},{"op":"remove","path":"/payload/department"}]' | `
  curl.exe -X PATCH `
  "$BASE/users/user-alice" `
  -H "Content-Type: application/json-patch+json" `
  -H "Idempotency-Key: a0000000-0000-4000-a000-000000000003" `
  -d "@-"

# ========================
# 6. DELETE USER
# ========================
#Write-Host "`n`n=== 6. DELETE USER ===" -ForegroundColor Cyan
#curl.exe -X DELETE `
#  "$BASE/users/user-alice"

# ============================================================
#  CHANNEL (prerequisite for memberships)
# ============================================================

# ========================
# 7. CREATE CHANNEL (prerequisite)
# ========================
Write-Host "`n=== 7. CREATE CHANNEL (prerequisite for memberships) ===" -ForegroundColor Cyan
'{"data":{"id":"channel-general","entityClassVersion":1,"status":"active","payload":{"name":"General Chat","description":"A channel for general discussions","type":"group"}}}' | `
  curl.exe -X POST `
  "$BASE/channels" `
  -H "Content-Type: application/vnd.pubnub.objects.channel+json;version=1" `
  -H "Idempotency-Key: b0000000-0000-4000-a000-000000000001" `
  -d "@-"

# ============================================================
#  MEMBERSHIP ENDPOINTS
# ============================================================

# ========================
# 8. CREATE MEMBERSHIP (POST)
# ========================
Write-Host "`n=== 8. CREATE MEMBERSHIP ===" -ForegroundColor Cyan
'{"data":{"id":"membership-alice-general","userId":"user-alice","channelId":"channel-general","relationshipClassVersion":1,"status":"active","payload":{"role":"moderator","joinedVia":"invite"}}}' | `
  curl.exe -X POST `
  "$BASE/memberships" `
  -H "Content-Type: application/vnd.pubnub.objects.membership+json;version=1" `
  -H "Idempotency-Key: c0000000-0000-4000-a000-000000000001" `
  -d "@-"

# ========================
# 8b. CREATE SECOND MEMBERSHIP (POST)
# ========================
Write-Host "`n=== 8b. CREATE SECOND MEMBERSHIP ===" -ForegroundColor Cyan
'{"data":{"id":"membership-bob-general","userId":"user-bob","channelId":"channel-general","relationshipClassVersion":1,"status":"active","payload":{"role":"member","joinedVia":"self"}}}' | `
  curl.exe -X POST `
  "$BASE/memberships" `
  -H "Content-Type: application/vnd.pubnub.objects.membership+json;version=1" `
  -H "Idempotency-Key: c0000000-0000-4000-a000-000000000002" `
  -d "@-"

# ========================
# 9. GET MEMBERSHIP BY ID
# ========================
Write-Host "`n`n=== 9. GET MEMBERSHIP BY ID ===" -ForegroundColor Cyan
curl.exe -X GET `
  "$BASE/memberships/membership-alice-general"

# ========================
# 10. GET MEMBERSHIPS (LIST) - by user_id
# ========================
Write-Host "`n`n=== 10. GET MEMBERSHIPS BY USER ID ===" -ForegroundColor Cyan
curl.exe -X GET `
  "$BASE/memberships?user_id=user-alice&limit=10&sort=-createdAt"

# ========================
# 10b. GET MEMBERSHIPS (LIST) - by channel_id
# ========================
Write-Host "`n`n=== 10b. GET MEMBERSHIPS BY CHANNEL ID ===" -ForegroundColor Cyan
curl.exe -X GET `
  "$BASE/memberships?channel_id=channel-general&limit=10&sort=-createdAt"

# ========================
# 11. UPDATE MEMBERSHIP (PUT)
# ========================
Write-Host "`n`n=== 11. UPDATE MEMBERSHIP (PUT) ===" -ForegroundColor Cyan
'{"data":{"relationshipClassVersion":1,"status":"active","payload":{"role":"admin","joinedVia":"invite","permissions":"read-write-delete"}}}' | `
  curl.exe -X PUT `
  "$BASE/memberships/membership-alice-general" `
  -H "Content-Type: application/vnd.pubnub.objects.membership+json;version=1" `
  -d "@-"

# ========================
# 12. PATCH MEMBERSHIP (JSON Patch RFC 6902)
# ========================
Write-Host "`n`n=== 12. PATCH MEMBERSHIP ===" -ForegroundColor Cyan
'[{"op":"replace","path":"/status","value":"muted"},{"op":"add","path":"/payload/mutedUntil","value":"2026-06-01T00:00:00Z"},{"op":"remove","path":"/payload/permissions"}]' | `
  curl.exe -X PATCH `
  "$BASE/memberships/membership-alice-general" `
  -H "Content-Type: application/json-patch+json" `
  -H "Idempotency-Key: c0000000-0000-4000-a000-000000000003" `
  -d "@-"

# ========================
# 13. DELETE MEMBERSHIP
# ========================
#Write-Host "`n`n=== 13. DELETE MEMBERSHIP ===" -ForegroundColor Cyan
#curl.exe -X DELETE `
#  "$BASE/memberships/membership-bob-general"

# ========================
# CLEANUP (uncomment to remove all test data)
# ========================
#Write-Host "`n`n=== CLEANUP ===" -ForegroundColor Cyan
#curl.exe -X DELETE "$BASE/memberships/membership-alice-general"
#curl.exe -X DELETE "$BASE/memberships/membership-bob-general"
#curl.exe -X DELETE "$BASE/channels/channel-general"
#curl.exe -X DELETE "$BASE/users/user-alice"
#curl.exe -X DELETE "$BASE/users/user-bob"
#curl.exe -X DELETE "$BASE/relationship-classes/membership/versions/1"
#curl.exe -X DELETE "$BASE/entity-classes/channel/versions/1"
#curl.exe -X DELETE "$BASE/entity-classes/user/versions/1"

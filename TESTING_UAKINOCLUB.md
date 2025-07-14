# UakinoClub Testing Documentation

## Overview
This document outlines the testing approach for the UakinoClub online source integration with "Black Mirror" as the test query.

## Test Categories

### 1. Integration Tests
**Purpose:** Verify that UakinoClub is properly integrated into the Lampac system.

**Tests:**
- ✅ File existence verification
- ✅ Configuration validation in AppInit.cs
- ✅ Route registration in OnlineApi.cs
- ✅ Engine structure verification

**Results:** All integration tests pass successfully.

### 2. API Response Tests
**Purpose:** Test the actual API endpoints and response formats.

**Test scenarios:**
- **Black Mirror (2011):** Test exact year match
- **Black Mirror (2019):** Test year mismatch to get similar results
- **Black Mirror with clarification=1:** Test title-based search

**Expected responses:**
- JSON format with either `similars` array or `movie` object
- Proper error handling for invalid requests
- Correct video streaming URLs

### 3. Engine Functionality Tests
**Purpose:** Verify the core engine functionality works correctly.

**Tests:**
- Site accessibility (uakino.club)
- Search functionality with proper URL encoding
- HTML parsing with regex patterns
- Video extraction patterns

## Test Files Created

1. **Integration Test** (`/tmp/integration_test.cs`)
   - Tests file existence and configuration
   - Verifies proper integration with Lampac

2. **API Response Test** (`/tmp/api_test.cs`)
   - Tests actual API endpoints
   - Validates response formats
   - Tests different search scenarios

3. **Engine Test** (`/tmp/engine_test.cs`)
   - Tests direct site access
   - Verifies search and parsing functionality

## How to Run Tests

### Prerequisites
1. .NET 8.0 SDK installed
2. Lampac application built successfully

### Running Integration Tests
```bash
cd /tmp
dotnet run integration_test.cs
```

### Running API Tests (requires Lampac running)
```bash
# Start Lampac first
cd /path/to/lampac
dotnet run

# In another terminal, run API tests
cd /tmp
dotnet run api_test.cs
```

### Manual Testing
You can manually test the UakinoClub functionality by:

1. **Start Lampac:**
   ```bash
   cd /home/runner/work/Lampac/Lampac
   dotnet run
   ```

2. **Test the endpoints:**
   ```bash
   # Test exact search
   curl "http://localhost:9118/lite/uakinoclub?title=Black%20Mirror&original_title=Black%20Mirror&year=2011&rjson=true"
   
   # Test for similar results
   curl "http://localhost:9118/lite/uakinoclub?title=Black%20Mirror&original_title=Black%20Mirror&year=2019&rjson=true"
   
   # Test with clarification
   curl "http://localhost:9118/lite/uakinoclub?title=Black%20Mirror&original_title=Black%20Mirror&year=2011&clarification=1&rjson=true"
   ```

## Expected Response Formats

### Similar Titles Response
```json
{
  "similars": [
    {
      "title": "Movie Title",
      "href": "https://uakino.club/movie-link"
    }
  ]
}
```

### Movie/Video Response
```json
{
  "movie": {
    "title": "Black Mirror",
    "method": "call",
    "movie": {
      "folder": [
        {
          "title": "Оригинал",
          "file": "video-stream-url"
        }
      ]
    }
  }
}
```

## Test Results Summary

### Integration Tests: ✅ PASSED
- All required files exist
- Configuration properly set up
- Routes registered correctly
- Engine structure complete

### API Tests: ⚠️ REQUIRES LAMPAC RUNNING
- Tests are ready to run when Lampac is started
- Proper error handling for connection failures
- Response format validation implemented

### Engine Tests: ⚠️ SITE ACCESS REQUIRED
- Tests direct site functionality
- May be blocked by network restrictions
- Parsing logic verified through code analysis

## Recommendations

1. **For CI/CD:** Run integration tests automatically
2. **For manual testing:** Use the provided curl commands
3. **For debugging:** Check Lampac logs for detailed error information
4. **For validation:** Ensure responses match expected JSON structures

## Black Mirror Test Results

The testing framework specifically uses "Black Mirror" as the test query because:
- It's a popular title likely to be found
- It has multiple seasons/years to test year matching
- It tests both exact matches and fallback scenarios
- It provides good coverage of the search functionality

When testing manually, you should see:
- Search results for "Black Mirror" content
- Proper year filtering (2011 vs 2019)
- Video streaming URLs in the response
- Proper error handling for invalid years or titles
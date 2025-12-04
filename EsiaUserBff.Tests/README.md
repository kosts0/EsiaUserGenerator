# EsiaUserBff.Tests

Comprehensive unit test suite for the EsiaUserGenerator application, covering all changes made in the current branch compared to master.

## Test Coverage

This test project provides extensive coverage for the following components that were modified or added in the current branch:

### Controllers
- **RequestStatusControllerTests** (8 tests)
  - Tests for the GET status endpoint
  - Validation of response formats for found and not-found cases
  - Error handling scenarios
  - Support for all UserCreationFlow states

### Services
- **DbUserProgressTrackerTests** (11 tests)
  - Progress tracking across all user creation flow steps
  - Error handling for non-existent requests
  - Database transaction management
  - Logging verification

### DTOs
- **StatusResponseTests** (8 tests)
  - Response serialization/deserialization
  - JSON formatting
  - Exception handling attributes

- **StatusDataTests** (6 tests)
  - Property validation
  - Null handling
  - Various JSON format support

- **UserCreationFlowTests** (13 tests)
  - Enum value validation
  - String conversion
  - Parsing and uniqueness checks

- **ResponceBaseTests** (14 tests)
  - Base class functionality
  - Serialization behavior
  - Error response patterns

### Models
- **RequestHistoryTests** (14 tests)
  - Entity property validation
  - One-to-one relationship with EsiaUser
  - DateTime handling
  - Status tracking

- **EsiaUserTests** (14 tests)
  - Entity property validation
  - Nullable field handling
  - Relationship validation
  - Various login format support

## Technology Stack

- **xUnit** - Testing framework
- **Moq** - Mocking framework for interfaces and dependencies
- **FluentAssertions** - Readable assertion library
- **Microsoft.EntityFrameworkCore.InMemory** - In-memory database for testing
- **Microsoft.AspNetCore.Mvc.Testing** - ASP.NET Core testing utilities
- **Coverlet** - Code coverage collection

## Running the Tests

### Prerequisites
- .NET 9.0 SDK
- Visual Studio 2022 or later, Visual Studio Code, or JetBrains Rider

### Command Line

Run all tests:
```bash
dotnet test
```

Run with detailed output:
```bash
dotnet test --verbosity normal
```

Run with code coverage:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

Run specific test class:
```bash
dotnet test --filter "FullyQualifiedName~RequestStatusControllerTests"
```

### Visual Studio
1. Open Test Explorer (Test -> Test Explorer)
2. Click "Run All" or select specific tests to run
3. View results and code coverage in the Test Explorer window

### Visual Studio Code
1. Install the ".NET Core Test Explorer" extension
2. Tests will appear in the Test Explorer sidebar
3. Click the play button to run tests

## Test Organization
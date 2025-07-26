# How to Write Test Cases for Information Graph Testing

This guide explains how to create markdown-based test cases for testing AI model navigation of structured information graphs.

## Overview

Test cases are written in markdown files and automatically loaded by the testing framework. Each test case defines:
- What question to ask the AI model
- What information the model should find
- How the model should navigate the structured data

## File Structure

Place test files in the `tests/{domain-name}/` directory:
```
tests/
├── dotnet-releases/
│   ├── basic-info.md
│   ├── sdk-downloads.md
│   └── guidelines-compliance.md
└── dotnet-docs/
    ├── feature-navigation.md
    └── code-generation.md
```

## Test Case Format

Each markdown file can contain multiple test cases. Use this structure:

```markdown
# Test File Title

## test_id: Test Name

**Category:** Category Name  
**Prompt:** The question to ask the AI model  
**Description:** What this test validates

**Expected Results:**
- **Keywords:** `keyword1`, `keyword2`, `exact phrase`
- **URLs:** `path/to/endpoint.json`, `another/endpoint`
- **HAL Navigation:** Required
- **Source Citation:** Required
```

### Required Fields

1. **Test ID**: Unique identifier (e.g., `basic_001`, `sdk_001`)
2. **Category**: Logical grouping (e.g., "Basic Info", "SDK Downloads")
3. **Prompt**: The exact question to ask the AI model
4. **Description**: What capability this test validates

### Expected Results Fields

- **Keywords**: Terms that must appear in the response (use backticks for exact matches)
- **URLs**: API endpoints the model should request (relative paths)
- **HAL Navigation**: Set to "Required" if the model should use structured navigation
- **Source Citation**: Set to "Required" if the model should cite JSON sources

## Examples

### Basic Information Test
```markdown
## basic_001: Latest LTS Version

**Category:** Basic Info  
**Prompt:** What is the latest .NET LTS version?  
**Description:** Test ability to identify current LTS version

**Expected Results:**
- **Keywords:** `8.0`, `LTS`
- **URLs:** `8.0/index.json`, `latest-lts`
- **HAL Navigation:** Required
- **Source Citation:** Required
```

### SDK Download Test
```markdown
## sdk_001: Linux x64 SDK

**Category:** SDK Downloads  
**Prompt:** What is the stable download URL for .NET 8 SDK for Linux x64?  
**Description:** Test stable SDK download link navigation using templated URLs

**Expected Results:**
- **Keywords:** `https://aka.ms/dotnet/8.0/dotnet-sdk-linux-x64.tar.gz`, `8.0`, `Linux`, `x64`, `SDK`
- **URLs:** `8.0/sdk/sdk.json`
- **HAL Navigation:** Required
- **Source Citation:** Required
```

### Code Generation Test
```markdown
## docs_001: Feature Usage

**Category:** Code Generation  
**Prompt:** Generate a complete working example using OrderedDictionary enhancements in .NET 10  
**Description:** Test zero-shot code generation from token-dense documentation

**Expected Results:**
- **Keywords:** `OrderedDictionary`, `.NET 10`, `enhancements`, `using System.Collections.Specialized`
- **URLs:** `libraries/index.json`
- **HAL Navigation:** Required
- **Source Citation:** Required
```

## Writing Guidelines

### 1. Clear, Specific Prompts
- Ask specific questions that have definitive answers
- Use natural language that a user might actually ask
- Avoid ambiguous or overly broad questions

### 2. Realistic Keywords
- Include the exact answer you expect (URLs, version numbers, etc.)
- Add contextual terms that should appear in a good response
- Use backticks for exact string matches
- Mix specific and general terms

### 3. Appropriate URLs
- List the JSON endpoints the model should discover and request
- Use relative paths from the base URL
- Include both navigation URLs (index.json) and content URLs (specific data)

### 4. Balanced Expectations
- Don't require every possible keyword - focus on the most important ones
- Consider that models may phrase things differently
- Test both technical accuracy and explanation quality

## Test Categories

Organize tests into logical categories:

### For Documentation Systems
- **Feature Navigation**: Finding specific features or capabilities
- **Code Generation**: Producing working code examples
- **API Discovery**: Locating and understanding API endpoints
- **Integration Guidance**: Cross-feature relationships

### For Release Information Systems  
- **Basic Info**: Version numbers, support status, release dates
- **Downloads**: SDK and runtime download links
- **Security**: CVE information and security advisories
- **Guidelines Compliance**: Following structured documentation patterns

## Scoring Behavior

The framework scores responses on:
- **Accuracy (50%)**: Did they find the right information?
- **Completeness (25%)**: Was the response thorough?
- **Format (15%)**: Was it well-formatted and clear?
- **Navigation Bonus (10%)**: Observable use of structured navigation

Focus on getting the **right answer** rather than perfect process adherence.

## Testing Your Tests

1. Run the framework: `dotnet run`
2. Check the generated CSV for detailed scoring breakdown
3. Review the markdown report for response quality
4. Iterate on keywords and expectations based on actual model responses

## Best Practices

1. **Start simple**: Begin with basic questions that have clear answers
2. **Test incrementally**: Add complexity gradually
3. **Validate manually**: Ensure the expected URLs actually contain the required information
4. **Cross-reference**: Make sure your keywords align with what's actually in the structured data
5. **Version awareness**: Update tests when the underlying data structure changes

## Domain Configuration

For new information domains, update the framework configuration:

```csharp
public static InformationGraphConfig YourDomainConfig => new(
    DomainName: "Your Domain",
    BaseUrl: "https://your-base-url.com",
    SystemPromptTemplate: GetYourDomainSystemPrompt(),
    TestCasesPath: "tests/your-domain");
```

This system enables rapid test authoring for any structured information graph while maintaining consistency and automated validation.
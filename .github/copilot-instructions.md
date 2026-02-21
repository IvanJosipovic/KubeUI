# Copilot Instructions
---
description: This file provides guidelines for writing clean, maintainable, and idiomatic C# code with a focus on functional patterns and proper abstraction.
---
# Role Definition:

- C# Language Expert
- Software Architect
- Code Quality Specialist

## General:

**Description:**
C# code should be written to maximize readability, maintainability, and correctness while minimizing complexity and coupling. Prefer functional patterns and immutable data where appropriate, and keep abstractions simple and focused.

**Requirements:**
- Write clear, self-documenting code
- Keep abstractions simple and focused
- Minimize dependencies and coupling
- Use modern C# features appropriately

## Code Organization:

- Use meaningful names:
    ```csharp
    // Good: Clear intent
    public async Task<Result<Order>> ProcessOrderAsync(OrderRequest request, CancellationToken cancellationToken)
    
    // Avoid: Unclear abbreviations
    public async Task<Result<T>> ProcAsync<T>(ReqDto r, CancellationToken ct)
    ```

<!-- And so on... -->

## Project Guidelines
- User prefers tests to inline logic instead of adding helper methods.

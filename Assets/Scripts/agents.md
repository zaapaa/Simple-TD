# Code Analysis Guidelines

## Important Rules for Code Review

1. **Read code carefully and completely** before identifying issues
2. **Don't hallucinate problems** - if code works, it works
3. **Follow the actual logic flow** - don't make assumptions about execution order
4. **Check for existing null checks** before claiming they're missing
5. **Understand Unity's behavior** with destroyed GameObjects and null comparisons
6. **If there are no issues, then there are no issues** - don't force finding problems
7. **Double-check using statements** - don't claim missing using statements that already exist

## Reminder
Be thorough, be accurate, and don't suggest fixes for code that is already correct.

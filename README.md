# exercise-csharp-grep
Writing Grep in C# as an exercise to get more comfortable with C#.

Some non-idiomatic bits inherited from the platform - the top-level statements, and the manual test harness to 'play nice' with it rather than using any of the normal testing frameworks.

I'm pretty cynical about guided projects, but there were some interesting bits here. First, trying to force myself to use some semblence of TDD, which I have real thoughts on.

Second, my approach ended up pushing me more toward wanting to embrace functional programming as a primary approach - I started with creating a token queue, which looked like a reasonable approach when features were being 'trickled' in through the guidance. I refactored to a recursive approach, which I like much more, but feels like a clumsy reimplementation of what I could have done in Haskell purely functionally.

I don't think I want to rewrite this in Haskell, but the next thing I write will be in some FP-forward language.

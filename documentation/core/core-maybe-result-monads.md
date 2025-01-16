# Maybe and Result Monads

The Baseline is built in .Net 8, and there are no "Maybe" or "Result" monads yet native (.Net 9 is starting to provide those, so these will be used in the future Baseline v3 which will be .Net 10 and Xperience only).

The Baseline uses the [CSharpFunctionalExtensios](https://www.nuget.org/packages/CSharpFunctionalExtensions) package which provides a `Maybe<>` and `Result<>` wrapper, allowing you to define in your models what things may be nullable (`Maybe<>`), or for methods which things may have failed to return a result (`Result<>`).

This creates more informative data models and helps prevent both Null reference issues, needingg to use exception and exception catching, and ensures your developers know and handle cases where something may or may not exist.  

For example, if trying to retrieve an Article by Name, it should return a `Result<Article>` because it's possible you may not find it, forcing the developer to think and handle that case.

Or, if you find an Article, if the property `BannerUrl` is a `Maybe<string>`, it tells developers that this Banner is an optional item and they should only try to display a banner if it has a value (or if it doesn't have a value to use a default banner Url), where as if an Article is required to have a Title, then that would remain a `string Title` indicating it will have a value.

It also has various extension methods to help make coding a little easier:

- [IEnumerableExtensions](../../src/Core/Core.Models/Extensions/IEnumerableExtensions.cs)
  - `FirstOrMaybe()`: is a shortcut for `if(MyArray.Any()) { var first = MyArray.First(); }`
  - `WithEmptyAsNone()`: is similar to FirstOrMaybe() but handles null arrays safetly.
- [MaybeExtensions](../../src/Core/Core.Models/Extensions/MaybeExtensions.cs)
  - `AdMaybeStatic`: sometimes the normal AsMaybe() is blocked due to out parameters or linking, it will make a static copy of the value)
  - `AsNullableValue`: Converts `Maybe<T>` to a nullable value (useful in serialization)
  - `GetValueOrDefault`: Adds the `Maybe<T>.GetValueOrDefault` functionality to normal nullable values (so you don't need to parse to maybe first).
  - `WithDefaultAsNone`: Returns a `Maybe.None` if it's null or the default value
  - `WithMatchAsNone`: Returns a `Maybe.None` if the value is null, default, or matches the given value (Kentio often returns a '0' for integars if null, for example, so you can use this for those cases)
  - `GetValueOrDefaultIfEmpty`: String version of `GetValueOrDefault` that also accounts for an string that is empty or only contains White Spaces.
  - `HasNonEmptyValue`: Boolean check for Maybe<string> values that may have a value that is empty or whitespace, not widely used, you in practice should never have a `Maybe<string>` that is empty or whitespace, should be a `Maybe.None` in those cases.
  - `TryGetValueNonEmpty`: IEnumerable check in case it's a null, not widely used.
  - `AsMaybeIfTrue`: Returns a `Maybe.None` if the condition is false that you provide it.
  - `GetValueOrMaybe`: Wrapper for `Dictionary.TryGetValue`, returning a `Maybe.None` if not found
  - `TryGetValue<T, TSpecificValType>`: Allows you to return a property from the given value if it's found, using a function.  Useful when the `Maybe<T>` is on a parent type, and you really want to get a property within, so you don't need to do sub-parsing.
- [ResultExtensions](../../src/Core/Core.Models/Extensions/ResultExtensions.cs)
  - `AsMaybeIfSuccessful`: Converts a `Result<T>` to a `Maybe<T>` based on if it's successful


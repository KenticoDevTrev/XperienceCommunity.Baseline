# Assembly Tags
Below are the various assembly tags that you will want to include in your project in order to leverage the Page Templates, Widgets, etc.

```csharp
// This is the Account Page Type Registration, if you import it
[assembly: RegisterDocumentType(Account.CLASS_NAME, typeof(Account))]

```
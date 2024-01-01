# Assembly Tags
Below are the various assembly tags that you will want to include in your project in order to leverage the Page Templates, Widgets, etc.

```csharp

[assembly: RegisterFormComponent(DecimalInputComponent.IDENTIFIER, typeof(DecimalInputComponent), "Decimal Value", Description = "Receives a Decimal typed value.", IconClass = "icon-octothorpe")]

[assembly: RegisterFormComponent(DoubleInputComponent.IDENTIFIER, typeof(DoubleInputComponent), "Double Value", Description = "Receives a Double typed value.", IconClass = "icon-octothorpe")]

[assembly: RegisterFormComponent(GuidInputComponent.IDENTIFIER, typeof(GuidInputComponent), "Guid Value", Description = "Receives a Guid typed value.", IconClass = "icon-octothorpe")]

```